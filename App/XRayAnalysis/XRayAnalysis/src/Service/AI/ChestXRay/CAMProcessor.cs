// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Reflection;

namespace XRayAnalysis.Service.AI.ChestXRay
{
    /// <summary>
    /// Main CAM processing class - forms Class Activation Maps (CAMs) for each of the 14 conditions given 
    /// the raw result from the model
    /// </summary>
    public class CAMProcessor
    {
        public const int Rows = 7;
        public const int Cols = 7;
        public const int Channels = 1024;

        // Path to pre-defined weights
        private const string FinalLayerWeights = "XRayAnalysis.Resources.SampleModel-FinalLayerWeights.csv";

        private static readonly int NumConditions = Enum.GetValues(typeof(ChestCondition)).Length;

        private ModelOutputOrder order;

        // Weights parsed from <see cref="FinalLayerWeights"/>
        // NOTE: conditionWeights uses a 2D jagged array (array of arrays): https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/arrays/jagged-arrays
        // The jagged array allows for accessing whole 1D arrays of information (the weights) per index (the condition)
        // Example Usage:
        //   - conditionWeights[0][499] ~ From the 0th condition (Atelectasis) grab the 499th weight from the weights file. 
        //   - conditionWeights[3]      ~ Grab all 1024 weights from the 3rd condition (Infiltration)
        private float[][] conditionWeights;

        /// <summary>
        /// Initializes a new instance of <see cref="CAMProcessor"/> and loads the weights from the <see cref="FinalLayerWeights"/> file.
        /// </summary>
        public CAMProcessor(ModelOutputOrder order)
        {
            this.order = order;
            this.conditionWeights = LoadWeights();
        }

        /// <summary>
        /// Converts the <see cref="CAM"/> model output to an array of <see cref="CAM"/> objects.
        /// </summary>
        /// <returns>
        /// An array of <see cref="CAM"/> objects "cams".
        /// Each CAM object is a <see cref="Rows"/> by <see cref="Cols"/> float array representing the CAM
        /// for a single condition
        /// </returns>
        /// <param name="camModelOutput">Raw 1 dimensional output from CAM model</param>
        public CAM[] ConvertToCAMArray(float[] camModelOutput)
        {
            float[,,] featureMap = GenerateFeatureMap(camModelOutput);

            CAM[] cams = new CAM[NumConditions];

            // Loop each of the 14 conditions
            for (int i = 0; i < NumConditions; i++)
            {
                cams[i] = GetCAM(conditionWeights[i], featureMap);
            }

            return cams;
        }

        /// <summary>
        /// Converts a 1 dimensional camModelOutput to a 3 dimensional featureMap
        /// </summary>
        /// <returns>The 3 dimensional feature map.</returns>
        /// <param name="camModelOutput">The CAM model's output, which is a 1D float array.</param>
        private float[,,] GenerateFeatureMap(float[] camModelOutput)
        {
            // Determine the dimensions/strides for the model output
            (int dim1, int dim2, int dim3) = GetDimensions();

            int dim1Stride = dim2 * dim3;
            int dim2Stride = dim3;
            int dim3Stride = 1;

            // Internally use Rows x Cols x Channels order
            float[,,] featureMap = new float[Rows, Cols, Channels];

            // Copy the model output data into the 3D featureMap
            int index, row, col, channel;

            for (int d1 = 0; d1 < dim1; d1++)
            {
                for (int d2 = 0; d2 < dim2; d2++)
                {
                    for (int d3 = 0; d3 < dim3; d3++)
                    {
                        // Calculate index
                        index = (d1 * dim1Stride) + (d2 * dim2Stride) + (d3 * dim3Stride);

                        // Determine which dimension is which
                        (row, col, channel) = GetIndexes(d1, d2, d3);

                        // Set feature map
                        featureMap[row, col, channel] = camModelOutput[index];
                    }
                }
            }

            return featureMap;
        }

        private (int dim1, int dim2, int dim3) GetDimensions()
        {
            switch (order)
            {
                case ModelOutputOrder.RCCh:
                    return (Rows, Cols, Channels);

                case ModelOutputOrder.ChRC:
                    return (Channels, Rows, Cols);

                default:
                    // Should never happen
                    throw new ArgumentOutOfRangeException("order", order, "order must be one of the ModelOutputOrder enum values");
            }
        }

        private (int rowIndex, int colIndex, int channelIndex) GetIndexes(int dim1Index, int dim2Index, int dim3Index)
        {
            switch (order)
            {
                case ModelOutputOrder.RCCh:
                    return (dim1Index, dim2Index, dim3Index);

                case ModelOutputOrder.ChRC:
                    return (dim2Index, dim3Index, dim1Index);

                default:
                    // Should never happen
                    throw new ArgumentOutOfRangeException("order", order, "order must be one of the ModelOutputOrder enum values");
            }
        }

        /// <summary>
        /// Returns a <see cref="CAM"/> for a given weight array and featureMap
        /// </summary>
        /// <returns>A 7x7 multidimensional array of floats representing a <see cref="CAM"/> for a given set of weights</returns>
        /// <param name="weights">Set of 1024 weights (in type float) for a given condition</param>
        /// <param name="featureMap">3D float array containing the converted model output</param>
        private CAM GetCAM(float[] weights, float[,,] featureMap)
        {
            float[,] camData = new float[Rows, Cols];

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    for (int channel = 0; channel < Channels; channel++)
                    {
                        // Set feature map
                        camData[row, col] += featureMap[row, col, channel] * weights[channel];
                    }
                }
            }

            return new CAM(camData);
        }


        /// <summary>
        /// Load condition weights from <see cref="FinalLayerWeights"/>
        /// </summary>
        /// <returns>A two dimensional jagged array of 1024 weights for each of the 14 conditions</returns>
        private float[][] LoadWeights()
        {
            float[][] output = new float[NumConditions][];

            // Load embedded resource into string input
            var assembly = typeof(CAMProcessor).GetTypeInfo().Assembly;
            Stream stream = assembly.GetManifestResourceStream(FinalLayerWeights);

            using (var reader = new System.IO.StreamReader(stream))
            {
                string line;
                int i = 0, j;
                while (!string.IsNullOrWhiteSpace(line = reader.ReadLine()))
                {
                    j = 0;

                    // Initialize each row
                    output[i] = new float[Channels];
                    System.Diagnostics.Debug.WriteLine(line);

                    foreach(var col in line.Trim().Split(','))
                    {
                        // Grab weight from column
                        output[i][j] = float.Parse(col);
                        j++;
                    }
                    i++;
                }
            }

            return output;
        }

        /// <summary>
        /// Different possible orderings for the output array of the CAM model
        /// </summary>
        public enum ModelOutputOrder
        {
            /// <summary>
            /// Rows x Cols x Channels
            /// </summary>
            RCCh,

            /// <summary>
            /// Channels x Rows x Cols
            /// </summary>
            ChRC
        }
    }
}
