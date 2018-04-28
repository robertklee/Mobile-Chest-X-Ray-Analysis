// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using XRayAnalysis.iOS.Results;
using XRayAnalysis.iOS.Service.AI;
using XRayAnalysis.iOS.Service.AI.ChestXRay;
using XRayAnalysis.iOS.Service.NavigationControllerUtil;
using XRayAnalysis.Service;
using XRayAnalysis.Service.AI.ChestXRay;
using Foundation;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using UIKit;

namespace XRayAnalysis.iOS.Analysis
{
    /// <summary>
    /// AnalysisViewController sits in between the ImageInput and Results view. 
    /// This class uses the AI service to determine the chest conditions based on the image passed in from ImageInput view
    /// Once the AI has finished the computation, the results will be passed into the Results view for the user
    /// </summary>
    public partial class AnalysisViewController : UIViewController
    {
        public UIImage InputImage { get; set; }

        public SKBitmap InputImageBitmap { get; set; }

        public NSUrl ImageUrl { get; set; }

        private UIStoryboard resultsStoryboard;

        private CancellationTokenSource cancellationTokenSource;

        public AnalysisViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            // Set navigation bar attributes
            NavigationControllerUtil.SetNavigationBarAttributes(this.NavigationController.NavigationBar);
            NavigationControllerUtil.SetNavigationTitle(this.NavigationItem, Constants.ApplicationName);
            this.NavigationItem.HidesBackButton = true;

            this.resultsStoryboard = UIStoryboard.FromName("Results", null);

            //set the initial focus element
            UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, AnalyzingLabel);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            this.NavigationController.NavigationBar.AccessibilityLabel = Constants.NavigationBarAccessibilityLabel_AnalysisPage;

            // TODO Notify the user that there was an error
            // If images are null return back to Image Input 
            if (this.InputImage == null || this.InputImageBitmap == null)
            {
                this.NavigationController.PopViewController(true);
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            // Retrieve the shared AI Client that was loaded by the AppDelegate
            ChestXRayAIClient aiClient = ((AppDelegate)UIApplication.SharedApplication.Delegate).AIClient;
            System.Diagnostics.Debug.WriteLine("AI Client retrieved from AppDelegate");

            try
            {
                (ScoreOutput[] scores, CAM[] cams) = await ChestXRayAIRunner.Analyze(aiClient, InputImageBitmap, cancellationTokenSource.Token);
                System.Diagnostics.Debug.WriteLine("Analysis completed");

                LaunchResults(scores, cams);
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Analysis cancelled");
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // dispose of unneeded assets
            InputImage?.Dispose();
            InputImageBitmap?.Dispose();
        }

        private void LaunchResults(ScoreOutput[] scores, CAM[] cams)
        {
            ResultsViewController resultsViewController = resultsStoryboard.InstantiateInitialViewController() as ResultsViewController;
            resultsViewController.InputImage = InputImage;
            resultsViewController.ImageUrl = ImageUrl;

            resultsViewController.InitResultSet(cams, scores);

            this.NavigationController.PushViewController(resultsViewController, true);

            RemoveFromNavigationStack();
        }

        private void RemoveFromNavigationStack()
        {
            // Remove this UIViewController from the Navigation stack
            if (this.NavigationController != null)
            {
                // Grab the current and create the new viewcontroller sets
                UIViewController[] controllers = this.NavigationController.ViewControllers;
                UIViewController[] newSet = new UIViewController[controllers.Length - 1];

                // Populate the new-set with all viewcontrollers except this
                int i = 0;
                foreach (UIViewController controller in controllers)
                {
                    if (controller != this)
                    {
                        newSet[i] = controller;
                        i++;
                    }
                }

                // Assign the current set of view controllers to the new set
                this.NavigationController.ViewControllers = newSet;
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

    }
}

