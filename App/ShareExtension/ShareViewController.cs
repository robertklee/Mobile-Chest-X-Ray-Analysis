using System;

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Foundation;
using Social;
using UIKit;
using CoreAnimation;
using MobileCoreServices;

namespace ShareExtension
{
    public partial class ShareViewController : UIViewController
	{
        private UIImage uiImage;

		protected ShareViewController(IntPtr handle) : base(handle)
		{
		}

		public override void DidReceiveMemoryWarning()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning();

			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

            System.Diagnostics.Debug.WriteLine("Entered Share Extension ViewDidLoad()");

            SetShareExtensionAttributes();

            // Retrieve data from host application through NSExtensionContext
            NSExtensionContext nsExtensionContext = this.ExtensionContext;
            NSItemProvider[] inputItems = nsExtensionContext.InputItems[0]?.Attachments;

            if (inputItems.Length != 1 || !(inputItems[0].HasItemConformingTo(UTType.Image)))
            {
                // If more than one image or a non-image was selected
                DisplayAlert("Image Selection Error", "Please select a single image to import.", "A single image was not selected.");
            }

            // Load image from NSItemProvider and set it to (UIImageView)ImageView
            inputItems[0].LoadItem(UTType.Image, null, (NSObject image, NSError error) =>
            {
                NSUrl url = image as NSUrl;
                if (url != null)
                {
                    uiImage = UIImage.LoadFromData(NSData.FromUrl(url));
                    InitWithImage(uiImage);
                    return;
                }

                uiImage = image as UIImage;
                if (uiImage != null)
                {
                    InitWithImage(uiImage);
                    return;
                }
            });

            // Set up touch events
            this.CancelButton.TouchUpInside += CancelButton_TouchUpInside;
            this.ImportButton.TouchUpInside += ImportButton_TouchUpInside;
		}

        /// <summary>
        /// Sets the attributes of the Share Extension View
        /// </summary>
        private void SetShareExtensionAttributes()
        {
            this.SharePopUp.Layer.CornerRadius = ShareExtensionConstants.CornerRadius;
            this.SharePopUp.ClipsToBounds = true;
            this.TopBar.Layer.CornerRadius = ShareExtensionConstants.CornerRadius;
            this.TopBar.Layer.MaskedCorners = CACornerMask.MinXMinYCorner | CACornerMask.MaxXMinYCorner;
            this.CancelButton.SetTitle("Cancel", UIControlState.Normal);
            this.CancelButton.AccessibilityLabel = "Cancel";
            this.CancelButton.SetTitleColor(ShareExtensionConstants.Purple, UIControlState.Normal);
            this.CenterLabel.Text = ShareExtensionConstants.ApplicationName;
            this.CenterLabel.TextColor = ShareExtensionConstants.Purple;
            this.CenterLabel.Font = UIFont.BoldSystemFontOfSize(ShareExtensionConstants.FontSize);
            this.ImportButton.SetTitle("Import", UIControlState.Normal);
            this.ImportButton.AccessibilityLabel = "Import";
            this.ImportButton.SetTitleColor(ShareExtensionConstants.Purple, UIControlState.Normal);
            this.ImageView.Layer.CornerRadius = ShareExtensionConstants.CornerRadius;
            this.ImageView.Layer.MaskedCorners = CACornerMask.MinXMaxYCorner | CACornerMask.MaxXMaxYCorner;
        }

        /// <summary>
        /// Displays an UIAlertController
        /// </summary>
        /// <param name="title">Title of the Alert.</param>
        /// <param name="content">Body text of the alert.</param>
        /// <param name="errorMessage">Error message to send to ExtensionContext.CancelRequest.</param>
        private void DisplayAlert(string title, string content, string errorMessage)
        {
            UIAlertController alert = UIAlertController.Create(title, content, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, (UIAlertAction obj) =>
            {
                // Dismiss ShareViewController
                this.ExtensionContext?.CancelRequest(new NSError((NSString)errorMessage, 0));
            }));
            PresentViewController(alert, true, null);
        }

        /// <summary>
        /// Sets the image of the Share Extension View
        /// </summary>
        /// <param name="image">Image.</param>
		void InitWithImage (UIImage image) 
        {
            InvokeOnMainThread( () =>
            {
                this.ImageView.Image = image;
                // If the image is wider than it is tall, it is set to ScaleAspectFill as otherwise it would leave 
                // an awkward amount of white space at the top and bottom of the image.
                // TODO dynamically adjust size of (UIImageView) ImageView based on size of image
                this.ImageView.ContentMode = (image.Size.Width > image.Size.Height) ? UIViewContentMode.ScaleAspectFill : UIViewContentMode.ScaleAspectFit;
            });
        }

        /// <summary>
        /// Dismisses the Share Extension View
        /// </summary>
        private void CancelButton_TouchUpInside(object sender, EventArgs eventArgs)
        {
            // Dismiss ShareViewController
            this.ExtensionContext?.CancelRequest(new NSError((NSString)"ShareViewController cancelled by user.", 0));
        }

        /// <summary>
        /// Saves the shared image into App Group's NSUserDefaults, and launches containing application.
        /// </summary>
        private void ImportButton_TouchUpInside(object sender, EventArgs eventArgs)
        {
            // Use the UserDefaults from the App Group in order to share data to containing app
            NSUserDefaults sharedDefaults = new NSUserDefaults(ShareExtensionConstants.AppGroupIdentifier, NSUserDefaultsType.SuiteName);

            // Store shared image as NSData into NSUserDefaults
            sharedDefaults.SetValueForKey(uiImage.AsPNG(), new NSString(ShareExtensionConstants.NSUserDefaultsKeyForImage));

            // Launch containing application using UrlScheme. Containing app MUST implement this UrlScheme
            UIApplication.SharedApplication.OpenUrl(new NSUrl(ShareExtensionConstants.UrlScheme + ShareExtensionConstants.NSUserDefaultsKeyForImage));

            this.ExtensionContext?.CompleteRequest(new NSExtensionItem[0], null);
        }
	}
}
