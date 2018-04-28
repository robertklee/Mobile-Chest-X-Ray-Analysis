// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace XRayAnalysis.Service.AI.ChestXRay
{
    /// <summary>
    /// A class containing static methods for running <see cref="ChestXRayAIClient"/> operations.
    /// </summary>
    public static class ChestXRayAIRunner
    {
        /// <summary>
        /// Calculate scores and CAMs for the provided image using the provided <see cref="ChestXRayAIClient"/>.
        /// This method is async so all processing will be performed in background threads if called using <code>await</code>.
        /// </summary>
        /// <param name="aiClient">A <see cref="ChestXRayAIClient"/> implementation to use for analysis.</param>
        /// <param name="inputImage">The image to analyse.</param>
        /// <param name="cancellationToken">
        ///     A cancellation token that can be used to cancel the analysisTasks.
        ///     A <see cref="OperationCanceledException"/> will be thrown if a cancellation is triggered.
        /// </param>
        /// <returns>The score and CAM results.</returns>
        public static async Task<(ScoreOutput[] scores, CAM[] cams)> Analyze(ChestXRayAIClient aiClient, SKBitmap inputImage, CancellationToken cancellationToken)
        {
            // Pre-process image
            SKBitmap image = await GetPreProcessTask(inputImage, cancellationToken);

            // Analyze the image
            return await aiClient.Analyze(image, cancellationToken);
        }

        /// <summary>
        /// Returns a task to pre-process the image, to prepare it for the models.
        /// </summary>
        /// <param name="image">The image to pre-process.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the task returned by this method.</param>
        /// <returns>The pre-processed image.</returns>
        private static Task<SKBitmap> GetPreProcessTask(SKBitmap image, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                // Scale to ChestXRayAIClient.ImageInputHeight x ChestXRayAIClient.ImageInputWidth
                SKBitmap scaledImage = ImageProcessor.ResizeImagePreservingAspectRatio(
                    image,
                    ChestXRayAIClient.ImageInputWidth,
                    ChestXRayAIClient.ImageInputHeight);

                // Check if the task was cancelled
                cancellationToken.ThrowIfCancellationRequested();

                return scaledImage;
            }, cancellationToken);
        }
    }
}
