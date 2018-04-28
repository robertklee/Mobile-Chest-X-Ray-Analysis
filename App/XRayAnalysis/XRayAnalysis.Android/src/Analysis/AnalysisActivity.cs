// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.App;
using XRayAnalysis.Droid.Results;
using XRayAnalysis.Droid.Service.Image;
using XRayAnalysis.Service.AI.ChestXRay;
using SkiaSharp;
using SkiaSharp.Views.Android;

//Alias directives
using AndroidUri = Android.Net.Uri;

namespace XRayAnalysis.Droid.Analysis
{
    /// <summary>
    /// Activity class to back the Analysis page.
    /// Running the AI model occurs on this page.
    /// If this Activity is moved the background, analysis
    ///  continues; however, the Activity won't finish until it has returned to the foreground.
    /// </summary>
    [Activity(Label = "@string/analyzing_msg", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AppTheme.Analysis")]
    public class AnalysisActivity : AppCompatActivity
    {
        /// <summary>
        /// Key for passing the cropped image URI through an intent to this Activity
        /// </summary>
        private static readonly string ExtraCroppedImageUri = "EXTRA_CROPPED_IMAGE_URI";

        /// <summary>
        /// Key for passing the original image URI through an intent to this Activity
        /// </summary>
        private static readonly string ExtraOriginalImageUri = "EXTRA_ORIGINAL_IMAGE_URI";

        private bool activityResumed = false;
        private AndroidUri croppedImageUri;
        private AndroidUri originalImageUri;
        private CancellationTokenSource cancellationTokenSource;

        private bool finishAnalysisOnResume = false;
        private ScoreOutput[] scores;
        private CAM[] cams;

        /// <summary>
        /// Generates an Intent to launch AnalysisActivity passing along the provided parameters.
        /// </summary>
        /// <param name="context">A context for the Intent.</param>
        /// <param name="croppedImageUri">URI of the cropped image.</param>
        /// <param name="originalImageUri">URI of the original image.</param>
        /// <returns>The generated Intent.</returns>
        public static Intent NewIntent(Context context, AndroidUri croppedImageUri, AndroidUri originalImageUri)
        {
            Intent intent = new Intent(context, typeof(AnalysisActivity));
            intent.PutExtra(ExtraCroppedImageUri, croppedImageUri);
            intent.PutExtra(ExtraOriginalImageUri, originalImageUri);
            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_analysis);

            // Extract the image URI from the intent parameters
            croppedImageUri = (AndroidUri)Intent.GetParcelableExtra(ExtraCroppedImageUri);

            //Extract the image filename from the intent parameters
            originalImageUri = (AndroidUri)Intent.GetParcelableExtra(ExtraOriginalImageUri);
        }

        protected override async void OnResume()
        {
            base.OnResume();

            // Set a flag so we know the activity is in the foreground
            activityResumed = true;

            // Makes sure that this only runs the first time OnResume is called
            if (cancellationTokenSource == null)
            {
                cancellationTokenSource = new CancellationTokenSource();

                // Retrieve the shared AI Client that was loaded by the MainApplication class
                ChestXRayAIClient aiClient = ((MainApplication)this.Application).AIClient;
                System.Diagnostics.Debug.WriteLine("AI Client retrieved from MainApplication class");

                try
                {
                    // Load image
                    SKBitmap image = await LoadImage(cancellationTokenSource.Token);
                    System.Diagnostics.Debug.WriteLine("Image loaded");

                    // Run analysis and save results
                    (scores, cams) = await ChestXRayAIRunner.Analyze(aiClient, image, cancellationTokenSource.Token);
                    System.Diagnostics.Debug.WriteLine("Analysis completed");

                    // Release the SKBitmap
                    image.Dispose();

                    // If the activity is currently in the foreground immediately finish the analysis
                    if (activityResumed)
                    {
                        FinishAnalysis();
                    }
                    else
                    {
                        // Otherwise set a flag to finish the analysis when the activity does become the foreground activity
                        finishAnalysisOnResume = true;
                    }
                }
                catch (System.OperationCanceledException) // NOT Android.OS.OperationCanceledException
                {
                    System.Diagnostics.Debug.WriteLine("Analysis cancelled");
                }
            }
            else if(finishAnalysisOnResume)
            {
                // Analysis completed while we were in the background, so finish the analysis now that we're in the foreground
                FinishAnalysis();
            }
        }

        private void FinishAnalysis()
        {
            Intent data = ResultsActivity.NewIntent(ApplicationContext, croppedImageUri, originalImageUri, scores, cams);
            SetResult(Result.Ok, data);
            Finish();
        }

        private Task<SKBitmap> LoadImage(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                // Load the image as a Bitmap
                Bitmap bitmap = ImageHelper.GetBitmapFromUri(this.ContentResolver, croppedImageUri);
                SKBitmap skBitmap = bitmap.ToSKBitmap();

                // Release the Android bitmap
                bitmap.Recycle();

                // Check if the task was cancelled
                cancellationToken.ThrowIfCancellationRequested();

                return skBitmap;
            }, cancellationToken);
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Set a flag so we know the activity is no longer in the foreground
            activityResumed = false;
        }

        public override void OnBackPressed()
        {
            base.OnBackPressed();

            // Cancel the task
            cancellationTokenSource.Cancel();
        }
    }
}
