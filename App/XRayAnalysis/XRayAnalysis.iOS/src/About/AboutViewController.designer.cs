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

namespace XRayAnalysis.iOS.About
{
    [Register ("AboutViewController")]
    partial class AboutViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel About { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel Version { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (About != null) {
                About.Dispose ();
                About = null;
            }

            if (Version != null) {
                Version.Dispose ();
                Version = null;
            }
        }
    }
}