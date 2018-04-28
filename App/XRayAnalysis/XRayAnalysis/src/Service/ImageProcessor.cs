// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using XRayAnalysis.Service.AI.ChestXRay;
using SkiaSharp;

namespace XRayAnalysis.Service
{
    /// <summary>
    /// Contains functions that manipulate images.
    /// </summary>
    public static class ImageProcessor
    {
        // NOTE: For blending modes, see: https://helpx.adobe.com/photoshop/using/blending-modes.html
        private const SKBlendMode CAMBlendMode = SKBlendMode.Color;
        private const float ExpMappingBase = 1.1F;

        /// <summary>
        /// Resizes the input image to <see cref="targetHeight"/> by <see cref="targetWidth"/>. 
        /// The image MUST have the correct aspect ratio.
        /// </summary>
        /// <param name="image">The image to pre-process. Must have the correct aspect ratio (or else the image will get distorted).</param>
        /// <param name="targetHeight">The target height to resize to.</param>
        /// <param name="targetWeight">The target weight to resize to.</param>
        /// <returns>The processed image of dimensions <see cref="targetHeight"/> by <see cref="targetWidth"/>.</returns>
        public static SKBitmap ResizeImagePreservingAspectRatio(SKBitmap image, int targetWidth, int targetHeight)
        {
            float targetRatio = (float)targetWidth / targetHeight;
            float imageRatio = (float)image.Width / image.Height;

            const float epsilon = 1e-9f;

            //if the image has the incorrect aspect ratio, throw exception
            if (Math.Abs(targetRatio - imageRatio) > epsilon)
            {
                String errorMessage = String.Format("The image has the incorrect ratio: {0}x{1}", image.Width, image.Height);
                throw new ArgumentException(errorMessage);
            }

            //if the image is not targetHeight by targetWidth
            if (image.Width != targetWidth || image.Height != targetHeight)
            {
                return image.Resize(new SKImageInfo(targetWidth, targetHeight), SKBitmapResizeMethod.Lanczos3);
            }
            return image;
        }

        /// <summary>
        /// Overlays 'overlayImage' over 'originalImage' with <see cref="CAMBlendMode"/> 
        /// </summary>
        /// <returns>The resulting image.</returns>
        /// <param name="originalImage">The image to use as the background.</param>
        /// <param name="overlayImage">The image to overlay on the background.</param>
        public static SKBitmap OverlayImage(SKBitmap originalImage, SKBitmap overlayImage)
        {
            if (originalImage.Width != overlayImage.Width || originalImage.Height != overlayImage.Height)
            {
                String errorMessage = String.Format("originalImage and overlayImage are not the same dimensions: original ~ {0}x{1} | overlay ~ {2}x{3}", 
                                                    originalImage.Width, originalImage.Height,
                                                    overlayImage.Width, overlayImage.Height);
                throw new ArgumentException(errorMessage);
            }

            SKBitmap output = originalImage.Copy();
            SKCanvas skCanvas = new SKCanvas(output);

            SKPaint skPaint = new SKPaint();
            skPaint.BlendMode = CAMBlendMode;

            skCanvas.DrawBitmap(overlayImage, 0, 0, skPaint);

            skCanvas.Flush();

            return output;
        }

        /// <summary>
        /// Generates an <see cref="SKBitmap"/> of size width x height that represents a set of CAM values mapped over shades of a particular color
        /// and interpolated to fill the size.
        /// i.e. The areas with a higher CAM value will appear as a higher intensity of the provided color.
        /// Note: All channels of the input color are mapped (Red, Green, Blue, Alpha).
        /// </summary>
        /// <param name="cam">The CAM data to map.</param>
        /// <param name="color">The color to map the CAM values over.</param>
        /// <param name="width">The desired width of the resulting color-map.</param>
        /// <param name="height">The desired height of the resulting color-map.</param>
        /// <returns>The color-mapped CAM as a <see cref="SKBitmap"/>.</returns>
        public static SKBitmap GenerateColorMap(CAM cam, SKColor color, int width, int height)
        {
            // Create an array to hold the output
            float[,] expCAMData = new float[cam.Rows, cam.Cols];

            // Map the input data onto an exponential curve (retain higher intensities more than lower intensities)
            for (int row = 0; row < cam.Rows; row++)
            {
                for (int col = 0; col < cam.Cols; col++)
                {
                    expCAMData[row, col] = (float)Math.Pow(ExpMappingBase, cam[row, col]);
                }
            }
            CAM expCAM = new CAM(expCAMData);

            // Normalize the new CAM to be between 0 and 1
            CAM normalizedCAM = expCAM.Normalize();

            // Create a bitmap to hold the output
            SKBitmap bitmap = new SKBitmap(new SKImageInfo(cam.Cols, cam.Rows, SKColorType.Rgba8888));

            // Iterate through and fill the bitmap
            for (int row = 0; row < cam.Rows; row++)
            {
                for (int col = 0; col < cam.Cols; col++)
                {
                    float camVal = normalizedCAM[row, col];

                    // SetPixel accepts an x and y coordinate: the row is the y coordinate, the column is the x coordinate
                    bitmap.SetPixel(col, row, new SKColor(
                        (byte)(color.Red * camVal),
                        (byte)(color.Green * camVal),
                        (byte)(color.Blue * camVal),
                        (byte)(color.Alpha * camVal)
                    ));
                }
            }

            // Resize the bitmap (with interpolation) to the desired width and height
            return bitmap.Resize(new SKImageInfo(width, height), SKBitmapResizeMethod.Lanczos3);
        }
    }
}
