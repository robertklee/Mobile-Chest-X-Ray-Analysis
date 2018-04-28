// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Foundation;
using UIKit;

namespace XRayAnalysis.iOS
{
    /// <summary>
    /// This class holds constants that are used throughout the application
    /// Anything that is used more than once should be placed and referenced
    /// from here
    /// </summary>
    public static class Constants
    {
        #region ApplicationProperties
        public static readonly string FirstLaunchKey = NSBundle.MainBundle.BundleIdentifier + ".FirstLaunch.WasLaunchedBefore";
        public static readonly string AppGroupIdentifier = "group.com.companyname.XRayAnalysis";
        public static readonly string ApplicationName = "Mobile Chest X-Ray Analysis";
        public static readonly string VersionNumber = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
        public static readonly string ShareExtensionKey = NSBundle.MainBundle.BundleIdentifier + ".LaunchedFromShareExtension";
        public static readonly string ShareExtensionImageKey = NSBundle.MainBundle.BundleIdentifier + ".SharedImage";
        public static readonly string URLScheme = "XRayAnalysis://";
        #endregion

        #region Accessibility
        public static readonly string NavigationBarAccessibilityLabel_AboutPage = "About app page";
        public static readonly string NavigationBarAccessibilityLabel_AnalysisPage = "Analyzing page";
        public static readonly string NavigationBarAccessibilityLabel_FirstRunPage = "Welcome page";
        public static readonly string NavigationBarAccessibilityLabel_ImageInputPage = "Image Input page";
        public static readonly string NavigationBarAccessibilityLabel_ResultsPage = "Results page";
        public static readonly string AccessibilityLabel_SeeMoreShowButton = "See More analysis results for X-Ray";
        public static readonly string AccessibilityLabel_SeeMoreHideButton = "See Less analysis results for X-Ray";
        #endregion

        #region Colours
        public static readonly UIColor Purple = UIColor.FromRGB(34, 21, 87);
        public static readonly UIColor Pink = UIColor.FromRGB(194, 3, 201);
        public static readonly UIColor Blue = UIColor.FromRGB(25, 118, 210);
        public static readonly UIColor Turquoise = UIColor.FromRGB(5, 131, 138);
        public static readonly UIColor White = UIColor.FromRGB(255, 255, 255);
        public static readonly UIColor LightGrey = UIColor.FromRGB(214, 214, 214);
        public static readonly UIColor Grey = UIColor.FromRGB(84, 84, 84);
        public static readonly UIColor PaleGrey = UIColor.FromRGB(242, 242, 242);
        public static readonly UIColor DarkPurple = UIColor.FromRGB(83, 49, 160);
        #endregion

        #region Dimensions
        public static readonly nfloat CropBorderWidth = 2;
        public static readonly nfloat MaximumCropImageZoomScale = 4;        // Max zoom for UIImage in <see cref="CropViewController.cs"/>
        public static readonly nfloat MaximumResultsImageZoomScale = 4;     // Max zoom for UIImage in <see cref="ResultsViewController.cs"/>
        #endregion

        #region Environment Variables
        public static readonly string EnvironmentPropertyFile = "environment.txt";
        public static readonly string AppCenterApiVariableKey = "APP_CENTER_API_KEY";
        #endregion

        #region Conditions
        public static readonly string[] ConditionList =
        {
            "Atelectasis",
            "Cardiomegaly",
            "Effusion",
            "Infiltration",
            "Mass",
            "Nodule",
            "Pneumonia",
            "Pneumothorax",
            "Consolidation",
            "Edema",
            "Emphysema",
            "Fibrosis",
            "Pleural Thickening",
            "Hernia"
        };
        #endregion

        #region LikelihoodLabel
        public static string[] LikelihoodLabel =
        {
            "Very Unlikely",
            "Unlikely",
            "Uncertain",
            "Likely",
            "Very Likely"
        };
        #endregion
    }
}
