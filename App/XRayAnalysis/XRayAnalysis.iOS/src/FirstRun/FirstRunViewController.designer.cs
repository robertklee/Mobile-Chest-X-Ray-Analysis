// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace XRayAnalysis.iOS.FirstRun
{
    [Register("FirstRunViewController")]
    partial class FirstRunViewController
    {
        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UILabel Description { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UILabel Disclaimer { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint DisclaimerCenterYAccessibilityConstraint { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint DisclaimerCenterYNormalConstraint { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UIButton GetStartedButton { get; set; }

        [Outlet]
        [GeneratedCode("iOS Designer", "1.0")]
        UIKit.UILabel Title { get; set; }

        void ReleaseDesignerOutlets()
        {
            if (Description != null)
            {
                Description.Dispose();
                Description = null;
            }

            if (Disclaimer != null)
            {
                Disclaimer.Dispose();
                Disclaimer = null;
            }

            if (DisclaimerCenterYAccessibilityConstraint != null)
            {
                DisclaimerCenterYAccessibilityConstraint.Dispose();
                DisclaimerCenterYAccessibilityConstraint = null;
            }

            if (DisclaimerCenterYNormalConstraint != null)
            {
                DisclaimerCenterYNormalConstraint.Dispose();
                DisclaimerCenterYNormalConstraint = null;
            }

            if (GetStartedButton != null)
            {
                GetStartedButton.Dispose();
                GetStartedButton = null;
            }

            if (Title != null)
            {
                Title.Dispose();
                Title = null;
            }
        }
    }
}