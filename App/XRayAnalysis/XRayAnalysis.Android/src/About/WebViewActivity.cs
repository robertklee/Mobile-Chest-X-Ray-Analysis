// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Webkit;

namespace XRayAnalysis.Droid.About
{
    [Activity(Label = "", ScreenOrientation = ScreenOrientation.Portrait)]
    public class WebViewActivity : AppCompatActivity
    {
        private const string ExtraTitle = "EXTRA_TITLE";
        private const string ExtraUrl = "EXTRA_URL";

        public static Intent NewIntent(Context context, string title, string url)
        {
            Intent intent = new Intent(context, typeof(WebViewActivity));
            intent.PutExtra(ExtraTitle, title);
            intent.PutExtra(ExtraUrl, url);
            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_webview);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            this.Title = this.Intent.GetStringExtra(ExtraTitle);

            WebView webview = FindViewById<WebView>(Resource.Id.webview);
            webview.LoadUrl(this.Intent.GetStringExtra(ExtraUrl));
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}
