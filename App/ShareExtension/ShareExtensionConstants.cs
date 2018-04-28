// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Foundation;
using UIKit;

namespace ShareExtension
{
    public static class ShareExtensionConstants
    {
        public static readonly int CornerRadius = 14;
        public static readonly int FontSize = 17;
        public static readonly UIColor Purple = UIColor.FromRGB(34, 21, 87);

        public static readonly string AppGroupIdentifier = "group.com.companyname.XRayAnalysis";
        public static readonly string ApplicationName = "Mobile Chest X-Ray Analysis";
        public static readonly string ShareExtensionBundleID = NSBundle.MainBundle.BundleIdentifier;
        public static readonly string ContainingApplicationBundleID = ShareExtensionBundleID.Substring(0, ShareExtensionBundleID.IndexOf(".ShareExtension"));
        public static readonly string NSUserDefaultsKeyForImage = ContainingApplicationBundleID + ".SharedImage";
        public static readonly string PathAppendingString = "ShareExtension";
        public static readonly string SharedImageName = "SharedImage.png";
        public static readonly string UrlScheme = "XRayAnalysis://";
    }
}
