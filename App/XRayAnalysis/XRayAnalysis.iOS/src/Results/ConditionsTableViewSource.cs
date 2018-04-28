// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using XRayAnalysis.Results;
using XRayAnalysis.Service.AI.ChestXRay;
using Foundation;
using SkiaSharp.Views.iOS;
using UIKit;

namespace XRayAnalysis.iOS.Results
{
    /// <summary>
    /// Delegate class for the table view for see more
    /// </summary>
    public class ConditionsTableViewSource : UITableViewSource
    {
        private ConditionResult[] models;
        private Action<int> clickAction;
        private TableType tableType;
        private nfloat height;
        private bool seeMoreVisible;

        /// <summary>
        /// Enum representing the type of this table (either a Primary or Secondary table)
        /// </summary>
        public enum TableType
        {
            Primary = 0,
            Secondary = 1
        }

        /// <summary>
        /// Boolean representing whether the SeeMore is activated within results
        /// </summary>
        /// <value>Boolean for if see more is activated</value>
        public bool SeeMoreVisible
        {
            set
            {
                this.seeMoreVisible = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionsTableViewSource"/>
        /// </summary>
        /// <param name="results">Array of conditions</param>
        /// <param name="clickAction">Action invoked when a cell is tapped</param>
        /// <param name="type">Type of the table view (either primary or secondary)</param>
        /// <param name="height">Height of the primary table cell</param>
        public ConditionsTableViewSource(ConditionResult[] results, Action<int> clickAction, TableType type, nfloat height)
        {
            this.models = results;
            this.clickAction = clickAction;
            this.tableType = type;
            this.height = height;

            // We initially start in a state where see more is disabled
            this.seeMoreVisible = false;
        }

        /// <summary>
        /// Get a cell to be rendered to the UITableView
        /// NOTE: This method is called:
        /// - At initialization (for all cells)
        /// - When a cell is about to be rendered to the screen (for particular cell)
        /// - When tableView.ReloadData() is called (for all cells)  <see cref="RowSelected(UITableView, NSIndexPath)"/>
        /// </summary>
        /// <param name="tableView">UITableView associated with this UITableViewSource</param>
        /// <param name="indexPath">Index of cell</param>
        /// <returns>The cell to be displayed</returns>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Get cell
            ConditionViewCell cell;

            // PrimaryCondition.xib
            if (indexPath.Row == 0 && tableType == TableType.Primary)
            {
                cell = tableView.DequeueReusableCell(ConditionViewCell.PrimaryCellReuseId) as ConditionViewCell;

                // The primary cell's voice over will always be enabled
                // as it is always visible
                EnableCellVoiceOver(cell, true);
            }
            else
            {
                // Condition.xib
                cell = tableView.DequeueReusableCell(ConditionViewCell.CellReuseId) as ConditionViewCell;

                // Enable/disable voice over for secondary cells
                bool enable = tableType == TableType.Secondary ? !seeMoreVisible : seeMoreVisible;
                EnableCellVoiceOver(cell, enable);
            }

            // Turn off selection styling
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            // Set Background color
            cell.HighlightedBgColor = models[indexPath.Row].HighlightColor.ToUIColor();

            // Check if the cell is highlighted
            cell.IsHighlighted = models[indexPath.Row].Selected;

            // Set Labels of cell to be accessible
            cell.ConfigureAccessibleLabels();

            // Set attributes of cell
            string condition = Constants.ConditionList[(int)models[indexPath.Row].Condition];
            float likelihood = models[indexPath.Row].Score.Likelihood;
            cell.ConditionName = condition;
            cell.Likelihood = likelihood;

            // Set Likelihood label for cell
            LikelihoodLabel enumlabel = LikelihoodLabelHelper.FromLikelihood(likelihood);
            cell.LikelihoodText = Constants.LikelihoodLabel[(int)enumlabel];

            return cell;
        }

        /// <summary>
        /// Enables a given cell voice over
        /// </summary>
        /// <param name="cell">Cell</param>
        /// <param name="enabled">Enabled boolean</param>
        private void EnableCellVoiceOver(UITableViewCell cell, bool enabled)
        {
            cell.AccessibilityElementsHidden = !enabled;
        }

		/// <summary>
		/// On cell selection
		/// </summary>
		/// <param name="tableView">UITableView associated with this UITableViewSource</param>
		/// <param name="indexPath">Index of cell being tapped</param>
		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            int row = (tableType == TableType.Primary) ? indexPath.Row : indexPath.Row + 1;

            // Invoke the Action provided
            clickAction.Invoke(row);
        }

        /// <summary>
        /// The number of rows in the table
        /// NOTE: We only have 1 section, so this param is ignored
        /// </summary>
        /// <returns>Number of rows</returns>
        /// <param name="tableview">UITableView associated with this UITableViewSource</param>
        /// <param name="section">Section (ignored)</param>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            // Only 1 row is visible for the primary table when See more is NOT visible
            if (tableType == TableType.Primary && !this.seeMoreVisible)
            {
                return 1;
            }

            return models.Length;
        }

        /// <summary>
        /// Return the height for a given row
        /// </summary>
        /// <returns>The height of the row</returns>
        /// <param name="tableView">UITableView associated with this UITableViewSource</param>
        /// <param name="indexPath">Index path of row</param>
        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            // Check for PrimaryCondition.xib cell
            if (tableType == TableType.Primary && indexPath.Row == 0) 
            { 
                // Return primary cell height (entire height)
                return height; 
            }

            // Secondary cell height
            return height / 2;
        }
    }
}
