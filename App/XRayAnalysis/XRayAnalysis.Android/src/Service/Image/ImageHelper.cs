// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Support.V4.Content;
using Android.Text.Format;
using AndroidUri = Android.Net.Uri;
using Com.Yalantis.Ucrop;
using Java.IO;

namespace XRayAnalysis.Droid.Service.Image
{
    /// <summary>
    /// Utility class for operations on/with Bitmaps on Android.
    /// </summary>
    public static class ImageHelper
    {
        /// <summary>
        /// Loads a <see cref="Bitmap"/> from the provided <see cref="Android.Net.Uri"/> using the provided <see cref="ContentResolver"/>.
        /// </summary>
        /// <param name="contentResolver">A <see cref="ContentResolver"/> to use to load the image.</param>
        /// <param name="uri">A <see cref="Android.Net.Uri"/> that identifies the image.</param>
        /// <param name="reqWidth">The requested width for the loaded <see cref="Bitmap"/>. The actual width after the bitmap is loaded may be larger, but will not be smaller.</param>
        /// <param name="reqHeight">The requested height for the loaded <see cref="Bitmap"/>. The actual height after the bitmap is loaded may be larger, but will not be smaller.</param>
        /// <returns></returns>
        public static Bitmap GetBitmapFromUri(ContentResolver contentResolver, AndroidUri uri, int reqWidth, int reqHeight)
        {
            ParcelFileDescriptor parcelFileDescriptor = contentResolver.OpenFileDescriptor(uri, "r");
            FileDescriptor fileDescriptor = parcelFileDescriptor.FileDescriptor;

            // Load just the dimensions of the image (doesn't actually allocate memory for the bitmap)
            BitmapFactory.Options options = new BitmapFactory.Options();
            options.InJustDecodeBounds = true;
            BitmapFactory.DecodeFileDescriptor(fileDescriptor, null, options);

            // Calculate inSampleSize so we can subsample the image appropriately.
            options.InSampleSize = CalculateInSampleSize(options.OutWidth, options.OutHeight, reqWidth, reqHeight);

            // Load the image into a Bitmap at the determined subsampling
            options.InJustDecodeBounds = false;
            Bitmap image = BitmapFactory.DecodeFileDescriptor(fileDescriptor, null, options);

            parcelFileDescriptor.Close();
            return image;
        }

        /// <summary>
        /// Calculates the <see cref="BitmapFactory.Options.InSampleSize"/> that needs to be used to decode an <code>imgWidth x imgHeight</code> image
        /// into a <see cref="Bitmap"/> of size as close as possible and no smaller than <code>reqWidth x reqHeight</code>.
        /// </summary>
        /// <param name="imgWidth">The width of the actual image.</param>
        /// <param name="imgHeight">The height of the actual image.</param>
        /// <param name="reqWidth">The requested width for the image when loaded as a <see cref="Bitmap"/>.</param>
        /// <param name="reqHeight">The request height for the image when loaded as a <see cref="Bitmap"/></param>
        /// <returns>The value that should be used for <see cref="BitmapFactory.Options.InSampleSize"/>.</returns>
        private static int CalculateInSampleSize(int imgWidth, int imgHeight, int reqWidth, int reqHeight)
        {
            int inSampleSize = 1;

            // Check if the image is actually too big
            if (imgHeight > reqHeight || imgWidth > reqWidth)
            {
                int halfHeight = imgHeight / 2;
                int halfWidth = imgWidth / 2;

                // Calculate the largest inSampleSize value that is a power of 2 and keeps both
                // height and width larger than the requested height and width.
                while ((halfHeight / inSampleSize) >= reqHeight && (halfWidth / inSampleSize) >= reqWidth)
                {
                    inSampleSize *= 2;
                }
            }

            return inSampleSize;
        }

        /// <summary>
        /// Loads a <see cref="Bitmap"/> from the provided <see cref="Android.Net.Uri"/> using the provided <see cref="ContentResolver"/>.
        /// WARNING: This method loads the full resolution image and may cause <see cref="Java.Lang.OutOfMemoryError"/>s for images that are too large.
        /// </summary>
        /// <param name="contentResolver">A <see cref="ContentResolver"/> to use to load the image.</param>
        /// <param name="uri">A <see cref="Android.Net.Uri"/> that identifies the image.</param>
        /// <returns></returns>
        public static Bitmap GetBitmapFromUri(ContentResolver contentResolver, AndroidUri uri)
        {
            ParcelFileDescriptor parcelFileDescriptor = contentResolver.OpenFileDescriptor(uri, "r");
            FileDescriptor fileDescriptor = parcelFileDescriptor.FileDescriptor;
            Bitmap image = BitmapFactory.DecodeFileDescriptor(fileDescriptor);
            parcelFileDescriptor.Close();
            return image;
        }

