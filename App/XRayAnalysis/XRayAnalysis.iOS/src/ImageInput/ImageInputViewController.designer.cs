// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace XRayAnalysis.iOS.ImageInput
{
    [Register ("ImageInputViewController")]
    partial class ImageInputViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView CameraImportImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ChooseCameraButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ChooseFilesButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ChoosePhotoButton { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView FileImportImage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ImportFromCameraDescription { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ImportFromCameraTitle { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ImportFromFilesDescription { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ImportFromFilesTitle { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ImportFromPhotosDescription { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel ImportFromPhotoTitle { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView PhotoImportImage { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CameraImportImage != null) {
                CameraImportImage.Dispose ();
                CameraImportImage = null;
            }

            if (ChooseCameraButton != null) {
                ChooseCameraButton.Dispose ();
                ChooseCameraButton = null;
            }

            if (ChooseFilesButton != null) {
                ChooseFilesButton.Dispose ();
                ChooseFilesButton = null;
            }

            if (ChoosePhotoButton != null) {
                ChoosePhotoButton.Dispose ();
                ChoosePhotoButton = null;
            }

            if (FileImportImage != null) {
                FileImportImage.Dispose ();
                FileImportImage = null;
            }

            if (ImportFromCameraDescription != null) {
                ImportFromCameraDescription.Dispose ();
                ImportFromCameraDescription = null;
            }

            if (ImportFromCameraTitle != null) {
                ImportFromCameraTitle.Dispose ();
                ImportFromCameraTitle = null;
            }

            if (ImportFromFilesDescription != null) {
                ImportFromFilesDescription.Dispose ();
                ImportFromFilesDescription = null;
            }

            if (ImportFromFilesTitle != null) {
                ImportFromFilesTitle.Dispose ();
                ImportFromFilesTitle = null;
            }

            if (ImportFromPhotosDescription != null) {
                ImportFromPhotosDescription.Dispose ();
                ImportFromPhotosDescription = null;
            }

            if (ImportFromPhotoTitle != null) {
                ImportFromPhotoTitle.Dispose ();
                ImportFromPhotoTitle = null;
            }

            if (PhotoImportImage != null) {
                PhotoImportImage.Dispose ();
                PhotoImportImage = null;
            }
        }
    }
}