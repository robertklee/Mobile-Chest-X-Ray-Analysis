// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Com.Yalantis.Ucrop;
using XRayAnalysis.Droid.About;
using XRayAnalysis.Droid.Analysis;
using XRayAnalysis.Droid.Results;
using XRayAnalysis.Droid.Service.Image;
using XRayAnalysis.Service.Analytics;
using Java.IO;
using AndroidUri = Android.Net.Uri;

namespace XRayAnalysis.Droid.ImageInput
{
    /// <summary>
    /// Activity class for the Image Input page where the user can select and launch an image input method.
    /// This activity also receives ACTION_SEND intents with mime-type image/* so other apps can share images to this activity.
    /// When launched via ACTION_SEND the crop interface is immediately launched.
    /// LaunchMode is set to SingleTask so we don't have multiple instances of the Activity running (OnNewIntent will be called on the existing activity instance instead)
    /// </summary>
    [Activity(Label = "@string/image_input_title", LaunchMode = LaunchMode.SingleTask, ScreenOrientation = ScreenOrientation.Portrait)]
    [IntentFilter(
        new[] { Intent.ActionSend },
        Categories = new[] { Intent.CategoryDefault },
        Icon = "@mipmap/ic_launcher",
        Label = "@string/app_name",
        DataMimeType = "image/*")]
    public class ImageInputActivity : AppCompatActivity
    {
        private const string FILENAME_CAMERA_IMAGE = "xray_camera.png";

        private const int PermissionRequestExtStorage = 1;

        private enum RequestCode
        {
            Crop = UCrop.RequestCrop,
            ImageOpen = 1,
            Camera = 2,
            Analysis = 3,
            Results = 4
        }

        private enum InputMethod
        {
            ImageOpen,
            Camera,
            Sharing
        }

        private View rootView;

        private AndroidUri originalImageUri;
        private InputMethod selectedInputMethod;

        private ActionPendingPermissionRequest actionPendingPermissionRequest;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AnalyticsService.TrackEvent(AnalyticsService.Event.ImageInputPageViewed);

            // Load XML layout
            SetContentView(Resource.Layout.activity_two_buttons);
            rootView = FindViewById(Resource.Id.layout_root);

            // Setup toolbar
            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            SetSupportActionBar(toolbar);

            // Import Image click listener
            FindViewById(Resource.Id.layout_button_top).Click += delegate
            {
                TryExtStorageDependentAction(LaunchPhotoPicker);
            };

            // Import Camera click listener
            FindViewById(Resource.Id.layout_button_bottom).Click += delegate 
            {
                LaunchCamera();
            };

            CheckAndHandleSharing(this.Intent);
        }

        protected override void OnNewIntent(Intent intent)
        {
            CheckAndHandleSharing(intent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.menu_image_input, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_item_about:
                    Intent aboutIntent = new Intent(this, typeof(AboutActivity));
                    StartActivity(aboutIntent);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
        
        /// <summary>
        /// Callback for when a permission request has been accepted or denied.
        /// Implementation of <see cref="FragmentActivity.OnRequestPermissionsResult(int, string[], Permission[])"/>.
        /// </summary>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case PermissionRequestExtStorage:
                    {
                        if (grantResults[0] == Permission.Granted)
                        {
                            actionPendingPermissionRequest();
                        }
                        else
                        {
                            // Permission denied
                            Snackbar.Make(rootView, Resource.String.permission_request_ext_storage_denied, Snackbar.LengthShort).Show();
                        }
                        break;
                    }
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode == Result.Canceled)
            {
                AnalyticsService.TrackEvent(AnalyticsService.Event.ImagePickerCancelled, new Dictionary<string, string> {
                    { AnalyticsService.PropertyKey.Method, AnalyticsService.PropertyValue.PhotoLibrary }
                });

                HandleActivityResultCancelled((RequestCode)requestCode);
            }
            else if (resultCode == Result.Ok)
            {
                HandleActivityResultOK((RequestCode)requestCode, data);
            }
            else
            {
                HandleActivityResultError((RequestCode)requestCode, data);
            }
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();
            AnalyticsService.TrackEvent(AnalyticsService.Event.ReturnBackToImageInputPage);
        }

        private delegate void ActionPendingPermissionRequest();

