// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Android.Support.V7.Widget;
using Android.Views;
using XRayAnalysis.Results;
using XRayAnalysis.Service.AI.ChestXRay;
using SkiaSharp.Views.Android;

namespace XRayAnalysis.Droid.Results
{
    /// <summary>
    /// A class to hold references to the UI components (Custom view of the results) that is displayed in a row of the RecyclerView.
    /// </summary>
    public class ResultViewHolder : RecyclerView.ViewHolder
    {
        public ConditionResultView ResultView { get; private set; }

        /// <summary>
        /// Get references to the views for the ConditionResultsViews
        /// </summary>
        public ResultViewHolder(View itemView, Action<int> listener) : base(itemView)
        {
            // Locate and cache view reference
            ResultView = (ConditionResultView)itemView;

            // Detect user clicks on the item view and report which item was clicked (by layout position) to the listener
            itemView.Click += (sender, e) => listener(base.LayoutPosition);
        }
    }

    /// <summary>
    /// Adapter to connect the the results data to the RecyclerView. 
    /// </summary>
    public class ResultsAdapter : RecyclerView.Adapter
    {
        // Event handler for item clicks
        public event EventHandler<int> ClickHandler;

        // Underlying data set the results
        public ConditionResult[] ConditionResults;

        private string[] conditionNames;
        private string[] likelihoodLabels;

        /// <summary>
        /// Constuctor to load the adapter with the data set.
        /// </summary>
        public ResultsAdapter(ConditionResult[] conditionResults, string[] conditionNames, string[] likelihoodLabels, EventHandler<int> clickHandler)
        {
            this.ConditionResults = conditionResults;
            this.conditionNames = conditionNames;
            this.likelihoodLabels = likelihoodLabels;
            this.ClickHandler = clickHandler;
        }

        /// <summary>
        /// Determines the view type to use for the element at the given position.
        /// </summary>
        /// <param name="position">The position of the element.</param>
        /// <returns>The item view type.</returns>
        public override int GetItemViewType(int position)
        {
            // The first item should appear LARGE, the rest should appear NORMAL
            return (int)(position == 0 ? ItemViewType.LARGE : ItemViewType.NORMAL);
        }

        /// <summary>
        /// Create a new view (invoked by the layout manager).
        /// </summary>
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            // Determine the layout to use based on the view type
            int layoutResId = Resource.Layout.item_condition_result;
            if (viewType == (int)ItemViewType.LARGE)
            {
                layoutResId = Resource.Layout.item_condition_result_large;
            }

            // Inflate the View for the results
            View itemView = LayoutInflater.From(parent.Context).Inflate(layoutResId, parent, false);

            // Create a ViewHolder to find and hold these view references, and register OnClick with the view holder
            return new ResultViewHolder(itemView, OnClick);
        }

        /// <summary>
        /// Fill in the contents of the view (invoked by the layout manager)
        /// </summary>
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ResultViewHolder vh = holder as ResultViewHolder;

            vh.ResultView.ConditionName = conditionNames[(int)ConditionResults[position].Condition];
            vh.ResultView.Likelihood = ConditionResults[position].Score.Likelihood;
            vh.ResultView.LikelihoodLabel = likelihoodLabels[(int)LikelihoodLabelHelper.FromLikelihood(ConditionResults[position].Score.Likelihood)];
            vh.ResultView.Highlighted = ConditionResults[position].Selected;
            vh.ResultView.HighlightColor = ConditionResults[position].HighlightColor.ToColor();
        }

        /// <summary>
        /// Gets the item count.
        /// </summary>
        /// <value>The number of items in the dataset backing this adapter.</value>
        public override int ItemCount
        {
            get
            {
                return ConditionResults.Length;
            }
        }

        /// <summary>
        /// Raise an event when the item-click takes place.
        /// </summary>
        /// <param name="position">Position.</param>
        void OnClick(int position)
        {
            ClickHandler?.Invoke(this, position);
        }

        /// <summary>
        /// List of view types for elements in this adapter.
        /// </summary>
        private enum ItemViewType
        {
            NORMAL,
            LARGE
        }
    }
}
