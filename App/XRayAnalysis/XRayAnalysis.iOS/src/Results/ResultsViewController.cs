// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Globalization;
using CoreGraphics;
using Foundation;
using XRayAnalysis.Results;
using XRayAnalysis.Service;
using XRayAnalysis.Service.AI.ChestXRay;
using XRayAnalysis.Service.Analytics;
using ImageIO;
using UIKit;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using XRayAnalysis.iOS.Service.NavigationControllerUtil;

namespace XRayAnalysis.iOS.Results
{
    /// <summary>
    /// ViewController for the results page. Displays the results generated from the offline AI model. 
    /// </summary>
    public partial class ResultsViewController : UIViewController
    {
        private readonly int borderWidth = 1; //border width for the result

        // Set of colors to be used for cell tableViews 
        private readonly SKColor[] cellColorset = {
            Constants.DarkPurple.ToSKColor(),
            Constants.Pink.ToSKColor(),
            Constants.Blue.ToSKColor(),
            Constants.Turquoise.ToSKColor()
        };

        private UIImageView imageView;
        private UIImageView originalImageView;

        /// <summary>
        /// The width and height at which to load the x-ray image
        /// 
        /// NOTE: The image must be loaded at a fixed resolution to
        /// ensure a high CAM generation and overlay speed
        /// </summary>
        public const int LoadImageWidth = 1024;
        public const int LoadImageHeight = 1024;

        private const string DefaultFileStringForCamera = "From Camera";
        private const string DefaultNoDateDisplay = "Not Found";
        private const string NavBarTitle = "Sample Model In Use - Results Not Valid";

        public UIImage InputImage { get; set; }
        public NSUrl ImageUrl { get; set; }

        private ConditionResult[] conditionResults;

        private ChestXRayAIClient aiClient;
        private CancellationTokenSource cancellationTokenSource;
        private UIImage blendedImage;

        public UITableView primaryTableView;
        public UITableView secondaryTableView;

        private bool seeMoreToggled = false;

        public Action<int> OnClick;

        private List<UILabel> originalLabels;

        public ResultsViewController() : base("ResultsViewController", null)
        {
        }

        public ResultsViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AnalyticsService.TrackEvent(AnalyticsService.Event.ResultsPageViewed);

            //Programmatically add a back button and an arrow
            UIImage backArrowImage = UIImage.FromBundle("back-arrow");
            UIButton backButton = new UIButton(UIButtonType.Custom);

            backButton.SetImage(backArrowImage, UIControlState.Normal);
            backButton.SetTitle("Back", UIControlState.Normal);
            backButton.ImageEdgeInsets = new UIEdgeInsets(0.0f, -12.5f, 0.0f, 0.0f);
            backButton.AddTarget((sender, e) =>
            {
                AnalyticsService.TrackEvent(AnalyticsService.Event.ReturnBackToCroppingPage);
                this.NavigationController?.PopViewController(true);
            }, UIControlEvent.TouchUpInside);

            this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(backButton);

            UIButton newSession = new UIButton(UIButtonType.Custom);

            newSession.SetTitle("New Session", UIControlState.Normal);
            newSession.AddTarget((sender, e) =>
            {
                AnalyticsService.TrackEvent(AnalyticsService.Event.ReturnBackToImageInputPage);
                this.NavigationController?.PopToRootViewController(true);
            }, UIControlEvent.TouchUpInside);

            this.NavigationItem.RightBarButtonItem = new UIBarButtonItem(newSession);

            // Set nav bar attributes
            NavigationControllerUtil.SetNavigationBarAttributes(this.NavigationController.NavigationBar);
            NavigationControllerUtil.SetNavigationTitle(this.NavigationItem, NavBarTitle);

            InputImage = InputImage.Scale(new CoreGraphics.CGSize(LoadImageWidth, LoadImageHeight));

            // Initialize UIImageView
            imageView = new UIImageView();
            imageView.Frame = new CGRect(0, 0, InputImage.Size.Width, InputImage.Size.Height);
            imageView.Image = InputImage;
            imageView.UserInteractionEnabled = true;

            // Initialize original imageview
            originalImageView = new UIImageView();
            originalImageView.Frame = new CGRect(0, 0, InputImage.Size.Width, InputImage.Size.Height);
            originalImageView.Image = InputImage;
            originalImageView.UserInteractionEnabled = true;

