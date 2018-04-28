// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using CoreGraphics;
using XRayAnalysis.iOS.About;
using XRayAnalysis.iOS.ImageInput;
using XRayAnalysis.iOS.Service;
using XRayAnalysis.iOS.Service.NavigationControllerUtil;
using XRayAnalysis.iOS.Service.UserDefaults;
using Foundation;
using SafariServices;
using UIKit;

namespace XRayAnalysis.iOS.FirstRun
{
    /// <summary>
    /// Implementation of FirstRun.storyboard's UIViewController
    /// </summary>
    public partial class FirstRunViewController : UIViewController
    {
        private const int CornerRadius = 5;
        private const int titleFontSize = 30;

        private const string tappableStringChestXRayAIModel = "ChestXRay AI model";
        private readonly string descriptionText = Constants.ApplicationName + " is a research project \n used to showcase the " + tappableStringChestXRayAIModel + ".";

        private NSRange rangeOfStringChestXRayAIModel;

        public FirstRunViewController(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// Initialization of the view <see cref="ImageInputViewController"/> 
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib;

            this.GetStartedButton.Layer.CornerRadius = CornerRadius;
            this.GetStartedButton.ClipsToBounds = true;

            SetUpStringAttributes();

            // Add gesture recognizer for the string tappableStringChestXRayAIModel
            this.Description.UserInteractionEnabled = true; // UserInteractionEnabled must be true for gesture recognizers to be activated
            this.Description.AddGestureRecognizer(new UITapGestureRecognizer((UITapGestureRecognizer gestureRecognizer) =>
            {
                // Launch link if tap lands within description bounds
                LaunchUrlInSafari(SharedConstants.MLModelRepository);

                // To implement tappable portions of an attributed string, uncomment the lines of code below. 
                // Issues with the function: VoiceOver taps don't land within the bounds of the tappable string range.
                //UILabel label = gestureRecognizer.View as UILabel;

                //if (GestureRecognizerUtil.DidTapAttributedTextInLabel(label, rangeOfStringChestXRayAIModel, gestureRecognizer, label.TextAlignment))
                //{
                //    LaunchUrlInSafari(SharedConstants.MLModelRepository);
                //}
            }));

            NavigationControllerUtil.SetNavigationBarAttributes(this.NavigationController.NavigationBar);
            NavigationControllerUtil.SetNavigationTitle(this.NavigationItem, Constants.ApplicationName);

            UIButton rightButton = new UIButton(UIButtonType.InfoLight);

            rightButton.TouchUpInside += NavBarButtonInfo_TouchUpInside;

            UIBarButtonItem rightBarButton = new UIBarButtonItem(customView: rightButton);

            this.NavigationItem.SetRightBarButtonItem(rightBarButton, true);

            // Set up touch events
            this.GetStartedButton.TouchUpInside += GetStartedButton_TouchUpInside;

            //set the initial focus element
            UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, Title);
        }

		public override void ViewDidAppear(bool animated)
		{
            base.ViewDidAppear(animated);

            this.NavigationController.NavigationBar.AccessibilityLabel = Constants.NavigationBarAccessibilityLabel_FirstRunPage;
		}

		/// <summary>
		/// Launches the URL in a SFSafariViewController (without launching Safari app itself).
		/// </summary>
		/// <param name="url">URL to launch.</param>
		private void LaunchUrlInSafari(string url)
        {
            System.Diagnostics.Debug.WriteLine("Url to launch: " + url);

            SFSafariViewController sfSafariViewController = new SFSafariViewController(new NSUrl(url));

            this.PresentViewController(sfSafariViewController, true, null);
        }

        /// <summary>
        /// Sets up the attributed strings for the tappable text.
        /// </summary>
        private void SetUpStringAttributes()
        {
            // NSMutableAttributedString enables different sections to have different attributes
            NSMutableAttributedString attributedDescriptionText = new NSMutableAttributedString(descriptionText);
            // Find the section of the string where the target string resides
            rangeOfStringChestXRayAIModel = (new NSString(descriptionText)).LocalizedStandardRangeOfString(new NSString(tappableStringChestXRayAIModel));
            // Set font size explicitly
            attributedDescriptionText.AddAttribute(UIStringAttributeKey.Font,
                                                   UIFont.PreferredBody,
                                                   new NSRange(0, this.Description.Text.Length - 1));
            // Single underline the string located at the NSRange
            attributedDescriptionText.AddAttribute(UIStringAttributeKey.UnderlineStyle,
                                                   NSNumber.FromInt32((int)NSUnderlineStyle.Single),
                                                   rangeOfStringChestXRayAIModel);

            // Set the label text to the attributed text
            this.Description.AttributedText = attributedDescriptionText;

            UIFont title1Bold = UIFont.FromDescriptor(UIFontDescriptor.PreferredTitle1.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold), titleFontSize);
            this.Title.Font = UIFontMetrics.DefaultMetrics.GetScaledFont(title1Bold);
            this.Title.AdjustsFontForContentSizeCategory = true;
        }

        /// <summary>
        /// Launches the About view controller.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void NavBarButtonInfo_TouchUpInside(object sender, EventArgs eventArgs)
        {
            // Get about storyboard
            UIStoryboard aboutStoryboard = UIStoryboard.FromName("About", null);

            // Grab about view Controller reference
            AboutViewController aboutViewController = aboutStoryboard.InstantiateInitialViewController() as AboutViewController;

            // Push about view controller onto stack
            this.NavigationController.PushViewController(aboutViewController, true);
        }

        /// <summary>
        /// Sets a flag that the user has launched the app before, then launches the view that is normally launched when the user enters the app.
        /// </summary>
        /// <param name="sender">Button object that was pressed.</param>
        private void GetStartedButton_TouchUpInside(object sender, EventArgs eventArgs)
        {
            FirstLaunchClient firstLaunch = ((AppDelegate)UIApplication.SharedApplication.Delegate).FirstLaunch;
            firstLaunch.WasLaunchedBefore = true;

            var storyboard = UIStoryboard.FromName("Main", null);
            UIViewController imageInputViewController = storyboard.InstantiateInitialViewController() as UIViewController;
            this.DismissViewController(false, null);
            this.PresentViewController(imageInputViewController, true, null);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}