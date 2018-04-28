// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Constraints;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Support.Transitions;
using Android.Views;
using Android.Widget;
using XRayAnalysis.Droid.Service.AI.ChestXRay;
using XRayAnalysis.Droid.Service.Image;
using XRayAnalysis.Results;
using XRayAnalysis.Service;
using XRayAnalysis.Service.AI.ChestXRay;
using XRayAnalysis.Service.Analytics;
using SkiaSharp;
using SkiaSharp.Views.Android;

//Alias directives
using AndroidUri = Android.Net.Uri;
using SupportWidget = Android.Support.V7.Widget;

namespace XRayAnalysis.Droid.Results
{
    /// <summary>
    /// Activity class for the Results page where the user is presented with the results to the image they selected to be analyzed and a display of that image they can interact with.
    /// </summary>
    [Activity(Label = "@string/results_title", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ResultsActivity : AppCompatActivity, Transition.ITransitionListener
    {
        /// <summary>
        /// The width and height at which to load the x-ray image
        /// 
        /// NOTE: The image must be loaded at a fixed resolution to
        /// ensure a high CAM generation and overlay speed
        /// </summary>
        public const int LoadImageWidth = 1024;
        public const int LoadImageHeight = 1024;

        /// <summary>
        /// Key for passing the cropped image URI through an intent to this Activity
        /// </summary>
        private static readonly string ExtraCroppedImageUri = "EXTRA_CROPPED_IMAGE_URI";

        /// <summary>
        /// Key for passing the original image URI through an intent to this Activity
        /// </summary>
        private static readonly string ExtraOriginalImageUri = "EXTRA_ORIGINAL_IMAGE_URI";

        /// <summary>
        /// Key for passing model output scores through an intent to this Activity
        /// </summary>
        private static readonly string ExtraScores = "EXTRA_SCORES";

        /// <summary>
        /// Key for passing CAMs through an intent to this Activity
        /// </summary>
        private static readonly string ExtraCAMs = "EXTRA_CAMS";

        private CancellationTokenSource cancellationTokenSource;
        
        private string[] conditionNames;
        private string[] likelihoodLabels;
        private SKColor[] conditionColors;

        private ChestXRayAIClient aiClient;

        private Bitmap originalXRay;
        private Bitmap blendedImage;

        private AndroidUri croppedImageUri;
        private AndroidUri originalImageUri;

        private ConditionResult[] results;

        // Views
        private XRayDisplayView xRayDisplayView;
        private XRayDisplayView xRayDisplayViewOriginal;

        private RecyclerView conditionListPrimary;
        private ResultsAdapter adapterPrimary;
        private ScrollControlledLinearLayoutManager layoutManagerPrimary;

        private RecyclerView conditionListSecondary;
        private ResultsAdapter adapterSecondary;
        private ScrollControlledLinearLayoutManager layoutManagerSecondary;

        // See More
        private bool seeMoreModeEnabled;
        private ConstraintLayout root;
        private ConstraintSet resultsConstraints;
        private ConstraintSet seeMoreConstraints;
        private Button seeMoreHideButton;

        // Flags
        private bool backPressed = false;

        /// <summary>
        /// Generates an Intent to launch ResultsActivity passing along the provided parameters.
        /// </summary>
        /// <param name="context">A context for the Intent.</param>
        /// <param name="croppedImageUri">URI of the cropped image.</param>
        /// <param name="originalImageUri">URI of the original image.</param>
        /// <param name="scores">Array of <see cref="ScoreOutput"/>s from the analysis.</param>
        /// <param name="cams">Array of <see cref="CAM"/>s from the analysis.</param>
        /// <returns>The generated Intent.</returns>
        public static Intent NewIntent(Context context, AndroidUri croppedImageUri, AndroidUri originalImageUri, ScoreOutput[] scores, CAM[] cams)
        {
            Intent intent = new Intent(context, typeof(ResultsActivity));

            // Parcel the ScoreOutputs for sending through an Intent
            ParcelableScoreOutput[] parcelableScores = new ParcelableScoreOutput[scores.Length];
            for (int i = 0; i < scores.Length; i++)
            {
                parcelableScores[i] = new ParcelableScoreOutput(scores[i]);
            }

            // Parcel the CAMs for sending through an Intent
            ParcelableCAM[] parcelableCAMs = new ParcelableCAM[cams.Length];
            for (int i = 0; i < cams.Length; i++)
            {
                parcelableCAMs[i] = new ParcelableCAM(cams[i]);
            }

            intent.PutExtra(ExtraCroppedImageUri, croppedImageUri);
            intent.PutExtra(ExtraOriginalImageUri, originalImageUri);
            intent.PutExtra(ExtraScores, parcelableScores);
            intent.PutExtra(ExtraCAMs, parcelableCAMs);

            return intent;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AnalyticsService.TrackEvent(AnalyticsService.Event.ResultsPageViewed);

            // Load XML layout
            SetContentView(Resource.Layout.activity_results);

            // Setup toolbar
            SupportWidget.Toolbar toolbar = FindViewById<SupportWidget.Toolbar>(Resource.Id.toolbar);
            toolbar.Title = ""; // We don't want to show the title in the toolbar, but we need it as the Activity label for accessibility
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            // Load strings and colors
            conditionNames = Resources.GetStringArray(Resource.Array.ai_chest_conditions);
            likelihoodLabels = Resources.GetStringArray(Resource.Array.ai_likelihood_labels);
            conditionColors = new SKColor[]
            {
                LoadColor(Resource.Color.purple),
                LoadColor(Resource.Color.pink),
                LoadColor(Resource.Color.blue),
                LoadColor(Resource.Color.turquoise)
            };

            // Obtain UI references
            xRayDisplayView = FindViewById<XRayDisplayView>(Resource.Id.img_xray);
            xRayDisplayViewOriginal = FindViewById<XRayDisplayView>(Resource.Id.img_xray_original);
            conditionListPrimary = FindViewById<RecyclerView>(Resource.Id.condition_list_primary);
            conditionListSecondary = FindViewById<RecyclerView>(Resource.Id.condition_list_secondary);
            seeMoreHideButton = FindViewById<Button>(Resource.Id.button_see_more_hide);

            // Set up for constraint transitions to/from see more
            seeMoreModeEnabled = false;
            root = FindViewById<ConstraintLayout>(Resource.Id.root);
            resultsConstraints = new ConstraintSet();
            resultsConstraints.Clone(this, Resource.Layout.activity_results);
            seeMoreConstraints = new ConstraintSet();
            seeMoreConstraints.Clone(this, Resource.Layout.activity_see_more);

            // See more and hide click listener
            seeMoreHideButton.Click += delegate
            {
                PageTransition();
            };

            // Load analysis results
            ExtractResultsFromIntent();

            // Load and display the original X-Ray
            croppedImageUri = (AndroidUri)this.Intent.GetParcelableExtra(ExtraCroppedImageUri);
            originalImageUri = (AndroidUri)this.Intent.GetParcelableExtra(ExtraOriginalImageUri);

            originalXRay = ImageHelper.GetBitmapFromUri(
                this.ContentResolver,
                croppedImageUri,
                LoadImageWidth,
                LoadImageHeight
            );
            xRayDisplayView.SetXRayImageResetZoom(originalXRay);
            xRayDisplayViewOriginal.SetXRayImageResetZoom(originalXRay);

            // Load and display filename
            string filename = ImageHelper.ExtractFilename(originalImageUri, this.ContentResolver);
            xRayDisplayView.Filename = filename;
            xRayDisplayViewOriginal.Filename = filename;

            // Load and display the image date
            string date = ImageHelper.ExtractDate(originalImageUri, this.ContentResolver, this);
            xRayDisplayView.Date = date;
            xRayDisplayViewOriginal.Date = date;

            // Retrieve the shared AI Client that was loaded by the MainApplication class
            aiClient = ((MainApplication)this.Application).AIClient;

            // Fill in data and attach listeners
            SetupConditionResultViews();
        }

        public override void OnBackPressed()
        {
            SetResult(Result.Canceled);
            backPressed = true;
            base.OnBackPressed();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.menu_results, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    return true;
                case Resource.Id.menu_item_new_session:
                    SetResult(Result.Ok);
                    Finish();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!backPressed)
            {
                // If back wasn't pressed, make sure we set the result to OK so that
                // the calling activity doesn't think it was a back navigation
                SetResult(Result.Ok);
            }
        }

        private SKColor LoadColor(int colorResId)
        {
            return new SKColor((uint)ContextCompat.GetColor(this, colorResId));
        }

        private void ExtractResultsFromIntent()
        {
            // Load and unparcel scores
            IParcelable[] parcelableScores = this.Intent.GetParcelableArrayExtra(ExtraScores);
            ScoreOutput[] scores = new ScoreOutput[parcelableScores.Length];
            for (int i = 0; i < parcelableScores.Length; i++)
            {
                scores[i] = ((ParcelableScoreOutput)parcelableScores[i]).Value;
            }

            // Load and unparcel CAMs
            IParcelable[] parcelableCAMs = this.Intent.GetParcelableArrayExtra(ExtraCAMs);
            CAM[] cams = new CAM[parcelableCAMs.Length];
            for (int i = 0; i < parcelableCAMs.Length; i++)
            {
                cams[i] = ((ParcelableCAM)parcelableCAMs[i]).Value;
            }

            // Collapse into results array
            results = ResultsHelper.BuildConditionResultArray((ChestCondition[])Enum.GetValues(typeof(ChestCondition)), scores, cams, conditionColors);
        }

        private void SetupConditionResultViews()
        {
            /* CONDITION LIST PRIMARY */

            // Setup layout manager
            layoutManagerPrimary = new ScrollControlledLinearLayoutManager(this);
            layoutManagerPrimary.ScrollEnabled = false;
            conditionListPrimary.SetLayoutManager(layoutManagerPrimary);
            conditionListPrimary.HasFixedSize = true;

            // Setup divider
            DividerItemDecoration dividerPrimary = new DividerItemDecoration(conditionListPrimary.Context, layoutManagerPrimary.Orientation);
            conditionListPrimary.AddItemDecoration(dividerPrimary);

            // Setup adapter
            adapterPrimary = new ResultsAdapter(results, conditionNames, likelihoodLabels, ConditionClicked);
            conditionListPrimary.SetAdapter(adapterPrimary);


            /* CONDITION LIST SECONDARY */

            // Setup layout manager
            layoutManagerSecondary = new ScrollControlledLinearLayoutManager(this);
            layoutManagerSecondary.ScrollEnabled = true;
            conditionListSecondary.SetLayoutManager(layoutManagerSecondary);

            // Setup divider
            DividerItemDecoration dividerSecondary = new DividerItemDecoration(conditionListSecondary.Context, layoutManagerSecondary.Orientation);
            conditionListSecondary.AddItemDecoration(dividerSecondary);

            // Setup adapter
            ConditionResult[] resultsSecondary = results.Take(3).ToArray();
            adapterSecondary = new ResultsAdapter(resultsSecondary, conditionNames, likelihoodLabels, ConditionClicked);
            conditionListSecondary.SetAdapter(adapterSecondary);
            conditionListSecondary.HasFixedSize = true;

            // Set initial position
            conditionListSecondary.ScrollToPosition(1);
            layoutManagerSecondary.ScrollEnabled = false;
        }

        /// <summary>
        /// Loads the new constraint layout and adds the animation operation.
        /// </summary>
        private void PageTransition()
        {
            // Disable the See More / Hide button so we can't trigger another animation while this one is happening
            // It will get re-enabled in OnTransitionEnd
            seeMoreHideButton.Enabled = false;

            seeMoreModeEnabled = !seeMoreModeEnabled;

            if (seeMoreModeEnabled)
            {
                // Current: Results
                // Transition To: See More

                // The original x-ray view (that only appears in See More) is hidden to begin with so that
                // TalkBack doesn't narrate its elements
                xRayDisplayViewOriginal.Visibility = ViewStates.Visible;

                Transition transition = new AutoTransition();
                transition.AddListener(this); // This listener will hide the conditionListSecondary when the transition is done (and re-enable the See More / Hide button)

                TransitionManager.BeginDelayedTransition(root, transition);

                seeMoreConstraints.ApplyTo(root);
                seeMoreHideButton.SetText(Resource.String.see_all_btn_hide);
                layoutManagerPrimary.ScrollEnabled = true;

                xRayDisplayView.TextDisplayMode = XRayDisplayView.DisplayMode.Small;
                xRayDisplayView.FilenameLabel = GetString(Resource.String.see_all_image_label_analyzed);
            }
            else
            {
                // Current: See More
                // Transition To: Results

                // We need a little bit of a custom transition here:
                // We want to fade in the conditionListSecondary and move it at the same time
                // We also want to keep the origin x-ray view visible during the transition then hide it afterwards

                // This transition takes care of the fade in + move for all elements
                TransitionSet mainTransitions = new TransitionSet();
                mainTransitions.SetOrdering(TransitionSet.OrderingTogether);
                mainTransitions.AddTransition(new ChangeBounds());
                mainTransitions.AddTransition(new Fade(Fade.In));

                // This transition takes care of fading out the original x-ray view
                Transition fadeOut = new Fade(Fade.Out);
                fadeOut.AddTarget(Resource.Id.img_xray_original);

                // We can then run the two transition sequentially to make sure the fade out happens after everything else
                TransitionSet transitions = new TransitionSet();
                transitions.SetOrdering(TransitionSet.OrderingSequential);
                transitions.AddTransition(mainTransitions);
                transitions.AddTransition(fadeOut);
                transitions.AddListener(this); // This listener will re-enable the See More / Hide button when the transition is done

                TransitionManager.BeginDelayedTransition(root, transitions);

                resultsConstraints.ApplyTo(root);
                seeMoreHideButton.SetText(Resource.String.results_btn_see_more);

                // Need to reset the scroll of conditionListPrimary to the top
                layoutManagerPrimary.ScrollToPositionWithOffset(0, 0);
                layoutManagerPrimary.ScrollEnabled = false;

                xRayDisplayView.TextDisplayMode = XRayDisplayView.DisplayMode.Large;
                xRayDisplayView.FilenameLabel = GetString(Resource.String.results_image_label);
            }
        }

        private async void ConditionClicked(object sender, int position)
        {
            ConditionResult item = results[position];

            // Save the current state of the selected view
            bool currentlySelected = item.Selected;

            // De-select everything
            for (int i = 0; i < results.Length; i++)
            {
                results[i].Selected = false;
            }

            // Toggle the state of the selected view
            item.Selected = !currentlySelected;
            adapterPrimary.NotifyDataSetChanged();
            adapterSecondary.NotifyDataSetChanged();

            try
            {
                // Update the image
                await SetOverlayVisibility(position, item.Selected);
            }
            catch (System.OperationCanceledException)
            {
                // Unhighlight the button
                item.Selected = false;
                adapterPrimary.NotifyDataSetChanged();
                adapterSecondary.NotifyDataSetChanged();
            }
        }

        private async Task SetOverlayVisibility(int index, bool visible)
        {
            // Release previous blended bitmap
            xRayDisplayView.XRayImage = originalXRay;
            blendedImage?.Recycle();
            blendedImage = null;

            if (visible)
            {
                try
                {
                    blendedImage = await GenerateBlendedCAMImage(results[index].CAM, results[index].HighlightColor);
                    xRayDisplayView.XRayImage = blendedImage;
                }
                catch (System.OperationCanceledException)
                {
                    xRayDisplayView.XRayImage = originalXRay;
                    System.Diagnostics.Debug.WriteLine("CAM overlay task cancelled");
                    // Pass the exception to the caller to unhighlight the button
                    throw;
                }
            }
            else
            {
                xRayDisplayView.XRayImage = originalXRay;
            }

            // Cleanup garbage-collectable resources ASAP
            GC.Collect();
        }

        private async Task<Bitmap> GenerateBlendedCAMImage(CAM cam, SKColor color)
        {
            // Cancel the previous blending task if there is one
            cancellationTokenSource?.Cancel();

            cancellationTokenSource = new CancellationTokenSource();

            // Dispose of previous blended image
            blendedImage?.Dispose();

            SKBitmap colorMap = await ImageProcessorRunner.GetCAMColorMapTask(
                cam,
                color,
                originalXRay.Width,
                originalXRay.Height,
                cancellationTokenSource.Token
            );

            SKBitmap input = originalXRay.ToSKBitmap();
            SKBitmap blendedSKBitmap = await ImageProcessorRunner.GetOverlayImageTask(
                input,
                colorMap,
                cancellationTokenSource.Token
            );
            // Dispose of temporary SKBitmap
            input.Dispose();

            Bitmap blendedBitmap = blendedSKBitmap.ToBitmap();

            // Cleanup the intermediate bitmaps
            colorMap?.Dispose();
            blendedSKBitmap?.Dispose();

            return blendedBitmap;
        }

        #region Transition.ITransitionListener support

        public void OnTransitionStart(Transition transition)
        {
            // Do nothing
        }

        public void OnTransitionResume(Transition transition)
        {
            // Do nothing
        }

        public void OnTransitionPause(Transition transition)
        {
            // Do nothing
        }

        public void OnTransitionEnd(Transition transition)
        {
            if (seeMoreModeEnabled)
            {
                conditionListSecondary.Visibility = ViewStates.Gone;
            }

            seeMoreHideButton.Enabled = true;
        }

        public void OnTransitionCancel(Transition transition)
        {
            // Do nothing
        }

        #endregion
    }

    /// <summary>
    /// Custom extension to LinearLayoutManager that allows us to control whether scrolling is enabled
    /// </summary>
    public class ScrollControlledLinearLayoutManager: LinearLayoutManager
    {
        public bool ScrollEnabled { get; set; }

        public ScrollControlledLinearLayoutManager(Context context) : base(context)
        {
            ScrollEnabled = true;
        }

        public override bool CanScrollVertically()
        {
            return ScrollEnabled && base.CanScrollVertically();
        }
    }
}