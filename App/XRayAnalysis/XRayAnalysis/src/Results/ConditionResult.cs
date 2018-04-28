// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
 
using System;
using XRayAnalysis.Service.AI.ChestXRay;
using SkiaSharp;

namespace XRayAnalysis.Results
{
    /// <summary>
    /// Model class to be used by the results pages for binding to table view cell's / view holders
    /// </summary>
    public class ConditionResult : IComparable<ConditionResult>
    {
        /// <summary>
        /// The condition represented by this result.
        /// </summary>
        public ChestCondition Condition { get; }

        /// <summary>
        /// The score represented by this result.
        /// </summary>
        public ScoreOutput Score { get; }

        /// <summary>
        /// The CAM represented by this result.
        /// </summary>
        public CAM CAM { get; }

        /// <summary>
        /// The color to highlight this result when selected.
        /// </summary>
        public SKColor HighlightColor { get; set; }

        /// <summary>
        /// Stores the "selected" state of the item.
        /// </summary>
        public bool Selected { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="ConditionResult"/> with the provided data
        /// </summary>
        public ConditionResult(ChestCondition condition, ScoreOutput score, CAM cam)
        {
            Condition = condition;
            Score = score;
            CAM = cam;
        }

        /// <summary>
        /// Compares two <see cref="ConditionResult"/>s based on their score likelihood values.
        /// </summary>
        /// <param name="other">The <see cref="ConditionResult"/> to compare to</param>
        /// <returns>A positive number if this > other, a negative number if this < other, or 0 if this == other</returns>
        public int CompareTo(ConditionResult other)
        {
            return this.Score.CompareTo(other.Score);
        }

    }
}
