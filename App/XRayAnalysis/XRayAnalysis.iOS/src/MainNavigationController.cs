// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using XRayAnalysis.iOS.Service.NavigationControllerUtil;

using UIKit;

namespace XRayAnalysis.iOS
{
    /// <summary>
    /// MainNavigationController displays all the storyboards in a UINavigationController. 
    /// </summary>
    public partial class MainNavigationController : UINavigationController
    {
        public MainNavigationController() : base("MainNavigationController", null)
        {
        }

        public MainNavigationController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Set the navigation bar styling
            NavigationControllerUtil.SetNavigationBarAttributes(this.NavigationBar);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }
    }
}

