// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using XRayAnalysis.Droid.ImageInput;
using XRayAnalysis.Droid.Intro;

namespace XRayAnalysis.Droid.Splash
{
    /// <summary>
    /// Activity class for the Splash Activity.
    /// This is the MainLauncher Activity.
    /// This Activity contains no layout and only uses a theme to draw a window background.
    /// </summary>
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/ic_launcher", Theme = "@style/AppTheme.Splash", NoHistory = true, ScreenOrientation = ScreenOrientation.Portrait)]
    public class SplashActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (!((MainApplication)this.Application).AppPrefManager.HasPassedIntroPage)
            {
                //if first time using the app launch the InroActivity
                StartActivity(new Intent(Application.Context, typeof(IntroActivity)));
            }
            else
            {
                //if not the first time using the app launch the ImageInputActivity
                StartActivity(new Intent(Application.Context, typeof(ImageInputActivity)));
            }
        }
    }
}