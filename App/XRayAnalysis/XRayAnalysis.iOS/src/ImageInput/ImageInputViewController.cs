// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AVFoundation;
using CoreGraphics;
using Foundation;
using MobileCoreServices;
using UIKit;
using Wapps.TOCrop;
using XRayAnalysis.iOS.About;
using XRayAnalysis.iOS.Cropping;
using XRayAnalysis.iOS.Service.NavigationControllerUtil;
using XRayAnalysis.iOS.Service.UserDefaults;
using XRayAnalysis.Service.Analytics;

namespace XRayAnalysis.iOS.ImageInput
{
    /// <summary>
    /// Implementation of ImageInput.storyboard's UIViewController
    /// </summary>
    public partial class ImageInputViewController : UIViewController
    {
        private const float CardButtonShadowOpacity = 0.16f;
        private const float CardButtonCornerRadius = 2.0f;
        private const float CardButtonShadowOffsetWidth = 0.0f;
        private const float CardButtonShadowOffsetHeight = 4.0f;
        private const string ImportFromPhotoTitleText = "Import from Photos";
        private const string ImportFromPhotoDescriptionText = "Select a chest x-ray image from your \ncamera roll to begin analysis";
        private const string ImportFromFilesTitleText = "Import from Files";
        private const string ImportFromFilesDescriptionText = "Select a chest x-ray image to begin analysis";
        private const string ImportFromCameraTitleText = "Take a Photo";
        private const string ImportFromCameraDescriptionText = "Take a photo of a printed chest x-ray \nto begin analysis";

        private UIImagePickerController imagePicker;

        public ImageInputViewController(IntPtr handle) : base(handle)
        {
        }

        /// <summary>
        /// Initialization of the view <see cref="ImageInputViewController"/> 
        /// </summary>
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AnalyticsService.TrackEvent(AnalyticsService.Event.ImageInputPageViewed);

            // Set nav bar attributes
            NavigationControllerUtil.SetNavigationBarAttributes(this.NavigationController.NavigationBar);
            NavigationControllerUtil.SetNavigationTitle(this.NavigationItem, Constants.ApplicationName);

            SetButtonTouchEvents();
            SetLabelTexts();
            SetButtonBorderShadows();

            UIButton rightButton = new UIButton(UIButtonType.InfoLight);
            rightButton.TouchUpInside += NavBarButtonInfo_TouchUpInside;
            UIBarButtonItem rightBarButton = new UIBarButtonItem(customView: rightButton);

            this.NavigationItem.SetRightBarButtonItem(rightBarButton, true);

            //set the initial focus element
            UIAccessibility.PostNotification(UIAccessibilityPostNotification.ScreenChanged, ChoosePhotoButton);

            SetSmartInvertProperties();

            CheckLaunchFromShareExtension();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            this.NavigationController.NavigationBar.AccessibilityLabel = Constants.NavigationBarAccessibilityLabel_ImageInputPage;
        }

        /// <summary>
        /// Checks whether the app was launched from the share extension. If it is, the CropViewController is launched without user input.
        /// </summary>
		private void CheckLaunchFromShareExtension()
        {
            if (((AppDelegate)UIApplication.SharedApplication.Delegate).LaunchedFromShareExtension)
            {
                // reset to false to prevent this from executing when the user returns to home page
                ((AppDelegate)UIApplication.SharedApplication.Delegate).LaunchedFromShareExtension = false;

                NSUserDefaults sharedDefaults = new NSUserDefaults(Constants.AppGroupIdentifier, NSUserDefaultsType.SuiteName);
                NSData imageData = sharedDefaults.DataForKey(Constants.ShareExtensionImageKey);
                sharedDefaults.SetValueForKey(new NSData(), new NSString(Constants.ShareExtensionImageKey));

                try
                {
                    UIImage image = new UIImage(imageData);

                    //NOTE EXIF data is not retrieved
                    NSUrl url = null;

                    LaunchCropViewController(image, url, this.NavigationController);
                }
                catch (Exception e)
                {
                    // If data was not able to be retrieved
                    var alert = UIAlertController.Create("Error Launching from Share Extension",
                                                         "An unexpected error occured.",
                                                         UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);
                }
            }
        }

		private void SetSmartInvertProperties()
        {
            this.PhotoImportImage.AccessibilityIgnoresInvertColors = true;
            this.FileImportImage.AccessibilityIgnoresInvertColors = true;
            this.CameraImportImage.AccessibilityIgnoresInvertColors = true;
        }

