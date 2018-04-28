// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;

namespace XRayAnalysis.Service.AI.ChestXRay
{
    /// <summary>
    /// Represents the output of the ChestXRay score model (for determining condition likelihoods).
    /// </summary>
    public class ScoreOutput: IComparable<ScoreOutput>
    {

        /// <summary>
        /// The likelihood of the condition as a decimal value (multiply by 100 for a percentage).
        /// </summary>
        public float Likelihood { get; }

        /// <summary>
        /// Initializes a new instance of the class <see cref="ScoreOutput"/>.
        /// </summary>
        /// <param name="likelihood">The likelihood of the condition as a decimal value.</param>
        public ScoreOutput(float likelihood)
        {
            this.Likelihood = likelihood;
        }

        /// <summary>
        /// Compares two <see cref="ScoreOutput"/>s based on their likelihood values.
        /// </summary>
        /// <param name="other">The <see cref="ScoreOutput"/> to compare to</param>
        /// <returns></returns>
        public int CompareTo(ScoreOutput other)
        {
            return this.Likelihood.CompareTo(other.Likelihood);
        }

        /// <summary>
        /// Formats the likelihood into a human readable string
        /// </summary>
        /// <returns>The string representation of the object.</returns>
        public override string ToString()
        {
            // Returns Likelihood float
            return Likelihood.ToString();
        }
    }
}
