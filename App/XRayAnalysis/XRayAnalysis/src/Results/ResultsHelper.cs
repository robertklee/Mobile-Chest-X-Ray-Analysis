// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using XRayAnalysis.Service.AI.ChestXRay;
using SkiaSharp;

namespace XRayAnalysis.Results
{
    /// <summary>
    /// Helper class for code that's shared between the ResultsActivity (Android) and ResultsViewController (iOS)
    /// </summary>
    public static class ResultsHelper
    {
        // The minimum required likelihood for a condition to be included in the output of BuildConditionResultArray 
        public static readonly float MinLikelihood = 0.0f;

        // Minimum number of conditions to be displayed in results
        private static readonly int MinConditions = 3;

        /// <summary>
        /// Assembles the input arrays into a single ConditionResult[] that can be used by the results pages
        /// </summary>
        /// <param name="conditions">The <see cref="ChestCondition"/> represented in the other arrays. Must be the same length as scores and cams.</param>
        /// <param name="scores">The results of the score model. Must be the same length as conditions and cams.</param>
        /// <param name="cams">The results of the CAM model. Must be the same length as conditions and scores.</param>
        /// <param name="colors">The colors to associate with each result. If this array is shorter than the other input arrays, the last color entry will be re-used for all the remaining results.</param>
        /// <returns>The ConditionResult array.</returns>
        public static ConditionResult[] BuildConditionResultArray(ChestCondition[] conditions, ScoreOutput[] scores, CAM[] cams, SKColor[] colors)
        {
            if (conditions.Length != scores.Length || scores.Length != cams.Length)
            {
                throw new ArgumentException("conditions.Length must match scores.Length which must match cams.Length");
            }

            int length = conditions.Length;
            List<ConditionResult> results = new List<ConditionResult>();
            for (int i = 0; i < length; i++)
            {
                results.Add(new ConditionResult(conditions[i], scores[i], cams[i]));
            } 

            // Sorts by ConditionResults's natural ordering (by likelihood)
            results.Sort(new ResultsHelper.DescendComparer());


            // Index where all values following in results have a likelihood less than <see="ResultsHelper.MinLikelihood />
            int count;
            for (count = 0; count < results.Count; count++)
            {
                float likelihood = results[count].Score.Likelihood;

                if (count > (MinConditions-1) && likelihood < MinLikelihood)
                {
                    break;
                }
            }

            // Partition results into the top "count" conditions
            results = results.GetRange(0, count);

            // Assign colors after sorting the array
            for (int i = 0; i < results.Count; i++)
            {
                // If there are fewer colors than results, use the last color for all subsequent results
                int colorIndex = i;
                if (i >= colors.Length)
                {
                    colorIndex = colors.Length - 1;
                }
                results[i].HighlightColor = colors[colorIndex];
            }

            return results.ToArray();
        }

        /// <summary>
        /// A comparer to sort a collection of floats in descending order.
        /// </summary>
        private class DescendComparer : IComparer<ConditionResult>
        {
            public int Compare(ConditionResult x, ConditionResult y)
            {
                return y.CompareTo(x);
            }
        }
    }
}
