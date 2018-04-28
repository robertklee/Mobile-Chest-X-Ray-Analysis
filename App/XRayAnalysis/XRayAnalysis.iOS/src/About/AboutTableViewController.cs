// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
 
using System;

using Foundation;
using SafariServices;
using UIKit;

namespace XRayAnalysis.iOS.About
{
    /// <summary>
    /// Implementation of About.storyboard's Container View embedded segue to UITableViewController
    /// </summary>
    public partial class AboutTableViewController : UITableViewController
    {
        private readonly nint numberOfSections = 1; 

        public AboutTableViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            this.TableView.ContentInset = UIEdgeInsets.Zero;

            // Credit to https://stackoverflow.com/questions/14520185/how-to-remove-empty-cells-in-uitableview
            // Set a zero height table footer view (to prevent blank cells from being displayed
            this.TableView.TableFooterView = new UIView(CoreGraphics.CGRect.Empty);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        [Export("numberOfSectionsInTableView:")]
        public override nint NumberOfSections(UITableView tableView)
        {
            return numberOfSections;
        }

        /// <summary>
        /// Launches the page once the user selects a row.
        /// </summary>
        /// <param name="tableView">The UITableView.</param>
        /// <param name="indexPath">Index path to the row that was selected.</param>
        [Export("tableView:didSelectRowAtIndexPath:")]
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            switch ((TableCell)indexPath.Row)
            {
                case TableCell.ApplicationRepository:
                    LaunchSafariView(SharedConstants.ApplicationRepository);
                    break;

                case TableCell.ModelRepository:
                    LaunchSafariView(SharedConstants.MLModelRepository);
                    break;
            }
        }

        /// <summary>
        /// Launches a new safari window modally
        /// </summary>
        /// <param name="url">The URL of the webpage to display</param>
        private void LaunchSafariView(string url)
        {
            SFSafariViewController sfSafariViewController = new SFSafariViewController(new NSUrl(url));

            this.PresentViewController(sfSafariViewController, true, null);
        }

        private enum TableCell
        {
            // These integers refer to the static table view cells
            ApplicationRepository = 0,
            ModelRepository = 1
        }
    }
}