        private void FinishedSelectingImageFromFiles(object sender, UIDocumentPickedAtUrlsEventArgs e)
        {
            try
            {
                NSUrl url = e.Urls[0];

                NSData imageData = NSData.FromUrl(url);
                UIImage image = new UIImage(imageData);

                LaunchCropViewController(image, url, this.NavigationController);
            }
            catch (Exception exception)
            {
                var alert = UIAlertController.Create("Invalid Selection",
                                                     "Please select an image instead.",
                                                     UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);
            }
        }

        private void CancelFilesView(object sender, EventArgs wasCancelledArgs)
        {
            AnalyticsService.TrackEvent(AnalyticsService.Event.FilesPageCancelled);
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        /// <summary>
        /// Launches the UIImagePickerController <see cref="imagePicker"/>  in mode "photo",
        /// allowing the user to access the image library. iOS 11 does not require explicit photo library access 
        /// permissions as the UIImagePickerController runs as a separate process from the app.
        /// (See https://stackoverflow.com/questions/46404628/ios11-photo-library-access-is-possible-even-if-settings-are-set-to-never)
        /// </summary>
        /// 
        /// <param name="sender">Button object that was pressed.</param>
        void ChoosePhotoButton_TouchUpInside(object sender, EventArgs ea)
        {
            // NOTE: UIImagePickerController only supports portrait mode (Based off: https://developer.apple.com/documentation/uikit/uiimagepickercontroller).
            CreateImagePicker(UIImagePickerControllerSourceType.PhotoLibrary);

            // show the picker
            NavigationController.PresentViewController(imagePicker, true, null);

            // NOTE: On iPads, this must be a ModalView or Apple rejects the app 
            // (per design guidelines for iOS: https://developer.apple.com/documentation/uikit/uiimagepickercontroller)
            // As this is only a sample application, this is does not need to be considered
        }

        /// <summary>
        /// Launches the file picker
        /// </summary>
        /// <param name="sender">Button object that was pressed</param>
        /// <param name="ea">Event Arguments</param>
        void ChooseFilesButton_TouchUpInside(object sender, EventArgs ea)
        {
            string[] documentTypes = new string[] { UTType.Data, UTType.Content };
            UIDocumentPickerViewController filePicker = new UIDocumentPickerViewController(documentTypes, UIDocumentPickerMode.Import);

            filePicker.WasCancelled += CancelFilesView;
            filePicker.DidPickDocumentAtUrls += FinishedSelectingImageFromFiles;

            NavigationController.PresentViewController(filePicker, true, null);
        }

        /// <summary>
        /// Launches the UIImagePickerController <see cref="imagePicker"/>  in mode "camera",
        /// allowing the user to access the image library.
        /// </summary>
        /// 
        /// <param name="sender">Button object that was pressed.</param>
        async void ChooseCameraButton_TouchUpInside(object sender, EventArgs ea)
        {
            bool permissionGranted = await HasCameraPermissions();

            if (permissionGranted)
            {
                System.Diagnostics.Debug.WriteLine("Permission granted");
                CreateImagePicker(UIImagePickerControllerSourceType.Camera);

                // show the camera
                NavigationController.PresentViewController(imagePicker, true, null);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Camera access denied");
                ShowEventsAccessDeniedAlert("Camera");
            }
        }

        /// <summary>
        /// Determines whether the app has access to the camera. If the user has never been asked,
        /// the app requests access and awaits the user's response.
        /// </summary>
        /// <returns>True if the user has given the app access to the camera. False otherwise.</returns>
        async private Task<bool> HasCameraPermissions()
        {
            // GetAuthorizationStatus is an enum. See https://developer.xamarin.com/api/type/MonoTouch.AVFoundation.AVAuthorizationStatus/
            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);
            System.Diagnostics.Debug.WriteLine(authorizationStatus);

            // if user has not specified been asked for permissions
            if (authorizationStatus == AVAuthorizationStatus.NotDetermined)
            {
                System.Diagnostics.Debug.WriteLine("User has not been asked for access yet. Asking user...");

                // ask for access to camera
                await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);

                // update authorizationStatus
                authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

                AnalyticsService.TrackEvent(AnalyticsService.Event.CameraPermission, new Dictionary<string, string> {
                    { AnalyticsService.PropertyKey.Selection, authorizationStatus.ToString() }
                });

                System.Diagnostics.Debug.WriteLine("Asked user for permissions. Current authorizationStatus is... " + authorizationStatus);
            }

