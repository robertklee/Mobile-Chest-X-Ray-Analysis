// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;

namespace XRayAnalysis.Service.AI.ChestXRay
{
    /// <summary>
    /// Represents the output for the ChestXRay CAM (Class Activation Map).
    /// </summary>
    public class CAM
    {
        /// <summary>
        /// The 2D float array which contains the CAM (Class Activation Map).
        /// </summary>
        /// <value>The cam.</value>
        private float[,] camData;

        /// <summary>
        /// Gets <see cref="camData"/>[dim1, dim2]. Provides read-only access to the 2d float array.
        /// </summary>
        /// 
        /// <returns>The float at <see cref="camData"/>[dim1, dim2]</returns>
        /// <param name="dim1">The first index.</param>
        /// <param name="dim2">The second index.</param>
        public float this[int dim1, int dim2]
        {
            get { return camData[dim1, dim2]; }
        }

        /// <summary>
        /// The number of rows in the CAM
        /// </summary>
        public int Rows
        {
            get { return camData.GetLength(0); }
        }

        /// <summary>
        /// The number of columns in the CAM
        /// </summary>
        public int Cols
        {
            get { return camData.GetLength(1); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CAM"/> class.
        /// </summary>
        /// <param name="cam">A 2D float array which contains the CAM (Class Activation Map).</param>
        public CAM(float[,] camData)
        {
            this.camData = camData;
        }
        
        /// <summary>
        /// Normalizes this <see cref="CAM"/> object to (0,1)
        /// </summary>
        /// <returns>A new <see cref="CAM"/> object containing the normalized values.</returns>
        public CAM Normalize()
        {
            float min = Single.PositiveInfinity;
            float max = Single.NegativeInfinity;

            float[,] normalizedCAM = new float[Rows, Cols];

            // Find the max and min in the 2D float array
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    var element = this[row, col];

                    min = Math.Min(min, element);
                    max = Math.Max(max, element);
                }
            }

            float range = max - min;

            // Normalize each element of the CAM
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    normalizedCAM[row, col] = (this[row, col] - min) / range;
                }
            }

            return new CAM(normalizedCAM);
        }

        /// <summary>
        /// Formats the CAM 2D array into a human readable string.
        /// </summary>
        /// <returns>The string representation of the object.</returns>
        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            for (int row = 0; row < camData.GetLength(0); row++ )
            {
                for (int col = 0; col < camData.GetLength(1); col++) 
                {
                    // Print float to be have 7 decimal places, padded by 0's
                    output.Append(String.Format("{0:#0.0000000}", camData[row, col]) + " ");
                }
                output.Append(Environment.NewLine);
            }

            return output.ToString();
        }
    }
}
