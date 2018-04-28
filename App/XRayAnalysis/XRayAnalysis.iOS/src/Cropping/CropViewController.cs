// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using CoreGraphics;
using XRayAnalysis.iOS.Analysis;
using Foundation;
using SkiaSharp;
using SkiaSharp.Views.iOS;
using UIKit;
using Wapps.TOCrop;
namespace XRayAnalysis.iOS.Cropping
{
    /// <summary>
    /// Sub class of <see cref="TOCropViewController"/>
    /// </summary>
    public class CropViewController : TOCropViewController
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CropViewController"/> and <see cref="TOCropViewController"/> base class
        /// </summary>
        /// <param name="style">Cropping style</param>
        /// <param name="image">Image to be cropped</param>
        public CropViewController(TOCropViewCroppingStyle style, UIImage image, NSUrl imageUrl) : base(style, image)
        {
            // Initialize this views properties
            SetAspectRatioPreset(TOCropViewControllerAspectRatioPreset.Square, false);
            AspectRatioLockEnabled = true;
            AspectRatioPickerButtonHidden = true;
            ResetAspectRatioEnabled = false;

            // Set the associated delegate
            this.Delegate = new CropViewControllerDelegate(imageUrl);
        }

        /// <summary>
        /// Never show the status bar in this view controller
        /// </summary>
        public override bool PrefersStatusBarHidden()
        {
            return true;
        }

        /// <summary>
        /// Implementation of <see cref="TOCropViewControllerDelegate"/>
        /// Consumed by <see cref="CropViewController"/> above
        /// </summary>
        private sealed class CropViewControllerDelegate : TOCropViewControllerDelegate
        {
            private NSUrl imageUrl;

            /// <summary>
            /// Initializes a new instance of the
            /// <see cref="CropViewControllerDelegate"/> class.
            /// </summary>
            /// <param name="url">The url of the image passed to cropping</param>
            public CropViewControllerDelegate(NSUrl url)
            {
                this.imageUrl = url;
            }

            /// <summary>
            /// On crop to rectangle complete
            /// ---------------
            /// NOTE:
            /// As of 23/04/2018, the cropping library utilized (TOCropViewController) has the following open issue on GitHub:
            /// https://github.com/TimOliver/TOCropViewController/issues/239
            /// 
            /// Steps to repro:
            /// 1. Select a non-square image
            /// 2. Zoom and/or pan on the image
            /// 3. Click the reset button at the bottom (2nd button from the right)
            /// 4. Click the done button
            ///
            /// Expected:
            /// cropViewController.FinalImage will be square
            /// 
            /// Result:
            /// cropViewController.FinalImage will NOT be square:
            /// FinalImage Width and FinalImage Height will differ by a pixel (from the repro attempts conducted)
            /// 
            /// Work around used:
            /// 
            /// [Let diff = absolute(FinalImage.Width - FinalImage.Height)]
            /// 
            /// 1. If {diff} > 1 px: throw error pop-up. This case is where the user 
            ///    purposely dragged the rectangular image out and then hitting done.
            ///    Return from function.
            ///
            /// 2. If 1px >= {diff} > 0px: set the outImageBitmap to a square cropping,
            ///    thus removing the {diff} offset on either the width or height
            /// ---------------
            /// </summary>
            /// <param name="cropVC">TOCropViewController object</param>
            /// <param name="cropRect">Rectangled cropped</param>
            /// <param name="angle">Angle at which cropped</param>
            public override void DidCropImageToRect(TOCropViewController cropViewController, CGRect cropRect, nint angle)
            {
                UIImage outImage = cropViewController.FinalImage;
                SKBitmap outImageBitmap = outImage.ToSKBitmap();
                nfloat width = outImage.Size.Width, height = outImage.Size.Height;

                // Work-around epsilon
                const float epsilon = 1.0f;

                // Case 1 (See comment on work-around above) 
                if (Math.Abs(width - height) > epsilon)
                {
                    // if it's a non-square image
                    var alert = UIAlertController.Create("Cropping Error",
                                                         "The AI model can only except a 1:1 aspect ratio. Please crop to a square.",
                                                         UIAlertControllerStyle.Alert);

                    // Add Actions
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

                    // Present Alert
                    cropViewController.PresentViewController(alert, true, null);

                    return;
                }
                else if (width != height)
                {
                    // Case 2 (See comment on work-around above)
                    SKBitmap image = outImage.ToSKBitmap();
                    int minDimension = (int)Math.Min(width, height);

                    // Crop to the minimum dimension
                    image.ExtractSubset(outImageBitmap, new SKRectI(0, 0, minDimension, minDimension));
                }

                // Get analysis storyboard
                UIStoryboard analysisStoryboard = UIStoryboard.FromName("Analysis", null);

                // Grab analysis view Controller reference
                AnalysisViewController analysisViewController = analysisStoryboard.InstantiateInitialViewController() as AnalysisViewController;
                analysisViewController.InputImage = outImage;
                analysisViewController.ImageUrl = imageUrl;
                analysisViewController.InputImageBitmap = outImageBitmap;

                // Push analysis view controller onto stack
                cropViewController.NavigationController.PushViewController(analysisViewController, true);
            }
        }
    }
}
