// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Android.Content;
using Android.Preferences;

namespace XRayAnalysis.Droid.Service.Preferences
{
    /// <summary>
    /// Class to manage stored preferences for the app.
    /// </summary>
    public sealed class AppPrefManager
    {
        private ISharedPreferences preferences;

        private const string KeyPassedIntroPage = "PassedIntroPage";

        private bool? hasPassedIntroPage;
        /// <summary>
        /// Gets a value indicating whether the intro page has been passed.
        /// </summary>
        public bool HasPassedIntroPage
        {
            get
            {
                if (!hasPassedIntroPage.HasValue)
                {
                    hasPassedIntroPage = preferences.GetBoolean(KeyPassedIntroPage, false);
                }
                return hasPassedIntroPage.Value;
            }

            private set
            {
                ISharedPreferencesEditor preferencesEditor = preferences.Edit();
                preferencesEditor.PutBoolean(KeyPassedIntroPage, value);
                preferencesEditor.Commit();

                hasPassedIntroPage = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the AppPrefManager class.
        /// </summary>
        /// <param name="context">Context.</param>
        public AppPrefManager(Context context)
        {
            preferences = PreferenceManager.GetDefaultSharedPreferences(context);
        }

        /// <summary>
        /// Sets HasPassedIntroPage to true.
        /// </summary>
        public void PassedIntroPage()
        {
            HasPassedIntroPage = true;
        }
      }
}
