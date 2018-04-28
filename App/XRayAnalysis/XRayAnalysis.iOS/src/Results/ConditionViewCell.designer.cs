// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace XRayAnalysis.iOS.Results
{
    [Register ("ConditionViewCell")]
    partial class ConditionViewCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ConditionLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView EyeImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel LikelihoodLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel LikelihoodTextLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ConditionLabel != null) {
                ConditionLabel.Dispose ();
                ConditionLabel = null;
            }

            if (EyeImage != null) {
                EyeImage.Dispose ();
                EyeImage = null;
            }

            if (LikelihoodLabel != null) {
                LikelihoodLabel.Dispose ();
                LikelihoodLabel = null;
            }

            if (LikelihoodTextLabel != null) {
                LikelihoodTextLabel.Dispose ();
                LikelihoodTextLabel = null;
            }
        }
    }
}