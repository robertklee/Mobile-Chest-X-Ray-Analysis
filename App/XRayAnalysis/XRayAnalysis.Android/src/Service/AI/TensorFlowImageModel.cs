// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Android.Content.Res;
using XRayAnalysis.Service.AI;
using Org.Tensorflow.Contrib.Android;
using SkiaSharp;

namespace XRayAnalysis.Droid.Service.AI
{
    /// <summary>
    /// Implementation of <see cref="IAIImageModel{T}"/> for Android that uses TensorFlow and outputs a float[].
    /// <para>
    /// This class uses TensorFlowAndroid, a Xamarin.Android C# bindings library project that generates bindings 
    /// to a Java JAR and a native .so library (stored in the .aar archive) for TensorFlow Mobile Android.
    /// </para>
    /// <para>
    /// TensorFlow Mobile website: https://github.com/tensorflow/tensorflow/tree/master/tensorflow/contrib/android
    /// </para>
    /// <para>
    /// Instructions to bind to .aar file: https://developer.xamarin.com/guides/android/advanced_topics/binding-a-java-library/binding-an-aar
    /// </para>
    /// </summary>
    public class TensorFlowImageModel : IAIImageModel<float[][]>
    {
        private const int ImageChannels = 3;

        private TensorFlowInferenceInterface tfInterface;
        private string inputTensorName;
        private string[] outputNodeNames;
        private int[] outputSizes;

        /// <summary>
        /// Initializes a new instance of the <see cref="TensorFlowImageModel"/> class and loads the TensorFlow model.
        /// </summary>
        /// <param name="assets">A reference to an <see cref="AssetManager"/> that will be used to load the model file.</param>
        /// <param name="modelAssetPath">The path to the model file (e.g. "file:///android_asset/model.pb")</param>
        /// <param name="inputTensorName">The name of the input Tensor of the model.</param>
        /// <param name="outputNodeNames">The names of the output nodes of the model.</param>
        /// <param name="outputSizes">The number of values expected in the array at each of the output nodes.</param>
        public TensorFlowImageModel(AssetManager assets, string modelAssetPath, string inputTensorName, string[] outputNodeNames, int[] outputSizes)
        {
            if (outputNodeNames.Length != outputSizes.Length)
            {
                throw new ArgumentException("outputNodeNames (length=" + outputNodeNames.Length + ") and outputSizes (length=" + outputSizes.Length + ") must be of equal length");
            }

            this.tfInterface = new TensorFlowInferenceInterface(assets, modelAssetPath);
            this.inputTensorName = inputTensorName;
            this.outputNodeNames = outputNodeNames;
            this.outputSizes = outputSizes;
        }

        /// <summary>
        /// Implements <see cref="IAIImageModel{T}.Predict(SKBitmap)"/> using a TensorFlow model
        /// </summary>
        public float[][] Predict(SKBitmap imageData)
        {
            float[] input = this.ExtractPixelData(imageData);

            // Supply the input data
            tfInterface.Feed(inputTensorName, input, 1, imageData.Width, imageData.Height, ImageChannels);

            // Run the model
            tfInterface.Run(outputNodeNames);

            // Extract the outputs
            int numOutputs = outputSizes.Length;

            float[][] results = new float[numOutputs][];
            // Iterate over each of the model outputs
            for (int i = 0; i < numOutputs; i++)
            {
                // Initialize an array to contain the data for the current output
                results[i] = new float[outputSizes[i]];
                // Fill the array with the actual data for that output (accessed by node name)
                tfInterface.Fetch(outputNodeNames[i], results[i]);
            }

            return results;
        }

        /// <summary>
        /// Extracts pixel data from a bitmap and formats in an array that can be interpreted as a 3D array,
        /// with dimensions HEIGHT x WIDTH x CHANNELS.
        /// </summary>
        /// 
        /// <code>
        /// 
        /// output = [ row1, row2, row3, ...]
        /// row1 = [ col1, col2, col3, ...] where col_x = [red, blue, green]
        /// ... (for remaining rows)
        /// 
        /// </code>
        /// 
        /// <returns>An float[] containing the properly formatted pixel data.</returns>
        /// <param name="image">The image to process</param>
        private float[] ExtractPixelData(SKBitmap image)
        {
            float[] multiArray = new float[image.Height * image.Width * ImageChannels];

            for (int row = 0; row < image.Height; row++)
            {
                for (int col = 0; col < image.Width; col++)
                {
                    SKColor pixel = image.GetPixel(col, row);
                    int index = (row * image.Width + col) * ImageChannels;
                    multiArray[index] = pixel.Red;
                    multiArray[index + 1] = pixel.Green;
                    multiArray[index + 2] = pixel.Blue;
                }
            }

            return multiArray;
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
                // Dispose unmanaged resources (TensorFlow uses unmanaged native code)
                System.Diagnostics.Debug.WriteLine("Releasing TensorFlow resources");
                tfInterface.Close();

                disposed = true;
            }
        }

        /// <summary>
        /// Finalizer for releasing unmanaged resources.
        /// </summary>
        ~TensorFlowImageModel()
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