            // Initialize meta-data display
            string filename;
            DateTime? time;
            try
            {
                filename = GetFileString(ImageUrl, DefaultFileStringForCamera);
                time = GetDate(ImageUrl);
            }
            catch(NullReferenceException ex)
            {
                filename = DefaultFileStringForCamera;
                time = DateTime.Now;
            }

            AnalyzedImageFileNameLabel.Text = filename;
            AnalyzedImageDateLabel.Text = time.HasValue ? time.Value.ToShortDateString() : DefaultNoDateDisplay;

            OriginalImageFileNameLabel.Text = filename;
            OriginalImageDateLabel.Text = time.HasValue ? time.Value.ToShortDateString() : DefaultNoDateDisplay;

            // Add all original labels to List<UILabel>
            originalLabels = new List<UILabel>();
            originalLabels.Add(OriginalImageDateLabel);
            originalLabels.Add(OriginalImageDateHeaderLabel);
            originalLabels.Add(OriginalImageFileNameLabel);
            originalLabels.Add(OriginalImageFileNameHeaderLabel);

            // Toggle accessibilty off for all original labels (they are initially hidden)
            EnableVoiceOverForViews(originalLabels.ToArray(), false);

            SetDisplayBorders();

            // Retrieve the shared AI Client that was loaded by the AppDelegate
            aiClient = ((AppDelegate)UIApplication.SharedApplication.Delegate).AIClient;
            System.Diagnostics.Debug.WriteLine("AI Client retrieved from AppDelegate");
        }

        /// <summary>
        /// Initializes the condition result set. Called from <see cref="AnalysisViewController.cs"/>
        /// </summary>
        /// <param name="cams">Cams.</param>
        /// <param name="scores">Scores.</param>
        public void InitResultSet(CAM[] cams, ScoreOutput[] scores)
        {
            this.conditionResults = ResultsHelper.BuildConditionResultArray((ChestCondition[])Enum.GetValues(typeof(ChestCondition)), scores, cams, cellColorset);
        }

        /// <summary>
        /// Called after the UIViewController.View is added to the view hierarchy.
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            this.NavigationController.NavigationBar.AccessibilityLabel = Constants.NavigationBarAccessibilityLabel_ResultsPage;

            SetImageScrollView();

