// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using UIKit;

namespace XRayAnalysis.iOS.Service.NavigationControllerUtil
{
    /// <summary>
    /// A utility class for functions that work with the navigation bar.
    /// </summary>
    public static class NavigationControllerUtil
    {
        /// <summary>
        /// Sets the navigation bar attributes to a consistent colour
        /// </summary>
        /// <param name="navigationBar">Navigation bar.</param>
        public static void SetNavigationBarAttributes(UINavigationBar navigationBar)
        {
            navigationBar.BarTintColor = Constants.Purple;
            navigationBar.TintColor = UIColor.White;
            navigationBar.BarStyle = UIBarStyle.Black;
        }

        /// <summary>
        /// Sets the navigation title to the provided title
        /// </summary>
        /// <param name="navigationItem">Navigation item.</param>
        /// <param name="title">Title.</param>
        public static void SetNavigationTitle(UINavigationItem navigationItem, string title)
        {
            navigationItem.Title = title ?? "";
        }
    }
}