        private void CheckAndHandleSharing(Intent intent)
        {
            // Determine if the Activity was launched via Intent.ActionSend
            String intentAction = intent.Action;
            if (Intent.ActionSend.Equals(intentAction))
            {
                System.Diagnostics.Debug.WriteLine("Received ACTION_SEND intent");

                AndroidUri uri = (AndroidUri)intent.GetParcelableExtra(Intent.ExtraStream);
                if (uri != null)
                {
                    originalImageUri = uri;
                    selectedInputMethod = InputMethod.Sharing;
                    TryExtStorageDependentAction(LaunchPhotoCropper);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Unable to retrieve shared image URI");
                    Snackbar.Make(rootView, Resource.String.sharing_uri_null_error, Snackbar.LengthIndefinite)
                            .SetAction(Android.Resource.String.Ok, (v) => { })
                            .Show();
                }
            }
        }

        private void HandleActivityResultCancelled(RequestCode requestCode)
        {
            // This effectively represents a back navigation

            switch (requestCode)
            {
                case RequestCode.Results:
                    // ImageInput <- Photo Library/Camera <- Crop <- Results
                    //                                            ^^
                    //                                      this transition
                    //

                    // Relaunch the crop interface
                    TryExtStorageDependentAction(LaunchPhotoCropper);
                    RunBackAnimation();
                    break;

                case RequestCode.Crop:
                    // ImageInput <- Photo Library/Camera <- Crop <- Results
                    //                                    ^^
                    //                              this transition
                    //

                    // Relaunch the selected input method
                    switch (selectedInputMethod)
                    {
                        case InputMethod.ImageOpen:
                            TryExtStorageDependentAction(LaunchPhotoPicker);
                            RunBackAnimation();
                            break;

                        case InputMethod.Camera:
                            LaunchCamera();
                            RunBackAnimation();
                            break;

                        case InputMethod.Sharing:
                            RunBackAnimation();
                            break;
                    }
                    break;

                default:
                    // ImageInput <- Photo Library/Camera <- Crop <- Results
                    //            ^^
                    //       this transition
                    //

                    // This is just the default behavior, don't need to do anything (other than the animation)
                    RunBackAnimation();
                    break;
            }
        }

        private void HandleActivityResultOK(RequestCode requestCode, Intent data)
        {
            switch (requestCode)
            {
                case RequestCode.ImageOpen:
                    // ImageInput -> Photo Library/Camera -> Crop -> Analysis -> Results
                    //                                    ^^
                    //                              this transition
                    //

                    // Launch Crop
                    originalImageUri = data.Data;
                    TryExtStorageDependentAction(LaunchPhotoCropper);
                    RunForwardAnimation();
                    break;

                case RequestCode.Camera:
                    // ImageInput -> Photo Library/Camera -> Crop -> Analysis -> Results
                    //                                    ^^
                    //                              this transition
                    //

                    // Launch Crop (originalImageUri got set when we launched the camera)
                    TryExtStorageDependentAction(LaunchPhotoCropper);
                    RunForwardAnimation();
                    break;

                case RequestCode.Crop:
                    // ImageInput -> Photo Library/Camera -> Crop -> Analysis -> Results
                    //                                            ^^
                    //                                      this transition
                    //

                    // Launch Analysis
                    AndroidUri croppedImageUri = UCrop.GetOutput(data);
                    LaunchAnalysis(croppedImageUri);
                    RunForwardAnimation();
                    break;

                case RequestCode.Analysis:
                    // ImageInput -> Photo Library/Camera -> Crop -> Analysis -> Results
                    //                                                        ^^
                    //                                                  this transition
                    //

                    // Launch Analysis
                    LaunchResults(data);
                    RunForwardAnimation();
                    break;
            }
        }

        private void HandleActivityResultError(RequestCode requestCode, Intent data)
        {
            switch (requestCode)
            {
                case RequestCode.ImageOpen:
                    System.Diagnostics.Debug.WriteLine("RequestImageOpen error");
                    Snackbar.Make(rootView, Resource.String.image_input_result_error, Snackbar.LengthIndefinite).SetAction(Android.Resource.String.Ok, (v) => { }).Show();
                    break;

                case RequestCode.Camera:
                    Snackbar.Make(rootView, Resource.String.camera_result_error, Snackbar.LengthIndefinite).SetAction(Android.Resource.String.Ok, (v) => { }).Show();
                    break;

                case RequestCode.Crop:
                    Java.Lang.Throwable cropError = UCrop.GetError(data);
                    cropError.PrintStackTrace();
                    Snackbar.Make(rootView, Resource.String.crop_result_error, Snackbar.LengthIndefinite).SetAction(Android.Resource.String.Ok, (v) => { }).Show();
                    break;
            }
        }

        private void RunForwardAnimation()
        {
            OverridePendingTransition(Resource.Animation.slide_in_right, Resource.Animation.slide_out_left);
        }