        private const string FILENAME_CROPPED_IMAGE = "xray_cropped.png";

        /// <summary>
        /// Launches the photo cropper.
        /// </summary>
        /// <param name="sourceUri">The URI of the source image.</param>
        /// <param name="activity">An <see cref="Activity"/> to use to obtain resources.</param>
        /// <returns>The URI where the cropped image will be saved.</returns>
        public static AndroidUri LaunchPhotoCropper(AndroidUri sourceUri, Activity activity)
        {
            File dest = new File(activity.CacheDir, FILENAME_CROPPED_IMAGE);
            AndroidUri destUri = AndroidUri.FromFile(dest);

            UCrop.Options options = new UCrop.Options();
            options.SetCompressionFormat(Bitmap.CompressFormat.Png);
            options.SetToolbarColor(ContextCompat.GetColor(activity, Resource.Color.color_primary));
            options.SetStatusBarColor(ContextCompat.GetColor(activity, Resource.Color.color_primary_dark));
            options.SetActiveWidgetColor(ContextCompat.GetColor(activity, Resource.Color.color_accent));
            options.SetToolbarTitle(activity.GetString(Resource.String.crop_title));

            UCrop.Of(sourceUri, destUri)
                 .WithAspectRatio(1, 1)
                 .WithOptions(options)
                 .Start(activity);

            return destUri;
        }

        private const string DefaultFilename = "X-Ray.png";

        /// <summary>
        /// Extracts the filename from an image <see cref="Android.Net.Uri"/>.
        /// </summary>
        /// <param name="imageUri">The URI of the image.</param>
        /// <param name="contentResolver">A <see cref="ContentResolver"/> to use to obtain data from the URI.</param>
        /// <returns>The filename if found, otherwise a default filename.</returns>
        public static string ExtractFilename(AndroidUri imageUri, ContentResolver contentResolver)
        {
            string[] projection = { MediaStore.MediaColumns.DisplayName };
            ICursor metaCursor = contentResolver.Query(imageUri, projection, null, null, null);
            if (metaCursor != null)
            {
                try
                {
                    if (metaCursor.MoveToFirst())
                    {
                        return metaCursor.GetString(0);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Error retrieving filename:\n" + e);
                }
                finally
                {
                    metaCursor.Close();
                }
            }
            return DefaultFilename;
        }

        /// <summary>
        /// Extracts the date from an image <see cref="Android.Net.Uri"/>.
        /// </summary>
        /// <param name="imageUri">The URI of the image.</param>
        /// <param name="contentResolver">A <see cref="ContentResolver"/> to use to obtain data from the URI.</param>
        /// <returns>The date the image was captured on.</returns>
        public static string ExtractDate(AndroidUri imageUri, ContentResolver contentResolver, Context context)
        {
            // Get documentId of imageUri
            ICursor cursor = contentResolver.Query(imageUri, null, null, null, null);
            cursor.MoveToFirst();
            string documentId = cursor.GetString(0);
            try
            {
                documentId = documentId.Split(':')[1];
            } 
            catch (System.IndexOutOfRangeException) 
            {
                // image input method chosen was camerea, set image date as today's date
                DateTime todayDate = DateTime.Today;
                // convert date to a string in the proper format
                String todayDateString = todayDate.ToString("MMMM dd, yyyy");
                return todayDateString;
            }
            cursor.Close();

            // Query to get to the correct place to be able to get the file path from
            cursor = contentResolver.Query(
            MediaStore.Images.Media.ExternalContentUri,
            null, MediaStore.Images.Media.InterfaceConsts.Id + " = ? ", new[] { documentId }, null);
            cursor.MoveToFirst();
            String path = "";
            try
            {
                // get path of imageUri
                path = cursor.GetString(cursor.GetColumnIndex(MediaStore.Images.Media.InterfaceConsts.Data));
            } 
            catch (Android.Database.CursorIndexOutOfBoundsException) 
            {
                // image does not have a date, return the default date string
                return context.GetString(Resource.String.xray_display_date_not_available);
            }
            cursor.Close();

            // Get creation date from imageUri path 
            DateTime createDate = System.IO.File.GetCreationTime(path);

            // convert date to a string in the proper format
            String dateString = createDate.ToString("MMMM dd, yyyy");

            return dateString;
        }
    }
}