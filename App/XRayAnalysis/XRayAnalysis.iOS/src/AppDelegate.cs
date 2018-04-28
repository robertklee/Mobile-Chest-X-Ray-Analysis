// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Foundation;
using UIKit;
using XRayAnalysis.Service.Analytics;
using XRayAnalysis.Service.AI.ChestXRay;
using XRayAnalysis.iOS.FirstRun;
using XRayAnalysis.iOS.ImageInput;
using XRayAnalysis.iOS.Service.UserDefaults;
using XRayAnalysis.iOS.Service.AI.ChestXRay;

namespace XRayAnalysis.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window
        {
            get;
            set;
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
                if (aiClient == null)
                {
                    LoadModel();
                }
                return aiClient;
            }
        }

        private FirstLaunchClient firstLaunch;

        /// <summary>
        /// An instance of <see cref="FirstLaunchClient"/> for use throughout the application.
        /// If null, a new instance will be instantiated with the default constructor.
        /// </summary>
        /// <value>The FirstLaunchClient instance.</value>
        public FirstLaunchClient FirstLaunch
        {
            get 
            {
                if (firstLaunch == null)
                {
                    firstLaunch = new FirstLaunchClient();
                }
                return firstLaunch;
            }
        }

        /// <summary>
        /// Indicates whether the app was launched from a share extension.
        /// </summary>
        /// <value><c>true</c> if launched from share extension; otherwise, <c>false</c>.</value>
        public bool LaunchedFromShareExtension
        {
            get
            {
                return NSUserDefaults.StandardUserDefaults.BoolForKey(Constants.ShareExtensionKey);
            }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, Constants.ShareExtensionKey);
            }
        }

        /// <summary>
        /// Open a URL that was sent to your app. If there is a URL to open, the system calls this method of your app delegate.
        /// See https://developer.apple.com/documentation/uikit/uiapplicationdelegate/1623112-application?preferredLanguage=occ
        /// </summary>
        /// <returns>True if the delegate successfully handled the request or false if the attempt to open the URL resource failed.</returns>
        /// <param name="application">Your singleton app object.</param>
        /// <param name="url">The URL resource to open.</param>
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            if (url != null)
            {
                LaunchedFromShareExtension = true;

                System.Diagnostics.Debug.WriteLine("Launched from share extension");

                if (FirstLaunch.WasLaunchedBefore)
                {
                    // Force a new instance of ImageInputViewController
                    this.Window = new UIWindow(frame: UIScreen.MainScreen.Bounds);

                    var storyboard = UIStoryboard.FromName("ImageInput", null);
                    ImageInputViewController imageInputViewController = storyboard.InstantiateInitialViewController() as ImageInputViewController;

                    UINavigationController navigationController = new UINavigationController(imageInputViewController);

                    if (this.Window != null)
                    {
                        this.Window.RootViewController = navigationController;
                        this.Window.MakeKeyAndVisible();
                    }
                }

                return true;
            }
            return false;
        }

        /// <summary>
        /// Override point for customization after application launch.
        /// </summary>
        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            string[] lines = System.IO.File.ReadAllLines(Constants.EnvironmentPropertyFile);
            AnalyticsService.SetEnvironmentVariables(lines);

            // For more info on how to get a Visual Studio App Center API key refer to: 
            // https://docs.microsoft.com/en-us/appcenter/sdk/getting-started/xamarin 
            var apiKey = System.Environment.GetEnvironmentVariable(Constants.AppCenterApiVariableKey);

            AnalyticsService.Start(apiKey);

            // Initially load the AIClient on application launch, so the first analysis goes through quickly
            LoadModel();

#if DEBUG
            // Force FirstRun to always display in DEBUG mode
            firstLaunch = new FirstLaunchClient(
                () => { return false; }, 
                (value) => { }
            );
#else
            firstLaunch = new FirstLaunchClient();
#endif

            if (!firstLaunch.WasLaunchedBefore)
            {
                System.Diagnostics.Debug.WriteLine("First launch of app");

                this.Window = new UIWindow(frame: UIScreen.MainScreen.Bounds);

                var storyboard = UIStoryboard.FromName("FirstRun", null);
                FirstRunViewController firstRunViewController = storyboard.InstantiateInitialViewController() as FirstRunViewController;

                UINavigationController navigationController = new UINavigationController(firstRunViewController);

                if (this.Window != null)
                {
                    this.Window.RootViewController = navigationController;
                    this.Window.MakeKeyAndVisible();
                }
            }

            return true;
        }

        // The return result from this method is combined with the return result from the 
        // application:willFinishLaunchingWithOptions: method to determine if a URL should be handled. 
        // If either method returns false, the URL is not handled.
        public override bool WillFinishLaunching(UIApplication application, NSDictionary launchOptions)
        {
            NSObject url;
            if (launchOptions != null && !launchOptions.TryGetValue(new NSString("UIApplicationLaunchOptionsURLKey"), out url))
            {
                // if the key is not in the dictionary
                System.Diagnostics.Debug.WriteLine("WillFinishLaunching with launchOptions: {0}", url.ToString());
                return true;
            }

            return false;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.

            // Release the resources used by the AI models
            ReleaseModel();

            // Reset LaunchedFromShareExtension flag
            LaunchedFromShareExtension = false;

            // Remove any images that may exist from previous launches from share extension
            NSUserDefaults sharedDefaults = new NSUserDefaults(Constants.AppGroupIdentifier, NSUserDefaultsType.SuiteName);
            sharedDefaults.SetValueForKey(new NSData(), new NSString(Constants.ShareExtensionImageKey));
        }

        private void LoadModel()
        {
            aiClient = iOSChestXRayAIClient.NewInstance();
            System.Diagnostics.Debug.WriteLine("AI Client Loaded");
        }

        private void ReleaseModel()
        {
            aiClient?.Dispose();
            aiClient = null;
            System.Diagnostics.Debug.WriteLine("AI Client Released");
        }
    }
}

