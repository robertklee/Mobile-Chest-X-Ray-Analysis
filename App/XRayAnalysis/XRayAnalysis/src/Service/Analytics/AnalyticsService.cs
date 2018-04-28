// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace XRayAnalysis.Service.Analytics
{
    /// <summary>
    /// Wrapper class for Microsoft.AppCenter.Analytics
    /// </summary>
    public static class AnalyticsService
    {
        public static class Event
        {
            public static readonly string CameraPermission = "Camera Permission";
            public static readonly string ImageInputPageViewed = "Image Input Page Viewed";
            public static readonly string ImagePickerCancelled = "Image Picker Cancelled";
            public static readonly string ImagePickerSelected = "Image Picker Selected";
            public static readonly string ResultsPageViewed = "Results Page Viewed";
            public static readonly string ReturnBackToImageInputPage = "Return Back to Image Input Page";
            public static readonly string ReturnBackToCroppingPage = "Return Back to Cropping page";
            public static readonly string FilesPageCancelled = "Files Page Cancelled";
        }

        public static class PropertyKey
        {
            public static readonly string Method = "Method";
            public static readonly string Selection = "Selection";
        }

        public static class PropertyValue
        {
            public static readonly string PhotoLibrary = "PhotoLibrary";
        }

        /// <summary>
        /// Start AppCenter
        /// </summary>
        /// <param name="apiKey">API Key</param>
        public static void Start(string apiKey)
        {
            AppCenter.Start(apiKey, typeof(Microsoft.AppCenter.Analytics.Analytics), typeof(Crashes));
        }

        /// <summary>
        /// Track a custom event.
        /// </summary>
        /// <param name="name">An event name.</param>
        /// <param name="properties">Optional properties.</param>
        public static void TrackEvent(String name, IDictionary<string, string> properties = null) 
        {
            Microsoft.AppCenter.Analytics.Analytics.TrackEvent(name, properties);
        }

        /// <summary>
        /// Set Environment Variables
        /// Each element of the input should be in the format of:
        /// key=value
        /// </summary>
        /// <param name="lines">An array of key & values seperated by '='</param>
        public static void SetEnvironmentVariables(string[] lines)
        {
            foreach(string line in lines) 
            {
                string[] keyValuePair = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

                if (keyValuePair.Length != 2) { continue; }

                string environmentKey = keyValuePair[0];
                string environmentValue = keyValuePair[1];

                Environment.SetEnvironmentVariable(environmentKey, environmentValue);
            }
        }
    }
}
