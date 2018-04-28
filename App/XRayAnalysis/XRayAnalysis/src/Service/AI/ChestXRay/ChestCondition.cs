// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace XRayAnalysis.Service.AI.ChestXRay
{
    /// <summary>
    /// A list of all the chest conditions that can be identified by the AI models.
    /// The order/value of these conditions must match the output order of the AI models.
    /// </summary>
    public enum ChestCondition
    {
        Atelectasis,
        Cardiomegaly,
        Effusion,
        Infiltration,
        Mass,
        Nodule,
        Pneumonia,
        Pneumothorax,
        Consolidation,
        Edema,
        Emphysema,
        Fibrosis,
        PleuralThickening,
        Hernia
    }
}
