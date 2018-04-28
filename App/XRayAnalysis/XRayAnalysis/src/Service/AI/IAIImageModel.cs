// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using SkiaSharp;

namespace XRayAnalysis.Service.AI
{
    /// <summary>
    /// Represents an AI model to analyze an image.
    /// </summary>
    /// <typeparam name="T">The output type of the model.</typeparam>
    public interface IAIImageModel<T> : IDisposable
    {
        /// <summary>
        /// Analyses the provided image using the ML model.
        /// </summary>
        /// <param name="image">The image to analyze.</param>
        /// <returns>The output of the model</returns>
        T Predict(SKBitmap image);
    }
}
