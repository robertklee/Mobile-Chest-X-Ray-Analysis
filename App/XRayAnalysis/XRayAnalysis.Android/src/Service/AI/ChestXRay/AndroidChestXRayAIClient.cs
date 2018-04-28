// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content.Res;
using XRayAnalysis.Service.AI;
using XRayAnalysis.Service.AI.ChestXRay;
using SkiaSharp;

namespace XRayAnalysis.Droid.Service.AI.ChestXRay
{
    /// <summary>
    /// Implementation of <see cref="ChestXRayAIClient"/> that loads the Android TensorFlow model and manages their resources.
    /// 
    /// This implementation uses a single TensorFlow model and extracts outputs from two layers:
    ///   - Scores (from the last layer)
    ///   - CAM data (from the final DenseNet layer)
    /// </summary>
    public class AndroidChestXRayAIClient : ChestXRayAIClient
    {
        /// <summary>
        /// Static class for storing constants related to the Android ML models.
        /// </summary>
        private static class ModelConstants
        {
            public const string AssetPath = "file:///android_asset/SampleModel.pb";
            public const string InputTensorName = "input_1";
            public const string OutputNodeName = "output_0";
            public const string ActivationMapNodeName = "DenseNet/activation_121/Relu";
        }

        private IAIImageModel<float[][]> model;

        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidChestXRayAIClient"> class.
        /// This constructor is private since this class is responsible for both instantiating and disposing of the IAIImageModel objects,
        /// so we want to limit the instantiation of this class to within this class itself (to prevent externally created IAIImageModel objects from being passed into the class).
        /// </summary>
        /// <param name="model">An AI model that can produce outputs for both scores and CAMs (index 0 of the model output should be scores, index 1 should be CAM data)</param>
        /// <param name="camProcessor">An instance of <see cref="CAMProcessor"/> that can process the CAM output from the provided model.</param>
        private AndroidChestXRayAIClient(IAIImageModel<float[][]> model, CAMProcessor camProcessor) : base(camProcessor)
        {
            this.model = model;
        }

        /// <summary>
        /// Constructs and returns an instance of <see cref="AndroidChestXRayAIClient"/>
        /// with the models loaded using the provided <see cref="AssetManager"/>.
        /// </summary>
        /// <param name="assets">An <see cref="AssetManager"/> that can be used to load the model files.</param>
        /// <returns>The constructed <see cref="AndroidChestXRayAIClient"/> instance.</returns>
        public static AndroidChestXRayAIClient NewInstance(AssetManager assets)
        {
            TensorFlowImageModel model = new TensorFlowImageModel(
                assets,
                ModelConstants.AssetPath,
                ModelConstants.InputTensorName,
                new string[] { ModelConstants.OutputNodeName, ModelConstants.ActivationMapNodeName },
                new int[] { Enum.GetValues(typeof(ChestCondition)).Length, CAMProcessor.Rows * CAMProcessor.Cols * CAMProcessor.Channels });

            System.Diagnostics.Debug.WriteLine("ChestXRay TensorFlow ML Model loaded");

            CAMProcessor camProcessor = new CAMProcessor(CAMProcessor.ModelOutputOrder.RCCh);

            return new AndroidChestXRayAIClient(model, camProcessor);
        }

        /// <summary>
        /// Implementation of <see cref="ChestXRayAIClient.Analyze(SKBitmap, CancellationToken)"/> using a single model.
        /// </summary>
        public override Task<(ScoreOutput[], CAM[])> Analyze(SKBitmap inputImage, CancellationToken cancellationToken)
        {
            ValidateImageDimensions(inputImage);

            return Task.Factory.StartNew(() =>
            {
                // We have to check for cancellation before each of the following steps

                // Pass the image to the model and run an inference on it
                cancellationToken.ThrowIfCancellationRequested();
                float[][] modelOutputs = model.Predict(inputImage);

                // Extract the score data and package it up into a convenient object array (ScoreOutput[])
                cancellationToken.ThrowIfCancellationRequested();
                float[] scoreData = modelOutputs[0];
                ScoreOutput[] scores = GenerateScoreArray(scoreData);

                // Extract the CAM data and post-process it to generate the CAM objects
                cancellationToken.ThrowIfCancellationRequested();
                float[] camData = modelOutputs[1];
                CAM[] cams = GenerateCAMArray(camData);

                // Final cancellation check
                cancellationToken.ThrowIfCancellationRequested();

                return (scores, cams);
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
                    model?.Dispose();
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
