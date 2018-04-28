// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Java.IO;
using System;

//Alias directives
using SupportWidget = Android.Support.V7.Widget;

namespace XRayAnalysis.Droid.About
{
    [Activity(Label = "@string/about_title", ScreenOrientation = ScreenOrientation.Portrait)]
    public class AboutActivity : AppCompatActivity
    {
        private TextView versionNumberTextView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_about);

            SupportWidget.Toolbar toolbar = FindViewById<SupportWidget.Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            //Assign the correct layout for the TextView for the verison number
            versionNumberTextView = FindViewById<TextView>(Resource.Id.txt_version); 

            string versionNumberString = GetString(Resource.String.about_version, 
                                               Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName);

            versionNumberTextView.Text = versionNumberString;

            FindViewById(Resource.Id.layout_model_repository).Click += delegate
            {
                URLs.Launch(this, SharedConstants.MLModelRepository);
            };

            FindViewById(Resource.Id.layout_application_repository).Click += delegate
            {
                URLs.Launch(this, SharedConstants.ApplicationRepository);
            };

            FindViewById(Resource.Id.button_clear_data).Click += delegate
            {
                DeleteCache(Application.Context);
            };
        }

        /// <summary>
        /// Helper method to delete the application's cache directory
        /// </summary>
        /// <param name="context">Context</param>
        private void DeleteCache(Context context)
        {
            try
            {
                File dir = context.CacheDir;
                int stringResource = DeleteDir(dir) ? Resource.String.about_success_clearing_data : Resource.String.about_error_clearing_data;
                View rootView = FindViewById(Resource.Id.root);

                Snackbar.Make(rootView, stringResource, Snackbar.LengthLong).Show();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write("Error when attempting to clear cache: " + e.Message);
            }
        }

        /// <summary>
        /// Helper method to delete a directory and its contents.
        /// </summary>
        /// <param name="dir">File Object</param>
        /// <returns>True if a directory and its contents have been successfully deleted. False otherwise.</returns>
        private bool DeleteDir(File dir)
        {
            if (dir == null) { return false; }

            if(dir.IsFile)
            {
                return dir.Delete();
            }
            else if (dir.IsDirectory)
            {
                string[] items = dir.List();

                for(int i = 0; i < items.Length; i++)
                {
                    if(!DeleteDir(new File(dir, items[i])))
                    {
                        return false;
                    }
                }

                return dir.Delete();
            }

            return false;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}