        private void RunBackAnimation()
        {
            OverridePendingTransition(Resource.Animation.slide_in_left, Resource.Animation.slide_out_right);
        }

        /// <summary>
        /// Performs a permissions check for the READ_EXTERNAL_STORAGE permission and calls the action immediately if we have permissions,
        /// or requests permissions if we don't have them.
        /// </summary>
        /// <param name="action">The method to call if/when we are granted the permission.</param>
        private void TryExtStorageDependentAction(ActionPendingPermissionRequest action)
        {
            // Check if we need/already have permissions
            const string permission = Manifest.Permission.ReadExternalStorage;

            // Build.VERSION.SdkInt < 23 means the Android version doesn't support request individual permissions,
            // so the permission will have already been granted at install-time
            if ((int)Build.VERSION.SdkInt < 23 || CheckSelfPermission(permission) == (int)Permission.Granted)
            {
                action();
                return;
            }

            // We do need and didn't have permission
            actionPendingPermissionRequest = action;
            GetExtStoragePermission();
        }

        /// <summary>
        /// Either displays a message explaining why we need permissions, or immediately launches the permission request dialog
        /// depending on whether the user has previously denied permissions.
        /// </summary>
        private void GetExtStoragePermission()
        {
            const string permission = Manifest.Permission.ReadExternalStorage;

            if (ShouldShowRequestPermissionRationale(permission))
            {
                // The user has previously denied the permission so show a message explaining why we need it
                Snackbar.Make(rootView, Resource.String.permission_request_ext_storage, Snackbar.LengthIndefinite)
                    .SetAction(Android.Resource.String.Ok, v =>
                    {
                        // Launch the permission request dialog
                        RequestPermissions(new string[] { permission }, PermissionRequestExtStorage);
                    }).Show();
            }
            else
            {
                // Don't need to show an explanation for the permissions, so immediately launch the permission request dialog
                RequestPermissions(new string[] { permission }, PermissionRequestExtStorage);
            }
        }

        private void LaunchPhotoPicker()
        {
            // See https://developer.android.com/guide/topics/providers/document-provider.html#client for why Intent.ActionGetContent is used
            Intent intent = new Intent(Intent.ActionGetContent);
            intent.SetType("image/*");
            intent.AddCategory(Intent.CategoryOpenable);

            AnalyticsService.TrackEvent(AnalyticsService.Event.ImagePickerSelected, new Dictionary<string, string> {
                { AnalyticsService.PropertyKey.Method, AnalyticsService.PropertyValue.PhotoLibrary }
            });

            selectedInputMethod = InputMethod.ImageOpen;
            StartActivityForResult(intent, (int)RequestCode.ImageOpen);
            RunForwardAnimation();
        }

        //https://developer.xamarin.com/recipes/android/other_ux/camera_intent/take_a_picture_and_save_using_camera_app/
        private void LaunchCamera()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);

            // if there is an app to resolve the ActionImageCapture intent
            if (intent.ResolveActivity(PackageManager) != null)
            {
                intent.SetFlags(ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);

                File image = new File(CacheDir, FILENAME_CAMERA_IMAGE); //generate file

                originalImageUri = FileProvider.GetUriForFile(ApplicationContext, PackageName + ".provider", image); //get uri

                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    // Workaround to make the camera work on Android Version < 5.0 (Lollipop)
                    // https://stackoverflow.com/questions/33650632/fileprovider-not-working-with-camera/33652695#33652695
                    IList<ResolveInfo> resInfoList = this.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
                    foreach (ResolveInfo resolveInfo in resInfoList)
                    {
                        String packageName = resolveInfo.ActivityInfo.PackageName;
                        GrantUriPermission(packageName, originalImageUri, ActivityFlags.GrantReadUriPermission | ActivityFlags.GrantWriteUriPermission);
                    }
                }

                intent.PutExtra(MediaStore.ExtraOutput, originalImageUri);

                selectedInputMethod = InputMethod.Camera;
                StartActivityForResult(intent, (int)RequestCode.Camera);
                RunForwardAnimation();
            }
        }

        private void LaunchPhotoCropper()
        {
            ImageHelper.LaunchPhotoCropper(originalImageUri, this);
        }

        private void LaunchAnalysis(AndroidUri croppedImageUri)
        {
            Intent intent = AnalysisActivity.NewIntent(this, croppedImageUri, originalImageUri);
            StartActivityForResult(intent, (int)RequestCode.Analysis);
        }

        private void LaunchResults(Intent data)
        {
            Intent intent = new Intent(this, typeof(ResultsActivity));
            intent.PutExtras(data);
            StartActivityForResult(intent, (int)RequestCode.Results);
        }
    }
}
