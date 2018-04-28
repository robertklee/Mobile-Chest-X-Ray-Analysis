// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using XRayAnalysis.Service.AI;
using XRayAnalysis.Service.AI.ChestXRay;
using SkiaSharp;

namespace XRayAnalysis.iOS.Service.AI.ChestXRay
{
    /// <summary>
    /// Implementation of <see cref="ChestXRayAIClient"/> that loads the CoreML models and manages their resources.
    /// </summary>
    public class iOSChestXRayAIClient : ChestXRayAIClient
    {
        /// <summary>
        /// Static class for storing constants related to the iOS ML models.
        /// </summary>
        private static class ModelConstants
        {
            /// <summary>
            /// Constants related to the Scoring model.
            /// </summary>
            public static class ScoreModel
            {
                public const string ResourceName = "ChestXRay-empty";
                public const string InputFeatureName = "input1";
                public const string OutputFeatureName = "output1";
            }

            /// <summary>
            /// Constants related to the CAM model.
            /// </summary>
            public static class CAMModel
            {
                public const string ResourceName = "ChestXRay-CAM-empty";
                public const string InputFeatureName = "input1";
                public const string OutputFeatureName = "output1";
            }
        }

        private IAIImageModel<float[]> scoreModel;
        private IAIImageModel<float[]> camModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="iOSChestXRayAIClient"> class.
        /// This constructor is private since this class is responsible for both instantiating and disposing of the IAIImageModel objects,
        /// so we want to limit the instantiation of this class to within this class itself (to prevent externally created IAIImageModel objects from being passed into the class).
        /// </summary>
        private iOSChestXRayAIClient(IAIImageModel<float[]> scoreModel, IAIImageModel<float[]> camModel, CAMProcessor camProcessor) : base(camProcessor)
        {
            this.scoreModel = scoreModel;
            this.camModel = camModel;
        }

        /// <summary>
        /// Constructs and returns an instance of <see cref="iOSChestXRayAIClient"/> with the CoreML models loaded.
        /// </summary>
        /// <returns>The constructed <see cref="iOSChestXRayAIClient"/> instance.</returns>
        public static iOSChestXRayAIClient NewInstance()
        {
            CoreMLImageModel scoreModel = new CoreMLImageModel(
                ModelConstants.ScoreModel.ResourceName,
                ModelConstants.ScoreModel.InputFeatureName,
                ModelConstants.ScoreModel.OutputFeatureName);
            System.Diagnostics.Debug.WriteLine($"{ModelConstants.ScoreModel.ResourceName} CoreML Model loaded");

            CoreMLImageModel camModel = new CoreMLImageModel(
                ModelConstants.CAMModel.ResourceName,
                ModelConstants.CAMModel.InputFeatureName,
                ModelConstants.CAMModel.OutputFeatureName);
            System.Diagnostics.Debug.WriteLine($"{ModelConstants.CAMModel.ResourceName} CoreML Model loaded");

            CAMProcessor camProcessor = new CAMProcessor(CAMProcessor.ModelOutputOrder.ChRC);

            return new iOSChestXRayAIClient(scoreModel, camModel, camProcessor);
        }
        
        /// <summary>
        /// Implementation of <see cref="ChestXRayAIClient.Analyze(SKBitmap, CancellationToken)"/> using two separate models.
        /// </summary>
        public override async Task<(ScoreOutput[], CAM[])> Analyze(SKBitmap inputImage, CancellationToken cancellationToken)
        {
            ValidateImageDimensions(inputImage);

            Task<ScoreOutput[]> scoreTask = GetScores(inputImage, cancellationToken);
            Task<CAM[]> camTask = GetCAMs(inputImage, cancellationToken);

            await Task.WhenAll(scoreTask, camTask);

            return (scoreTask.Result, camTask.Result);
        }

        private Task<ScoreOutput[]> GetScores(SKBitmap inputImage, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                float[] scoreModelOutput = scoreModel.Predict(inputImage);

                cancellationToken.ThrowIfCancellationRequested();

                ScoreOutput[] scores = GenerateScoreArray(scoreModelOutput);

                cancellationToken.ThrowIfCancellationRequested();

                return scores;
            }, cancellationToken);
        }

        private Task<CAM[]> GetCAMs(SKBitmap inputImage, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                float[] camModelOutput = camModel.Predict(inputImage);
                
                cancellationToken.ThrowIfCancellationRequested();

                CAM[] cams = GenerateCAMArray(camModelOutput);

                cancellationToken.ThrowIfCancellationRequested();

                return cams;
            }, cancellationToken);
        }

        #region IDisposable Support
        private bool disposed = false; // To detect redundant calls

        /// <summary>
        /// Shared method to consolidate resource release.
        /// </summary>
        /// <param name="disposing">Pass <code>True</code> if this is triggered by <see cref="IDisposable.Dispose()"/>, otherwise pass <code>False</code>.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose of the models (managed resources)
                    scoreModel?.Dispose();
                    camModel?.Dispose();
                }

                // No unmanaged resources to free

                disposed = true;
            }
        }

        /// <summary>
        /// Releases managed and unmanaged resources used by this class.
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
