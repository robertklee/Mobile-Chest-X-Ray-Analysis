// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace XRayAnalysis.iOS.About
{
	[Register ("AboutTableViewController")]
	partial class AboutTableViewController
	{
		[Outlet]
		UIKit.UITableViewCell ApplicationRepo { get; set; }

		[Outlet]
		UIKit.UITableViewCell ModelRepo { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ApplicationRepo != null) {
				ApplicationRepo.Dispose ();
				ApplicationRepo = null;
			}

			if (ModelRepo != null) {
				ModelRepo.Dispose ();
				ModelRepo = null;
			}
		}
	}
}
