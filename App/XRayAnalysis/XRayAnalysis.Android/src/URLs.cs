// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Content;
using XRayAnalysis.Droid.About;
using AndroidUri = Android.Net.Uri;

namespace XRayAnalysis.Droid
{
    public static class URLs
    {
        public static void Launch(Context context, string url, bool launchInBrowser = true, string title = "")
        {
            Intent intent;
            if (launchInBrowser)
            {
                intent = new Intent(Intent.ActionView, AndroidUri.Parse(url));
            }
            else
            {
                intent = WebViewActivity.NewIntent(context, title, url);
            }

            context.StartActivity(intent);
        }
    }
}