            //table views to be initialized after the views have loaded
            //this is so that the tableview heights can be calculated after
            //all views are on the screen
            SetTableViews();
        }

        /// <summary>
        /// This method is called prior to the removal of the UIViewthat is this 
        /// UIViewController’s UIViewController.View from the display UIView hierarchy.
        /// </summary>
        /// <param name="animated">If set to <c>true</c> animated.</param>
        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            cancellationTokenSource?.Cancel();

            // dispose of unneeded assets
            InputImage.Dispose();
            imageView.Dispose();
            originalImageView.Dispose();
            blendedImage?.Dispose();
        }

        /// <summary>
        /// Input is a double, output is a string of the same double 
        /// with 1 decimal place and a appended '%' character
        /// </summary>
        private string FormatOutputPercentages(double score)
        {
            return (score * 100).ToString("N1") + "%";
        }

        /// <summary>
        /// Initializes the image ScrollView
        /// </summary>
        private void SetImageScrollView()
        {
            // Initialize scrollview
            ImageScrollView.ContentSize = InputImage.Size;
            ImageScrollView.AddSubview(imageView);
            ImageScrollView.ViewForZoomingInScrollView += (UIScrollView sv) => { return imageView; };
            ImageScrollView.DidZoom += (object sender, EventArgs e) =>
            {
                CenterScrollViewContents(ImageScrollView, imageView);
            };
            imageView.ContentMode = UIViewContentMode.Center;

            OriginalImageScrollView.ContentSize = InputImage.Size;
            OriginalImageScrollView.AddSubview(originalImageView);
            OriginalImageScrollView.DidZoom += (object sender, EventArgs e) =>
            {
                CenterScrollViewContents(OriginalImageScrollView, originalImageView);
            };
            OriginalImageScrollView.ViewForZoomingInScrollView += (UIScrollView sv) => { return originalImageView; };
            originalImageView.ContentMode = UIViewContentMode.Center;

            // Get Image + Frame widths and heights
            var imgWidth = InputImage.Size.Width;
            var imgHeight = InputImage.Size.Height;
            var frameWidth = ImageScrollView.Frame.Size.Width;
            var frameHeight = ImageScrollView.Frame.Size.Height;

            // Compute scale
            nfloat scaleWidth = frameWidth / imgWidth;
            nfloat scaleHeight = frameHeight / imgHeight;
            nfloat minScale = (nfloat)Math.Min(scaleWidth, scaleHeight);

            // Apply zoom contraints given the minimum scale
            ImageScrollView.MinimumZoomScale = minScale;
            ImageScrollView.MaximumZoomScale = Constants.MaximumResultsImageZoomScale;
            ImageScrollView.ZoomScale = minScale;

            OriginalImageScrollView.MinimumZoomScale = minScale;
            OriginalImageScrollView.MaximumZoomScale = Constants.MaximumResultsImageZoomScale;
            OriginalImageScrollView.ZoomScale = minScale;

            // Center the image in the scroll view
            CenterScrollViewContents(ImageScrollView, imageView);
            CenterScrollViewContents(OriginalImageScrollView, originalImageView);
        }

        /// <summary>
        /// Sets the display borders.
        /// </summary>
        private void SetDisplayBorders()
        {
            this.ButtonSeeMore.Layer.BorderWidth = borderWidth;
            this.ButtonSeeMore.Layer.BorderColor = Constants.LightGrey.CGColor;
        }

        /// <summary>
        /// Generates a blended image with a CAM and the desired overlay color
        /// </summary>
        /// <returns>The blended CAMI mage.</returns>
        /// <param name="index">Index.</param>
        /// <param name="color">Color.</param>
        private async Task<UIImage> GenerateBlendedCAMImage(CAM cam, SKColor color)
        {
            // Cancel the previous blending task if there is one
            cancellationTokenSource?.Cancel();

            cancellationTokenSource = new CancellationTokenSource();

            // Dispose of previous blended image
            blendedImage?.Dispose();

            SKBitmap colorMap = await ImageProcessorRunner.GetCAMColorMapTask(
                cam,
                color,
                (int)(InputImage.Size.Width * InputImage.CurrentScale),
                (int)(InputImage.Size.Height * InputImage.CurrentScale),
                cancellationTokenSource.Token
            );

            SKBitmap input = InputImage.ToSKBitmap();
            SKBitmap blendedSKBitmap = await ImageProcessorRunner.GetOverlayImageTask(
                input,
                colorMap,
                cancellationTokenSource.Token
            );
            // Dispose of temporary SKBitmap
            input.Dispose();

            UIImage blendedUIImage = blendedSKBitmap.ToUIImage();

            // Cleanup the intermediate bitmaps
            colorMap?.Dispose();
            blendedSKBitmap?.Dispose();

            return blendedUIImage;
        }

        /// <summary>
        /// Initializes the table views
        /// Primary table view refers to the table view to the left with the 14 conditions
        /// Secondary table view refers to the table view to the right with 2 condition
        /// The secondary table view contains the conditions located at index 1 and 2 from the conditions outputted by the model
        /// </summary>
        private void SetTableViews()
        {
            this.OnClick = HandleCellClick;

            //grab references to table view in storyboard
            primaryTableView = TableViewConditionsList;
            secondaryTableView = SecondaryTableViewConditionsList;

            nfloat secondaryTableViewHeight = secondaryTableView.Frame.Height;

            primaryTableView.RegisterNibForCellReuse(UINib.FromName(ConditionViewCell.XIBKey, NSBundle.MainBundle), ConditionViewCell.CellReuseId);
            primaryTableView.RegisterNibForCellReuse(UINib.FromName(ConditionViewCell.PrimaryXIBKey, NSBundle.MainBundle), ConditionViewCell.PrimaryCellReuseId);
            secondaryTableView.RegisterNibForCellReuse(UINib.FromName(ConditionViewCell.XIBKey, NSBundle.MainBundle), ConditionViewCell.CellReuseId);

            // Set TableView source
            primaryTableView.Source = new ConditionsTableViewSource(conditionResults, OnClick, ConditionsTableViewSource.TableType.Primary, secondaryTableViewHeight);
            primaryTableView.ReloadData();

            //NOTE: we are assuming that there will be at least 3 conditions to display
            ConditionResult[] conditionsResultsSubset = { conditionResults[1], conditionResults[2] };

            ConditionsTableViewSource secondarySource = new ConditionsTableViewSource(conditionsResultsSubset, OnClick, ConditionsTableViewSource.TableType.Secondary, secondaryTableViewHeight);

            // See more
            secondaryTableView.Source = secondarySource;
            secondaryTableView.ReloadData();

            //initially disable scrolling
            primaryTableView.ScrollEnabled = false;
            secondaryTableView.ScrollEnabled = false;

            //set borders for tableviews
            primaryTableView.Layer.BorderWidth = borderWidth;
            primaryTableView.Layer.BorderColor = Constants.LightGrey.CGColor;
            secondaryTableView.Layer.BorderWidth = borderWidth;
            secondaryTableView.Layer.BorderColor = Constants.LightGrey.CGColor;
        }

        /// <summary>
        /// Callback for when a cell is tapped from either <see cref="primaryTableView"/> or <see cref="secondaryTableView"/>
        /// </summary>
        /// <param name="index">The index of the cell</param>
        public async void HandleCellClick(int index)
        {
            ConditionResult result = conditionResults[index];

            // Get cell selected and its current highlight state
            bool curHighlightState = result.Selected;

            // Re-assign displayed image
            imageView.Image = InputImage;

            // Dispose of blended image
            blendedImage?.Dispose();

            // Set all cells to un-highlight
            foreach (ConditionResult model in conditionResults)
            {
                model.Selected = false;
            }

            result.Selected = !curHighlightState;

            // Reload all cells
            primaryTableView.ReloadData();
            secondaryTableView.ReloadData();

            // If result is not selected, no need to generate a CAM
            if (!result.Selected)
            {
                return;
            }

            // Generate blended CAM
            try
            {
                blendedImage = await GenerateBlendedCAMImage(conditionResults[index].CAM, result.HighlightColor);
                imageView.Image = blendedImage;
                imageView.ContentMode = UIViewContentMode.ScaleAspectFit;
            }
            catch (OperationCanceledException)
            {
                // Unhighlight the button
                result.Selected = false;

                // Reload all cells
                primaryTableView.ReloadData();
                secondaryTableView.ReloadData();

                System.Diagnostics.Debug.WriteLine("CAM overlay task cancelled");
            }
        }

        /// <summary>
        /// Centers the scroll view contents.
        /// </summary>
        private void CenterScrollViewContents(UIScrollView imageScrollView, UIImageView imageView)
        {
            // Get scroll view bounds and image frame
            CGSize boundsSize = imageScrollView.Bounds.Size;
            CGRect imageFrame = imageView.Frame;

            // Image is contained within horizontal bounds of scrollview
            if (imageFrame.Size.Width < boundsSize.Width)
            {
                // Center
                imageFrame.X = (boundsSize.Width - imageFrame.Size.Width) / 2;
            }
            // Image is horizontally larger than scrollview
            else
            {
                // Center to 0
                imageFrame.X = 0;
            }

            // Image is contained within verticle bounds of scrollview
            if (imageFrame.Size.Height < boundsSize.Height)
            {
                // Center
                imageFrame.Y = (boundsSize.Height - imageFrame.Size.Height) / 2;
            }
            else
            {
                // Center to 0
                imageFrame.Y = 0;
            }

            // Set imageview frame
            imageView.Frame = imageFrame;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private void EnableVoiceOverForViews(UIView[] views, bool enabled)
        {
            foreach (UIView view in views)
            {
                view.AccessibilityElementsHidden = !enabled;
            }
        }

        /// <summary>
        /// Toggle see more
        /// </summary>
        /// <param name="button">UIButton</param>
        partial void SeeMore(UIButton button)
        {
            nfloat imageViewHeight = ImageViewWrapper.Frame.Height;
            nfloat imageViewWidth = ImageViewWrapper.Frame.Width;
            nfloat deviceWidth = View.Frame.Width;
            nfloat secondaryViewHeight = secondaryTableView.Frame.Height;
            const int scaleFactor = 2;

            seeMoreToggled = !seeMoreToggled;

            //toggle button state
            ButtonSeeMore.SetTitle(seeMoreToggled ? "Hide" : "See More", UIControlState.Normal);
            ButtonSeeMore.Enabled = false;
            ButtonSeeMore.AccessibilityLabel = seeMoreToggled ? Constants.AccessibilityLabel_SeeMoreHideButton : Constants.AccessibilityLabel_SeeMoreShowButton;

            View.LayoutIfNeeded();

            primaryTableView.ScrollEnabled = seeMoreToggled;

            //force the primary tableview to scroll to the top
            primaryTableView.ScrollToRow(NSIndexPath.Create(new int[] { 0, 0 }), UITableViewScrollPosition.Top, true);

            //animate secondary conditions
            ConstraintSecondaryTableViewTop.Constant = seeMoreToggled ? secondaryViewHeight : 0;
            ConstraintSecondaryTableViewBottom.Constant = seeMoreToggled ? -secondaryViewHeight : 0;

            //animate tableview into frame
            ConstraintPrimaryTableViewTop.Constant = seeMoreToggled ? -imageViewHeight : 0;

            AnalyzedImageFileNameHeaderLabel.Text = seeMoreToggled ? "Analyzed Image" : "X-Ray Image";

            ConditionsTableViewSource primarySource = primaryTableView.Source as ConditionsTableViewSource;
            ConditionsTableViewSource secondarySource = secondaryTableView.Source as ConditionsTableViewSource;

            // Set see-more visibility for tableviews
            primarySource.SeeMoreVisible = seeMoreToggled;
            secondarySource.SeeMoreVisible = seeMoreToggled;

            // Reload each tableview
            primaryTableView.ReloadData();
            secondaryTableView.ReloadData();

            // Set the original label voice over status dependent on see-more visibility
            EnableVoiceOverForViews(originalLabels.ToArray(), seeMoreToggled);

            // Focus to top back-button on see-more toggle
            if (seeMoreToggled)
            {
                UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, this.NavigationItem.LeftBarButtonItem);
            }

            UIView.Animate(
                0.33,
                0,
                UIViewAnimationOptions.CurveEaseIn,
                () =>
                {
                    View.LayoutIfNeeded();

                    //animate image to top right (this needs to be in the animation closure)
                    ImageViewWrapper.Layer.AnchorPoint = seeMoreToggled ? new CGPoint(0, 1) : new CGPoint(0.5, 0.5);
                    ImageViewWrapper.Transform = seeMoreToggled ? CGAffineTransform.MakeScale(0.5F, 0.5F) : CGAffineTransform.MakeScale(1F, 1F);

                    OriginalImageWrapper.Layer.AnchorPoint = seeMoreToggled ? new CGPoint(0, 0) : new CGPoint(0.5, 0.5);

                    //scale & transformation to move original image to to bottom right
                    var originalImageTransformation = CGAffineTransform.MakeScale(0.5F, 0.5F);
                    originalImageTransformation.Translate(0.0F, secondaryViewHeight / 2);
                    OriginalImageWrapper.Transform = seeMoreToggled ? originalImageTransformation : CGAffineTransform.MakeScale(1F, 1F);

                    if (seeMoreToggled)
                    {
                        //get the distance from the bottom of the Analyzed Image View Wrapper to the top of the original image view wrapper
                        var distance = OriginalImageWrapper.Frame.Top - ImageViewWrapper.Frame.Bottom;

                        //set the height of the info views to distance
                        AnalyzedImageInfoViewHeightConstraint.Constant = distance;
                        OriginalImageInfoViewHeightConstraint.Constant = distance;

                        //force autolayout to recalculate
                        View.LayoutIfNeeded();

                        //move the info views down
                        AnalyzedImageInfoViewBottomConstraint.Constant = AnalyzedImageInfoView.Frame.Size.Height;
                        OriginalImageInfoViewBottomConstraint.Constant = OriginalImageInfoView.Frame.Size.Height;

                        //scale the font size by a factor of 2
                        //because the imagewrapper is scaled by a factor of 0.5
                        AnalyzedImageFileNameHeaderLabel.Font   = UIFont.PreferredHeadline.WithSize(UIFont.PreferredHeadline.PointSize * scaleFactor);
                        AnalyzedImageFileNameLabel.Font         = UIFont.PreferredSubheadline.WithSize(UIFont.PreferredSubheadline.PointSize * scaleFactor);
                        AnalyzedImageDateHeaderLabel.Font       = UIFont.PreferredHeadline.WithSize(UIFont.PreferredHeadline.PointSize * scaleFactor);
                        AnalyzedImageDateLabel.Font             = UIFont.PreferredSubheadline.WithSize(UIFont.PreferredSubheadline.PointSize * scaleFactor);
                        
                        OriginalImageFileNameHeaderLabel.Font   = UIFont.PreferredHeadline.WithSize(UIFont.PreferredHeadline.PointSize * scaleFactor);
                        OriginalImageFileNameLabel.Font         = UIFont.PreferredSubheadline.WithSize(UIFont.PreferredSubheadline.PointSize * scaleFactor);
                        OriginalImageDateHeaderLabel.Font       = UIFont.PreferredHeadline.WithSize(UIFont.PreferredHeadline.PointSize * scaleFactor);
                        OriginalImageDateLabel.Font             = UIFont.PreferredSubheadline.WithSize(UIFont.PreferredSubheadline.PointSize * scaleFactor);
                    }
                    else
                    {
                        //remove the constants applied from above
                        //this will revert the height and position to before see more is tapped
                        AnalyzedImageInfoViewBottomConstraint.Constant = 0;
                        AnalyzedImageInfoViewHeightConstraint.Constant = 0;
                        OriginalImageInfoViewBottomConstraint.Constant = 0;
                        OriginalImageInfoViewHeightConstraint.Constant = 0;

                 
                        AnalyzedImageFileNameHeaderLabel.Font   = UIFont.PreferredHeadline;
                        AnalyzedImageFileNameLabel.Font         = UIFont.PreferredSubheadline;
                        AnalyzedImageDateHeaderLabel.Font       = UIFont.PreferredHeadline;
                        AnalyzedImageDateLabel.Font             = UIFont.PreferredSubheadline;

                        OriginalImageFileNameHeaderLabel.Font   = UIFont.PreferredHeadline;
                        OriginalImageFileNameLabel.Font         = UIFont.PreferredSubheadline;
                        OriginalImageDateHeaderLabel.Font       = UIFont.PreferredHeadline;
                        OriginalImageDateLabel.Font             = UIFont.PreferredSubheadline;
                    }
                },
                () =>
                {
                    ButtonSeeMore.Enabled = true;
                }
            );
        }

        /// <summary>
        /// Gets a filename for a given NSUrl
        /// </summary>
        /// <param name="url">The url path to the file</param>
        /// <param name="defaultVal">The default file string to be returned if url is null</param>
        /// <returns>
        /// The file string representing url
        /// </returns>
        private string GetFileString(NSUrl url, string defaultVal)
        {
            if (url == null)
            {
                return defaultVal;
            }

            return System.IO.Path.GetFileName(url.ToString());
        }

        /// <summary>
        /// Extract a DateTime object from a NSUrl object
        /// </summary>
        /// <param name="url">The url of the file</param>
        /// <returns>
        /// 1. The current date as a Nullable<DateTime> object IF url == null
        /// 2. The date (as a Nullable<DateTime> object) that exists within the file's metadata (pointed to by url) IFF the EXIF data exists
        /// 3. Null if the date cannot be found within the file's metadata
        /// </returns>
        private DateTime? GetDate(NSUrl url)
        {
            DateTime dateTaken;

            // If the provided url is null
            // NOTE: This case will happen if we import from camera
            if (url == null)
            {
                dateTaken = DateTime.Now;
                return dateTaken;
            }

            CGImageSource source = CGImageSource.FromUrl(url);
            NSDictionary ns = new NSDictionary();
            NSDictionary properties = source.CopyProperties(ns, 0);

            NSDictionary exifDict = properties?.ObjectForKey(ImageIO.CGImageProperties.ExifDictionary) as NSDictionary;

            if (exifDict != null)
            {
                NSString date = exifDict[ImageIO.CGImageProperties.ExifDateTimeOriginal] as NSString;

                if (!string.IsNullOrEmpty(date) &&
                    DateTime.TryParseExact(date, "yyyy:MM:dd HH:mm:ss", CultureInfo.CurrentCulture, DateTimeStyles.None, out dateTaken))
                {
                    return dateTaken;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// A comparer to sort a collection of doubles in descending order
    /// </summary>
    internal class DescendComparer : IComparer<ScoreOutput>
    {
        public int Compare(ScoreOutput x, ScoreOutput y)
        {
            return y.CompareTo(x);
        }
    }
}