            return authorizationStatus == AVAuthorizationStatus.Authorized;
        }

        /// <summary>
        /// Shows a UIAlertController that deep links to the app's settings page in the iOS Settings app. 
        /// This should be called when the app is not authorized to access <see cref="type"/>. This
        /// <see cref="type"/> should be anything protected by iOS with a privacy toggle: "Photos", "Camera", etc.
        /// </summary>
        /// <param name="type">The item that the app is not authorized to access.</param>
        private void ShowEventsAccessDeniedAlert(String type)
        {
            // get url to deep link into settings
            var deepLinkSettingsUrl = new NSUrl(UIKit.UIApplication.OpenSettingsUrlString);

            // create alert and populate the text fields
            var permissionsAlert = UIAlertController.Create(type + " Permissions",
                                                            "The app does not have permission to access your " + type.ToLower() + ". Please enter settings ",
                                                            UIAlertControllerStyle.Alert);

            //Add Actions
            permissionsAlert.AddAction(UIAlertAction.Create("Close", UIAlertActionStyle.Default, (UIAlertAction action) =>
            {
                AnalyticsService.TrackEvent(AnalyticsService.Event.CameraPermission, new Dictionary<string, string> {
                    { AnalyticsService.PropertyKey.Selection, action.Title }
                });
            }));

            permissionsAlert.AddAction(UIAlertAction.Create("Open Settings", UIAlertActionStyle.Default, (UIAlertAction action) =>
            {
                AnalyticsService.TrackEvent(AnalyticsService.Event.CameraPermission, new Dictionary<string, string> {
                    { AnalyticsService.PropertyKey.Selection, action.Title }
                });

                UIApplication.SharedApplication.OpenUrl(deepLinkSettingsUrl);
            }));

            //Present Alert
            PresentViewController(permissionsAlert, true, null);
        }

        /// <summary>
        /// Creates the UIImagePickerController picker in either camera or photo mode.
        /// Throws an ArgumentException if <see cref="sourceType"/> is neither "camera" nor "photo". 
        /// </summary>
        /// 
        /// <param name="mode">The source of UIImagePickerController.</param>
        private void CreateImagePicker(UIImagePickerControllerSourceType sourceType)
        {
            imagePicker = new UIImagePickerController();
            imagePicker.Delegate = this;
            NavigationControllerUtil.SetNavigationBarAttributes(imagePicker.NavigationBar);

            if (sourceType == UIImagePickerControllerSourceType.Camera)
            {
                // set our source to the camera
                imagePicker.SourceType = sourceType;

                // open camera in photo mode only
                imagePicker.CameraCaptureMode = UIImagePickerControllerCameraCaptureMode.Photo;
            }
            else if (sourceType == UIImagePickerControllerSourceType.PhotoLibrary)
            {
                // Set our source to the photo library
                // The 'availableMediaTypes' property determines the types of media presented in the interface.
                // By default, this is set to the single value kUTTypeImage, which specifies that only 
                // still images should be displayed in the media picker when browsing saved media.
                // See https://developer.apple.com/documentation/uikit/uiimagepickercontroller/1619173-mediatypes
                imagePicker.SourceType = sourceType;
            }
            else
            {
                // should never enter this
                throw new ArgumentException("Invalid UIIMagePickerControlerSourceType.");
            }

            AnalyticsService.TrackEvent(AnalyticsService.Event.ImagePickerSelected, new Dictionary<string, string> {
                {AnalyticsService.PropertyKey.Method, imagePicker.SourceType.ToString()}
            });

            // Set handlers
            imagePicker.FinishedPickingMedia += Handle_FinishedPickingMedia;
            imagePicker.Canceled += Handle_Canceled;
        }

        /// <summary>
        /// Closes the UIImagePickerController.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event.</param>
        void Handle_Canceled(object sender, EventArgs e)
        {
            AnalyticsService.TrackEvent(AnalyticsService.Event.ImagePickerCancelled, new Dictionary<string, string> {
                {AnalyticsService.PropertyKey.Method, imagePicker.SourceType.ToString()}
            });

            System.Diagnostics.Debug.WriteLine("Picker cancelled");
            imagePicker.DismissViewController(true, null);
        }

        /// <summary>
        /// The selected media object is verified to be a image. The UIImagePickerController is closed.
        /// The image is scaled to <see cref="ChestXRayAIClient.ImageInputWidth"/> by <see cref="ChestXRayAIClient.ImageInputHeight"/>
        /// before it is passed to the ML Model. If a video is selected, an alert is displayed before reverting to the ImageInput screen.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event.</param>
        protected void Handle_FinishedPickingMedia(object sender, UIImagePickerMediaPickedEventArgs e)
        {
            // determine what was selected, video or image
            bool isImage = false;
            switch (e.Info[UIImagePickerController.MediaType].ToString())
            {
                case "public.image":
                    System.Diagnostics.Debug.WriteLine("Image selected");
                    isImage = true;
                    break;

                case "public.video":
                    System.Diagnostics.Debug.WriteLine("Video selected");
                    break;
            }

            // if it was an image, scale it before passing to ML model
            if (isImage)
            {
                UIImage image = null;

                image = e.Info[UIImagePickerController.EditedImage] as UIImage;
                if (image == null)
                {
                    image = e.Info[UIImagePickerController.OriginalImage] as UIImage;
                }


                NSUrl url = e.Info[UIImagePickerController.ImageUrl] as NSUrl;

                imagePicker.DismissViewController(true, null);

                // Launch the cropping view
                LaunchCropViewController(image, url, this.NavigationController);
            }
            else
            {
                // if it's a video
                var alert = UIAlertController.Create("Video Selected",
                                                     "Unfortunately we are unable to analyse videos. Please select an image instead.",
                                                     UIAlertControllerStyle.Alert);

                //Add Actions
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

                //Present Alert
                PresentViewController(alert, true, null);
            }
        }

        /// <summary>
        /// Launches the About view controller.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="eventArgs">Event arguments.</param>
        private void NavBarButtonInfo_TouchUpInside(object sender, EventArgs eventArgs)
        {
            // Get about storyboard
            UIStoryboard aboutStoryboard = UIStoryboard.FromName("About", null);

            // Grab about view Controller reference
            AboutViewController aboutViewController = aboutStoryboard.InstantiateInitialViewController() as AboutViewController;

            // Push about view controller onto stack
            this.NavigationController.PushViewController(aboutViewController, true);
        }

        /// <summary>
        /// Creates and launches a new <see cref="TOCropViewController"/>
        /// </summary>
        /// <param name="image">Image to crop</param>
        void LaunchCropViewController(UIImage image, NSUrl imageUrl, UINavigationController nav)
        {

            // Instantiate new <see cref="CropViewController"/>
            TOCropViewController cropViewController = new CropViewController(TOCropViewCroppingStyle.Default, image, imageUrl);
         
            // Push the cropViewController
            nav.PushViewController(cropViewController, true);

            // Set the navbar styling
            NavigationControllerUtil.SetNavigationBarAttributes(nav.NavigationBar);
        }

        /// <summary>
        /// Sets touch events for the buttons
        /// </summary>
        private void SetButtonTouchEvents()
        {
            this.ChoosePhotoButton.TouchUpInside += ChoosePhotoButton_TouchUpInside;
            this.ChooseFilesButton.TouchUpInside += ChooseFilesButton_TouchUpInside;
            this.ChooseCameraButton.TouchUpInside += ChooseCameraButton_TouchUpInside;
        }

        /// <summary>
        /// Sets border shadows for the buttons to look like cards.
        /// </summary>
        private void SetButtonBorderShadows()
        {
            ChoosePhotoButton.Layer.ShadowOffset = ChooseFilesButton.Layer.ShadowOffset = ChooseCameraButton.Layer.ShadowOffset = new CGSize(CardButtonShadowOffsetWidth, CardButtonShadowOffsetHeight);
            ChoosePhotoButton.Layer.ShadowColor = ChooseFilesButton.Layer.ShadowColor = ChooseCameraButton.Layer.ShadowColor = UIColor.Black.CGColor;
            ChoosePhotoButton.Layer.ShadowOpacity = ChooseFilesButton.Layer.ShadowOpacity = ChooseCameraButton.Layer.ShadowOpacity = CardButtonShadowOpacity;
            ChoosePhotoButton.Layer.CornerRadius = CardButtonCornerRadius;
        }

        /// <summary>
        /// Sets static text for labels.
        /// </summary>
        private void SetLabelTexts()
        {
            this.ImportFromPhotoTitle.Text = ImportFromPhotoTitleText;
            this.ImportFromPhotosDescription.Text = ImportFromPhotoDescriptionText;
            this.ImportFromFilesTitle.Text = ImportFromFilesTitleText;
            this.ImportFromFilesDescription.Text = ImportFromFilesDescriptionText;
            this.ImportFromCameraTitle.Text = ImportFromCameraTitleText;
            this.ImportFromCameraDescription.Text = ImportFromCameraDescriptionText;
        }
    }
}

