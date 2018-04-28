// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
 
using System;

using XRayAnalysis.iOS.Service.NavigationControllerUtil;
using Foundation;
using UIKit;

namespace XRayAnalysis.iOS.About
{
    /// <summary>
    /// Implementation of About.storyboard's UIViewController
    /// </summary>
    public partial class AboutViewController : UIViewController
    {
        /// <summary>
        /// Initialization of the view <see cref="AboutViewController"/> 
        /// </summary>
        public AboutViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            UIFont aboutBold = UIFont.FromDescriptor(UIFontDescriptor.PreferredTitle1.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold), 0);
            this.About.Font = UIFontMetrics.DefaultMetrics.GetScaledFont(aboutBold);

            NavigationControllerUtil.SetNavigationBarAttributes(this.NavigationController.NavigationBar);
            NavigationControllerUtil.SetNavigationTitle(this.NavigationItem, Constants.ApplicationName);

            //Programmatically add a back button and an arrow
            UIImage backArrowImage = UIImage.FromBundle("back-arrow");
            UIButton backButton = new UIButton(UIButtonType.Custom);

            backButton.SetImage(backArrowImage, UIControlState.Normal);
            backButton.SetTitle("Back", UIControlState.Normal);
            backButton.ImageEdgeInsets = new UIEdgeInsets(0.0f, -12.5f, 0.0f, 0.0f);
            backButton.AddTarget((sender, e) =>
            {
                this.NavigationController?.PopViewController(true);
            }, UIControlEvent.TouchUpInside);

            this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(backButton);

            // Programatically set version number
            this.Version.Text = "Version " + Constants.VersionNumber;

            //set the initial focus element
            UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, About);
        }

		public override void ViewDidAppear(bool animated)
		{
            base.ViewDidAppear(animated);

            this.NavigationController.NavigationBar.AccessibilityLabel = Constants.NavigationBarAccessibilityLabel_AboutPage;
		}

		public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

