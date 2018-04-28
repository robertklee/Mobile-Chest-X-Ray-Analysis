// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.V4.Content;
using Android.Support.V4.View;
using Android.Support.V4.View.Accessibility;
using Android.Util;
using Android.Views;
using Android.Views.Accessibility;
using Android.Widget;

namespace XRayAnalysis.Droid.Results
{
    /// <summary>
    /// Custom View for displaying condition results (condition name, weight and likelihood)
    /// </summary>
    [Register("XRayAnalysis.Droid.Results.ConditionResultView")]
    public class ConditionResultView : ConstraintLayout
    {
        // References to layouts
        private static readonly int DisplayModeNormalLayout = Resource.Layout.view_condition_result;
        private static readonly int DisplayModeLargeLayout = Resource.Layout.view_condition_result_large;

        // References to colors
        private static readonly int BGColorNormal = Resource.Color.pale_grey;
        private static readonly int TextColorNormal = Resource.Color.grey;
        private static readonly int TextColorHighlight = Resource.Color.white;

        // References to drawables
        private static readonly int IconEyeHidden = Resource.Drawable.ic_eye_hidden;
        private static readonly int IconEyeVisible = Resource.Drawable.ic_eye_white;

        // Sub-views
        private TextView conditionNameView;
        private TextView likelihoodView;

        private TextView likelihoodLabelView;
        private ImageView visibilityButtonView;

        /// <summary>
        /// Controls whether the view is rendered in "normal" or "large" mode
        /// </summary>
        private DisplayMode displayMode;

        private string conditionName;

        /// <summary>
        /// Name of the condition.
        /// </summary>
        public string ConditionName
        {
            get { return conditionName; }
            set
            {
                conditionName = value;
                conditionNameView.Text = conditionName;
            }
        }

        private double likelihood;

        /// <summary>
        /// Likelihood of the condition (as a decimal, not a percentage).
        /// </summary>
        public double Likelihood
        {
            get { return likelihood; }
            set
            {
                likelihood = value;
                likelihoodView.Text = string.Format("{0:0.0}%", likelihood * 100);
            }
        }

        private string likelihoodLabel;

        /// <summary>
        /// Label for the likelihood
        /// </summary>
        public string LikelihoodLabel
        {
            get { return likelihoodLabel; }
            set
            {
                likelihoodLabel = value;
                likelihoodLabelView.Text = likelihoodLabel;
            }
        }

        private Color highlightColor;

        public Color HighlightColor
        {
            get { return highlightColor; }
            set
            {
                highlightColor = value;

                // Trigger the highlighting to update
                Highlighted = Highlighted;
            }
        }

        private bool highlighted;

        public bool Highlighted
        {
            get { return highlighted; }
            set
            {
                highlighted = value;

                Color bgColor = new Color(ContextCompat.GetColor(this.Context, BGColorNormal));
                int textColorRes = TextColorNormal;
                int iconRes = IconEyeHidden;

                if (highlighted)
                {
                    bgColor = highlightColor;
                    textColorRes = TextColorHighlight;
                    iconRes = IconEyeVisible;
                }

                // Update background
                SetBackgroundColor(bgColor);

                // Update text colors
                Color textColor = new Color(ContextCompat.GetColor(this.Context, textColorRes));

                conditionNameView.SetTextColor(textColor);
                likelihoodView.SetTextColor(textColor);
                likelihoodLabelView.SetTextColor(textColor);

                // Update Icon
                visibilityButtonView.SetImageResource(iconRes);
            }
        }

        /// <summary>
        /// Instantiates a new <see cref="ConditionResultView"/>.
        /// Overrides <see cref="ConstraintLayout(Context)"/>
        /// </summary>
        public ConditionResultView(Context context) : base(context)
        {
            Initialize(context, null, 0);
        }

        /// <summary>
        /// Instantiates a new <see cref="ConditionResultView"/>. For use internally by Android when inflating XML layouts.
        /// Overrides <see cref="ConstraintLayout(Context, IAttributeSet)"/>
        /// </summary>
        public ConditionResultView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(context, attrs, 0);
        }

