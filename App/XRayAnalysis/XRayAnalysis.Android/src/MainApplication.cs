// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Preferences;
using Android.Runtime;
using XRayAnalysis.Droid.Service.AI.ChestXRay;
using XRayAnalysis.Droid.Service.Preferences;
using XRayAnalysis.Service.AI.ChestXRay;
using XRayAnalysis.Service.Analytics;

namespace XRayAnalysis.Droid
{
    /// <summary>
    /// Subclass of Application. Initialization of telemetry is done here. 
    /// Refer to: https://developer.xamarin.com/guides/android/under_the_hood/architecture/#Java_Activation for more info
    /// </summary>
    [Application]
    public class MainApplication: Application
    {
        private const string EnvironmentPropertyFile = "environment.txt";
        private const string AppCenterApiVariableKey = "APP_CENTER_API_KEY";

        private AppPrefManager appPrefManager;

        public AppPrefManager AppPrefManager
        {
            get
            {
                if(appPrefManager == null)
                {
                    appPrefManager = new AppPrefManager(this);
                }
                return appPrefManager;
            }
        }

        private ChestXRayAIClient aiClient;

        /// <summary>
        /// An instance of ChestXRayAIClient for use throughout the application.
        /// If the AI models haven't been loaded, they will be loaded on a get of this property.
        /// </summary>
        /// <value>The ChestXRayAIClient instance.</value>
        public ChestXRayAIClient AIClient
        {
            get
            {
                if(aiClient == null)
                {
                    LoadModel();
                }
                return aiClient;
            }
        }

        public MainApplication(IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            LoadAppCenter();

            // Initially load the AIClient on application launch, so the first analysis goes through quickly
            LoadModel();
        }

        private void LoadModel()
        {
            aiClient = AndroidChestXRayAIClient.NewInstance(this.Assets);
            System.Diagnostics.Debug.WriteLine("AI Client Loaded");
        }

        private void ReleaseModel()
        {
            aiClient?.Dispose();
            aiClient = null;
            System.Diagnostics.Debug.WriteLine("AI Client Released");
        }

        private void LoadAppCenter()
        {
            AssetManager assets = this.Assets;
            string content = "";

            using (StreamReader sr = new StreamReader(assets.Open(EnvironmentPropertyFile)))
            {
                content = sr.ReadToEnd();
            }

            string[] lines = content.Split('\n');

            AnalyticsService.SetEnvironmentVariables(lines);

            // For more info on how to get a Visual Studio App Center API key refer to: 
            // https://docs.microsoft.com/en-us/appcenter/sdk/getting-started/xamarin 
            var apiKey = System.Environment.GetEnvironmentVariable(AppCenterApiVariableKey);

            AnalyticsService.Start(apiKey);
        }
    }
}