// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace XRayAnalysis.iOS.Results
{
    /// <summary>
    /// Custom UITableViewCell for displaying condition results
    /// </summary>
    public partial class ConditionViewCell : UITableViewCell
    {
        // Cell reuse identifier
        public static readonly NSString CellReuseId = new NSString("ConditionViewCell");
        public static readonly NSString PrimaryCellReuseId = new NSString("PrimaryConditionViewCell");

        // Key to associated .xib file
        public static readonly NSString XIBKey = new NSString("Condition");
        public static readonly NSString PrimaryXIBKey = new NSString("PrimaryCondition");

        // Default cell background color
        private readonly UIColor defaultBGColor = Constants.PaleGrey;

        private string accessibilityHint;

        /// <summary>
        /// Required constructor for <see cref="UITableViewCell"/>
        /// </summary>
        /// <param name="handle">Handle.</param>
        protected ConditionViewCell(IntPtr handle) : base(handle)
        { }

        /// <summary>
        /// Prepares cell for service after it has been loaded in
        /// </summary>
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            accessibilityHint = this.AccessibilityHint;

            this.AccessibilityElementsHidden = true;
        }

        private string conditionName;

        /// <summary>
        /// Name of the condition
        /// </summary>
        /// <value>The name of the condition.</value>
        public string ConditionName
        {
            get { return conditionName; }
            set
            {
                conditionName = value;
                this.ConditionLabel.Text = conditionName;
            }
        }

        private double likelihood;

        /// <summary>
        /// Likelihood of the condition (as a percentage, not a decimal).
        /// </summary>
        public double Likelihood
        {
            get { return likelihood; }
            set
            {
                likelihood = value;
                LikelihoodLabel.Text = string.Format("{0:0.0}%", likelihood * 100);
            }
        }

        private string likelihoodText;

        /// <summary>
        /// Text description likelihood of the condition
        /// </summary>
        public string LikelihoodText
        {
            get { return conditionName; }
            set
            {
                likelihoodText = value;
                this.LikelihoodTextLabel.Text = likelihoodText;
            }
        }

        private UIColor highlightedBgColor;

        /// <summary>
        /// Gets or sets the color of the highlighted background
        /// </summary>
        /// <value>The color of the highlighted background</value>
        public UIColor HighlightedBgColor
        {
            get { return highlightedBgColor; }
            set
            {
                highlightedBgColor = value;
            }
        }

        private bool isHighlighted;

        /// <summary>
        /// Sets the cell to be either be unselected (not highlighted) or selected (highlighted)
        /// Modifies the graphical content of the cell as needed 
        /// </summary>
        /// <value><c>true</c> if cell is highlighted; otherwise, <c>false</c>.</value>
        public bool IsHighlighted
        {
            get { return isHighlighted; }
            set
            {
                isHighlighted = value;

                UIImage eyeImage = UIImage.FromBundle("eye-hide");
                UIColor bgColor = defaultBGColor;
                UIColor textColor = Constants.Grey;

                this.AccessibilityHint = accessibilityHint + " On";
                this.AccessibilityTraits &= ~(UIAccessibilityTrait.Selected);

                // If highlighted, set assets
                if (isHighlighted)
                {
                    eyeImage = UIImage.FromBundle("eye-show");
                    bgColor = highlightedBgColor;
                    textColor = Constants.White;

                    this.AccessibilityHint = accessibilityHint + " Off";
                    this.AccessibilityTraits |= UIAccessibilityTrait.Selected;
                }

                ContentView.BackgroundColor = bgColor;
                ConditionLabel.TextColor = textColor;
                LikelihoodLabel.TextColor = textColor;
                LikelihoodTextLabel.TextColor = textColor;
                EyeImage.Image = eyeImage;
            }
        }

        /// <summary>
        /// Configure cell labels to function with dynamic text
        /// </summary>
        public void ConfigureAccessibleLabels()
        {
            UIFont conditionFont;
            UIFont likelihoodLabelFont;
            UIFont likelihoodTextLabelFont;

            int conditionFontSize, likelihoodFontSize, likelihoodTextFontSize;

            // Assign fonts dependent on cell type
            if (this.ReuseIdentifier.Equals(PrimaryCellReuseId))
            {
                // Primary cell
                conditionFontSize = likelihoodFontSize = 32;
                likelihoodTextFontSize = 25;
            }
            else
            {
                // Standard cell
                conditionFontSize = 16;
                likelihoodFontSize = 24;
                likelihoodTextFontSize = 14;
            }

            // Assign fonts
            conditionFont = UIFontMetrics.DefaultMetrics.GetScaledFont(
                    UIFont.FromDescriptor(ConditionLabel.Font.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold), conditionFontSize)
                );

            likelihoodLabelFont = UIFontMetrics.DefaultMetrics.GetScaledFont(
                UIFont.FromDescriptor(LikelihoodLabel.Font.FontDescriptor, likelihoodFontSize)
            );

            likelihoodTextLabelFont = UIFontMetrics.DefaultMetrics.GetScaledFont(
                UIFont.FromDescriptor(LikelihoodTextLabel.Font.FontDescriptor.CreateWithTraits(UIFontDescriptorSymbolicTraits.Bold), likelihoodTextFontSize)
            );

            ConditionLabel.Font = conditionFont;
            LikelihoodLabel.Font = likelihoodLabelFont;
            LikelihoodTextLabel.Font = likelihoodTextLabelFont;

            ConditionLabel.AdjustsFontForContentSizeCategory
                = LikelihoodLabel.AdjustsFontForContentSizeCategory
                = LikelihoodTextLabel.AdjustsFontForContentSizeCategory = true;
        }
    }
}
