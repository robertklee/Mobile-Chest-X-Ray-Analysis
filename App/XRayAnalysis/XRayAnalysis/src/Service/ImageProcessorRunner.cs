// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using XRayAnalysis.Service.AI.ChestXRay;
using SkiaSharp;

namespace XRayAnalysis.Service
{
    public static class ImageProcessorRunner
    {
        private const float ExpMappingBase = 1.1F;

        /// <summary>
        /// Returns a task to generate a CAM color map from a CAM object.
        /// </summary>
        /// <param name="cam">The CAM object.</param>
        /// <param name="color">Color for the color map.</param>
        /// <param name="width">Width of the generated color map.</param>
        /// <param name="height">Height of the generated color map.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the task returned by this method.</param>
        /// <returns>The CAM color map SKBitmap.</returns>
        public static Task<SKBitmap> GetCAMColorMapTask(CAM cam, SKColor color, int width, int height, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                SKBitmap colorMap = ImageProcessor.GenerateColorMap(cam, color, width, height);

                if (cancellationToken.IsCancellationRequested)
                {
                    colorMap.Dispose();
                    cancellationToken.ThrowIfCancellationRequested();
                }

                System.Diagnostics.Debug.WriteLine("CAM color map generated");

                return colorMap;
            }, cancellationToken);
        }

        /// <summary>
        /// Returns a task to overlay an image on another image.
        /// </summary>
        /// <param name="backgroundImage">The image to use as the background image.</param>
        /// <param name="overlayImage">The image to overlay.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the task returned by this method.</param>
        /// <returns>The overlayed image blended with the background image.</returns>
        public static Task<SKBitmap> GetOverlayImageTask(SKBitmap backgroundImage, SKBitmap overlayImage, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                SKBitmap blendedSKBitmap = ImageProcessor.OverlayImage(backgroundImage, overlayImage);

                System.Diagnostics.Debug.WriteLine("CAM image overlayed");

                if (cancellationToken.IsCancellationRequested)
                {
                    blendedSKBitmap.Dispose();
                    cancellationToken.ThrowIfCancellationRequested();
                }

                return blendedSKBitmap;
            }, cancellationToken);
        }
    }
}
