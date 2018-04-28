// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Foundation;

namespace XRayAnalysis.iOS.Service.UserDefaults
{
    // Based on article: https://medium.com/anysuggestion/detecting-the-first-launch-of-the-ios-application-the-wrong-and-the-right-way-78b0605bd8b2
    /// <summary>
    /// Encapsulates the 'FirstLaunch' check so it can be set to always return false for testing.
    /// </summary>
    public sealed class FirstLaunchClient
    {
        // Backing field for WasLaunchedBefore
        private bool? wasLaunchedBefore;

        /// <summary>
        /// Indicates whether the app was launched before.
        /// </summary>
        /// <value><c>true</c> if was launched before; otherwise, <c>false</c>.</value>
        public bool WasLaunchedBefore
        {
            get 
            { 
                if (!wasLaunchedBefore.HasValue)
                {
                    wasLaunchedBefore = getWasLaunchedBefore();
                }
                return wasLaunchedBefore.Value; 
            }
            set 
            { 
                setWasLaunchedBefore(value);
                wasLaunchedBefore = value;
            }
        }

        private Func<bool> getWasLaunchedBefore;
        private Action<bool> setWasLaunchedBefore;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirstLaunchClient"/> class with the provided lambdas.
        /// </summary>
        /// <param name="getWasLaunchedBefore">The lambda containing the implementation of getting the WasLaunchedBefore property.</param>
        /// <param name="setWasLaunchedBefore">The lambda containing the implementation of setting the WasLaunchedBefore property.</param>
        public FirstLaunchClient(Func<bool> getWasLaunchedBefore, Action<bool> setWasLaunchedBefore)
        {
            this.getWasLaunchedBefore = getWasLaunchedBefore;
            this.setWasLaunchedBefore = setWasLaunchedBefore;

            wasLaunchedBefore = getWasLaunchedBefore();
            if (wasLaunchedBefore.Value)
            {
                setWasLaunchedBefore(true);
            }
        }

        /// <summary>
        /// Default constructor for a new instance of the <see cref="FirstLaunchClient"/> class. Initializes the <see cref="getWasLaunchedBefore"/> lambda
        /// to retrieve the value from NSUserDefaults. Initializes the <see cref="setWasLaunchedBefore>"/> to set the value in NSUserDefaults.
        /// </summary>
        public FirstLaunchClient() : this(() =>
            {
                return NSUserDefaults.StandardUserDefaults.BoolForKey(Constants.FirstLaunchKey);
            }, (value) =>
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, Constants.FirstLaunchKey);
            }
        )
        { }
    }
}