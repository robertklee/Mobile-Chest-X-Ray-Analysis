// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace XRayAnalysis.Service.AI.ChestXRay
{
    /// <summary>
    /// A list of the likelihood label values.
    /// </summary>
    public enum LikelihoodLabel
    {
        VeryUnlikely,
        Unlikely,
        Uncertain,
        Likely,
        VeryLikely
    }

    /// <summary>
    /// A helper class to select <see cref="LikelihoodLabel"/>s
    /// </summary>
    public static class LikelihoodLabelHelper
    {
        // Thresholds
        private const float ThresholdForVeryUnlikely    = 0.1f;
        private const float ThresholdForUnlikely        = 0.33f;
        private const float ThresholdForUncertain       = 0.66f;
        private const float ThresholdForLikely          = 0.9f;

        /// <summary>
        /// Returns the correct <see cref="LikelihoodLabel"/> for the given likelihood, based on threshold values.
        /// </summary>
        /// <param name="likelihood">The numerical likelihood, as a decimal (not a percentage).</param>
        /// <returns>The corresponding <see cref="LikelihoodLabel"/>.</returns>
        public static LikelihoodLabel FromLikelihood(float likelihood)
        {
            if (likelihood < ThresholdForVeryUnlikely)
            {
                return LikelihoodLabel.VeryUnlikely;
            }
            else if (likelihood <= ThresholdForUnlikely)
            {
                return LikelihoodLabel.Unlikely;
            }
            else if (likelihood <= ThresholdForUncertain)
            {
                return LikelihoodLabel.Uncertain;
            }
            else if (likelihood <= ThresholdForLikely)
            {
                return LikelihoodLabel.Likely;
            }
            else
            {
                return LikelihoodLabel.VeryLikely;
            }
        }
    }
}
