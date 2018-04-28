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

namespace XRayAnalysis.iOS.Analysis
{
    [Register ("AnalysisViewController")]
    partial class AnalysisViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIActivityIndicatorView AnalyzingIndicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel AnalyzingLabel { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (AnalyzingIndicator != null) {
                AnalyzingIndicator.Dispose ();
                AnalyzingIndicator = null;
            }

            if (AnalyzingLabel != null) {
                AnalyzingLabel.Dispose ();
                AnalyzingLabel = null;
            }
        }
    }
}