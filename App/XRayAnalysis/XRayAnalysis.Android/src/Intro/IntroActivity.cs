// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using XRayAnalysis.Droid.About;

using SupportWidget = Android.Support.V7.Widget;

namespace XRayAnalysis.Droid.Intro
{
    /// <summary>
    /// Activity class for the intro page where the user can view the ChestXRay AI model or press the 'Get Started' button to go to the Image Input page.
    /// </summary>
    [Activity(Label = "@string/app_name", ScreenOrientation = ScreenOrientation.Portrait)]
    public class IntroActivity : AppCompatActivity 
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //Set the view of the main layout
            SetContentView(Resource.Layout.activity_intro);

            // Setup toolbar
            SupportWidget.Toolbar toolbar = FindViewById<SupportWidget.Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            SetSupportActionBar(toolbar);

            //create the ChestXRay AI model clickable
            TextView aboutTextView = FindViewById<TextView>(Resource.Id.txt_research_project);

            string modelName = GetString(Resource.String.intro_model_name);
            string researchProject = GetString(Resource.String.intro_research_project, modelName);
            SpannableString stringResearchProject = new SpannableString(researchProject);
            URLSpan clickableSpanChestXray = new URLSpan(SharedConstants.MLModelRepository);  
            
            // Make modelName clickable
            int modelNameIndex = researchProject.IndexOf(modelName, StringComparison.CurrentCulture);
            stringResearchProject.SetSpan(clickableSpanChestXray, modelNameIndex, modelNameIndex + modelName.Length, SpanTypes.ExclusiveExclusive);

            aboutTextView.TextFormatted = stringResearchProject;
            aboutTextView.MovementMethod = new LinkMovementMethod();

            FindViewById(Resource.Id.button_get_started).Click += delegate
            {
                ((MainApplication)this.Application).AppPrefManager.PassedIntroPage();
                StartActivity(new Intent(this, typeof(ImageInput.ImageInputActivity)));
                Finish();
            };
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.menu_intro, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_item_about:
                    Intent aboutIntent = new Intent(this, typeof(AboutActivity));
                    StartActivity(aboutIntent);
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}