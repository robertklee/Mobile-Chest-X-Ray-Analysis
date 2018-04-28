// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using CoreML;
using XRayAnalysis.Service.AI;
using Foundation;
using SkiaSharp;

namespace XRayAnalysis.iOS.Service.AI
{
    /// <summary>
    /// Implementation of <see cref="IAIImageModel{T}"/> for iOS that uses CoreML and outputs a float[].
    /// </summary>
    public class CoreMLImageModel : IAIImageModel<float[]>
    {
        private const int ImageChannels = 3;
        private const string ModelResourceExt = "mlmodelc";

        private MLModel mlModel;
        private string inputFeatureName;
        private string outputFeatureName;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreMLImageModel"/> class and loads the CoreML model.
        /// </summary>
        public CoreMLImageModel(string modelName, string inputFeatureName, string outputFeatureName)
        {
            NSUrl assetPath = NSBundle.MainBundle.GetUrlForResource(modelName, "mlmodelc");
            this.mlModel = MLModel.Create(assetPath, out NSError mlError);
            this.inputFeatureName = inputFeatureName;
            this.outputFeatureName = outputFeatureName;
        }

        /// <summary>
        /// Implements <see cref="IAIImageModel.Predict(SKBitmap)"/> using a CoreML model
        /// </summary>
        public float[] Predict(SKBitmap imageData)
        {
            MLMultiArray input = this.ExtractPixelData(imageData);
            MLMultiArray output = this.Predict(input);

            // Transfer the results from a MLMultiArray to a regular float[]
            float[] outputFloats = new float[output.Count];
            for (int i = 0; i < output.Count; i++)
            {
                outputFloats[i] = (float)output[i];
            }

            return outputFloats;
        }

        /// <summary>
        /// Extracts pixel data from a bitmap and formats in an array that can be interpreted as a 3D array,
        /// with dimensions CHANNELS x HEIGHT x WIDTH.
        /// </summary>
        /// 
        /// <code>
        /// 
        /// output = [ red, blue, green ] 
        /// red = [ row1Red, row2Red, row3Red, ... ]
        /// row1 = [ red of pixel (1,1), red of pixel (1,2), red of pixel (1,3), ...]
        /// ... (for remaining rows)
        /// ... (for blue and green)
        /// 
        /// </code>
        /// 
        /// <param name="image">The image to process.</param>
        /// <returns>An <see cref="MLModel"/> containing the properly formatted pixel data.</returns>
        private MLMultiArray ExtractPixelData(SKBitmap image)
        {
            MLMultiArray multiArray = new MLMultiArray(new NSNumber[] { ImageChannels, image.Height, image.Width}, MLMultiArrayDataType.Float32, out NSError error1);

            for (int h = 0; h < image.Height; h++)
            {
                for (int w = 0; w < image.Width; w++)
                {
                    SKColor pixel = image.GetPixel(w, h);

                    multiArray[(h * image.Width) + w] = pixel.Red;
                    multiArray[(image.Height * image.Width) + (h * image.Width) + w] = pixel.Green;
                    multiArray[(2 * image.Height * image.Width) + (h * image.Width) + w] = pixel.Blue;
                }
            }

            return multiArray;
        }

        /// <summary>
        /// Runs the input through the CoreML model
        /// </summary>
        /// <param name="input">The input data to the model.</param>
        /// <returns>The output of the CoreML model.</returns>
        private MLMultiArray Predict(MLMultiArray input)
        {
            var inputFeatureProvider = new ImageInputFeatureProvider(inputFeatureName)
            {
                ImagePixelData = input
            };

            MLMultiArray result = this.mlModel.GetPrediction(inputFeatureProvider, out NSError modelErr)
                                         .GetFeatureValue(outputFeatureName)
                                         .MultiArrayValue;
            
            return result;
        }

        /// <summary>
        /// Represents the input to the ChestXRay CoreML model
        /// </summary>
        private class ImageInputFeatureProvider : NSObject, IMLFeatureProvider
        {
            private string inputFeatureName;

            internal ImageInputFeatureProvider(string inputFeatureName)
            {
                this.inputFeatureName = inputFeatureName;
            }

            /// <summary>
            /// Gets or sets the image pixel data input to the model.
            /// </summary>
            public MLMultiArray ImagePixelData { get; set; }

            public NSSet<NSString> FeatureNames => new NSSet<NSString>(new NSString(inputFeatureName));

            public MLFeatureValue GetFeatureValue(string featureName)
            {
                if (inputFeatureName.Equals(featureName))
                {
                    return MLFeatureValue.Create(this.ImagePixelData);
                }
                else
                {
                    return MLFeatureValue.Create(0);
                }
            }
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
                    // Dispose of the MLModel (managed resource)
                    mlModel.Dispose();
                }

                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer for releasing unmanaged resources.
        /// </summary>
        ~CoreMLImageModel()
        {
            Dispose(false);
        }

        /// <summary>
        /// Releases managed and unmanaged resources used by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}