        /// <summary>
        /// Instantiates a new <see cref="ConditionResultView"/>. For use internally by Android when inflating XML layouts.
        /// Overrides <see cref="ConstraintLayout(Context, IAttributeSet, int)"/>
        /// </summary>
        public ConditionResultView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize(context, attrs, defStyle);
        }

        private void Initialize(Context context, IAttributeSet attrs, int defStyle)
        {
            if (context == null)
            {
                return;
            }

            TypedArray typedArray = context.ObtainStyledAttributes(attrs, Resource.Styleable.ConditionResultView);
            displayMode = (DisplayMode)typedArray.GetInt(Resource.Styleable.ConditionResultView_condition_displayMode, (int)DisplayMode.Normal);

            InflateLayout(context);
            InitializeAttributes(typedArray);

            ViewCompat.SetAccessibilityDelegate(this, new ConditionResultAccessibilityDelegate());
        }

        /// <summary>
        /// Inflates the layout into this View
        /// </summary>
        /// <param name="context">A <see cref="Context"/> to use to inflate the layout</param>
        private void InflateLayout(Context context)
        {
            LayoutInflater inflater = LayoutInflater.FromContext(context);

            if (displayMode == DisplayMode.Large)
            {
                inflater.Inflate(DisplayModeLargeLayout, this);
            }
            else
            {
                inflater.Inflate(DisplayModeNormalLayout, this);
            }

            conditionNameView = FindViewById<TextView>(Resource.Id.txt_condition_name);
            likelihoodView = FindViewById<TextView>(Resource.Id.txt_likelihood_value);

            likelihoodLabelView = FindViewById<TextView>(Resource.Id.txt_likelihood_label);
            visibilityButtonView = FindViewById<ImageView>(Resource.Id.icon_visibility);
        }

        /// <summary>
        /// Reads any attributes that were assigned in XML and sets properties appropriately
        /// </summary>
        /// <param name="typedArray">The attributes to apply to the view</param>
        private void InitializeAttributes(TypedArray typedArray)
        {
            string attrConditionName = typedArray.GetString(Resource.Styleable.ConditionResultView_condition_name);
            ConditionName = attrConditionName ?? "";

            Likelihood = typedArray.GetFloat(Resource.Styleable.ConditionResultView_condition_likelihood, 0);

            string attrLikelihoodLabel = typedArray.GetString(Resource.Styleable.ConditionResultView_condition_likelihood_label);
            LikelihoodLabel = attrLikelihoodLabel ?? "";
        }

        private enum DisplayMode
        {
            // These integers must match those declared under "condition_displayMode" in attrs.xml

            Normal = 0,
            Large = 1
        }

        /// <summary>
        /// Class to provide state and action information to accessibility services acting on this view.
        /// </summary>
        private class ConditionResultAccessibilityDelegate : AccessibilityDelegateCompat
        {
            public override void OnInitializeAccessibilityNodeInfo(View host, AccessibilityNodeInfoCompat info)
            {
                base.OnInitializeAccessibilityNodeInfo(host, info);

                // Provide information about what action the user can take
                AccessibilityNodeInfoCompat.AccessibilityActionCompat action = new AccessibilityNodeInfoCompat.AccessibilityActionCompat(
                    AccessibilityNodeInfoCompat.ActionClick,
                    host.Resources.GetString(Resource.String.condition_results_accessibility_action));
                info.AddAction(action);

                // Update the selected state
                info.Checkable = true;
                info.Checked = ((ConditionResultView)host).Highlighted;
            }

            public override void OnInitializeAccessibilityEvent(View host, AccessibilityEvent @event)
            {
                base.OnInitializeAccessibilityEvent(host, @event);

                // Update the state information when an event fires
                @event.Checked = ((ConditionResultView)host).Highlighted;
            }
        }
    }
}