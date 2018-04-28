// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace XRayAnalysis.iOS.Results
{
    [Register ("ResultsViewController")]
    partial class ResultsViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel AnalyzedImageDateHeaderLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel AnalyzedImageDateLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel AnalyzedImageFileNameHeaderLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel AnalyzedImageFileNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView AnalyzedImageInfoView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint AnalyzedImageInfoViewBottomConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint AnalyzedImageInfoViewHeightConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton ButtonSeeMore { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint ConstraintImageViewWrapperTrailing { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint ConstraintPrimaryTableViewTop { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint ConstraintSecondaryTableViewBottom { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint ConstraintSecondaryTableViewTop { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView ImageScrollView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView ImageViewWrapper { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel OriginalImageDateHeaderLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel OriginalImageDateLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel OriginalImageFileNameHeaderLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel OriginalImageFileNameLabel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView OriginalImageInfoView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint OriginalImageInfoViewBottomConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.NSLayoutConstraint OriginalImageInfoViewHeightConstraint { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIScrollView OriginalImageScrollView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView OriginalImageWrapper { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView SecondaryTableViewConditionsList { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView SeeMoreWrapper { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView TableViewConditionsList { get; set; }

        [Action ("SeeMore:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SeeMore (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (AnalyzedImageDateHeaderLabel != null) {
                AnalyzedImageDateHeaderLabel.Dispose ();
                AnalyzedImageDateHeaderLabel = null;
            }

            if (AnalyzedImageDateLabel != null) {
                AnalyzedImageDateLabel.Dispose ();
                AnalyzedImageDateLabel = null;
            }

            if (AnalyzedImageFileNameHeaderLabel != null) {
                AnalyzedImageFileNameHeaderLabel.Dispose ();
                AnalyzedImageFileNameHeaderLabel = null;
            }

            if (AnalyzedImageFileNameLabel != null) {
                AnalyzedImageFileNameLabel.Dispose ();
                AnalyzedImageFileNameLabel = null;
            }

            if (AnalyzedImageInfoView != null) {
                AnalyzedImageInfoView.Dispose ();
                AnalyzedImageInfoView = null;
            }

            if (AnalyzedImageInfoViewBottomConstraint != null) {
                AnalyzedImageInfoViewBottomConstraint.Dispose ();
                AnalyzedImageInfoViewBottomConstraint = null;
            }

            if (AnalyzedImageInfoViewHeightConstraint != null) {
                AnalyzedImageInfoViewHeightConstraint.Dispose ();
                AnalyzedImageInfoViewHeightConstraint = null;
            }

            if (ButtonSeeMore != null) {
                ButtonSeeMore.Dispose ();
                ButtonSeeMore = null;
            }

            if (ConstraintImageViewWrapperTrailing != null) {
                ConstraintImageViewWrapperTrailing.Dispose ();
                ConstraintImageViewWrapperTrailing = null;
            }

            if (ConstraintPrimaryTableViewTop != null) {
                ConstraintPrimaryTableViewTop.Dispose ();
                ConstraintPrimaryTableViewTop = null;
            }

            if (ConstraintSecondaryTableViewBottom != null) {
                ConstraintSecondaryTableViewBottom.Dispose ();
                ConstraintSecondaryTableViewBottom = null;
            }

            if (ConstraintSecondaryTableViewTop != null) {
                ConstraintSecondaryTableViewTop.Dispose ();
                ConstraintSecondaryTableViewTop = null;
            }

            if (ImageScrollView != null) {
                ImageScrollView.Dispose ();
                ImageScrollView = null;
            }

            if (ImageViewWrapper != null) {
                ImageViewWrapper.Dispose ();
                ImageViewWrapper = null;
            }

            if (OriginalImageDateHeaderLabel != null) {
                OriginalImageDateHeaderLabel.Dispose ();
                OriginalImageDateHeaderLabel = null;
            }

            if (OriginalImageDateLabel != null) {
                OriginalImageDateLabel.Dispose ();
                OriginalImageDateLabel = null;
            }

            if (OriginalImageFileNameHeaderLabel != null) {
                OriginalImageFileNameHeaderLabel.Dispose ();
                OriginalImageFileNameHeaderLabel = null;
            }

            if (OriginalImageFileNameLabel != null) {
                OriginalImageFileNameLabel.Dispose ();
                OriginalImageFileNameLabel = null;
            }

            if (OriginalImageInfoView != null) {
                OriginalImageInfoView.Dispose ();
                OriginalImageInfoView = null;
            }

            if (OriginalImageInfoViewBottomConstraint != null) {
                OriginalImageInfoViewBottomConstraint.Dispose ();
                OriginalImageInfoViewBottomConstraint = null;
            }

            if (OriginalImageInfoViewHeightConstraint != null) {
                OriginalImageInfoViewHeightConstraint.Dispose ();
                OriginalImageInfoViewHeightConstraint = null;
            }

            if (OriginalImageScrollView != null) {
                OriginalImageScrollView.Dispose ();
                OriginalImageScrollView = null;
            }

            if (OriginalImageWrapper != null) {
                OriginalImageWrapper.Dispose ();
                OriginalImageWrapper = null;
            }

            if (SecondaryTableViewConditionsList != null) {
                SecondaryTableViewConditionsList.Dispose ();
                SecondaryTableViewConditionsList = null;
            }

            if (SeeMoreWrapper != null) {
                SeeMoreWrapper.Dispose ();
                SeeMoreWrapper = null;
            }

            if (TableViewConditionsList != null) {
                TableViewConditionsList.Dispose ();
                TableViewConditionsList = null;
            }
        }
    }
}