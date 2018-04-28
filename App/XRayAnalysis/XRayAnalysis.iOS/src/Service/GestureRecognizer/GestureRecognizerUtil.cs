// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace XRayAnalysis.iOS.Service
{
    /// <summary>
    /// A utility class for functions that work with UIGestureRecognizers.
    /// </summary>
    public static class GestureRecognizerUtil
    {
        // NOTE: this method only works with single line UILabels, or multi-line labels with left aligned text
        // Potential fix found on timbroder's comment on https://stackoverflow.com/questions/1256887/create-tap-able-links-in-the-nsattributedstring-of-a-uilabel
        // Based on https://stackoverflow.com/questions/1256887/create-tap-able-links-in-the-nsattributedstring-of-a-uilabel
        // Based on https://samwize.com/2016/03/04/how-to-create-multiple-tappable-links-in-a-uilabel/
        /// <summary>
        /// Determines whether the tap is within the NSRange targetRange. Returns true if so, otherwise returns false.
        /// </summary>
        /// <returns><c>true</c>, if tap was within the targetRange of the label, <c>false</c> otherwise.</returns>
        /// <param name="label">The label containing the text.</param>
        /// <param name="targetRange">The target range of the tappable text within the label.</param>
        /// <param name="gestureRecognizer">Gesture recognizer.</param>
        /// <param name="textAlignment">Text alignment of the label.</param>
        public static bool DidTapAttributedTextInLabel(UILabel label, NSRange targetRange, UIGestureRecognizer gestureRecognizer, UITextAlignment textAlignment)
        {
            NSLayoutManager layoutManager = new NSLayoutManager();
            NSTextContainer textContainer = new NSTextContainer(CGSize.Empty);
            NSTextStorage textStorage = new NSTextStorage();
            textStorage.SetString(label.AttributedText);

            // Configure layoutManager and textStorage
            layoutManager.AddTextContainer(textContainer);
            textStorage.AddLayoutManager(layoutManager);

            // Configure textContainer
            textContainer.LineFragmentPadding = 0.0f;
            textContainer.LineBreakMode = label.LineBreakMode;
            textContainer.MaximumNumberOfLines = (System.nuint)label.Lines;

            CGSize labelSize = label.Bounds.Size;
            textContainer.Size = labelSize;

            // Find the tapped character location and compare it to the specified range
            CGPoint locationOfTouchInLabel = gestureRecognizer.LocationInView(label);
            CGRect textBoundingBox = layoutManager.GetUsedRectForTextContainer(textContainer);

            // Change based on the UITextAlignment enum
            float multiplier;

            if (textAlignment == UITextAlignment.Center)
            {
                multiplier = 0.5f;
            }
            else if (textAlignment == UITextAlignment.Right)
            {
                multiplier = 1.0f;
            }
            else
            {
                // textAlignment is Left, Natural, or Justified
                multiplier = 0.0f;
            }

            CGPoint textContainerOffset = new CGPoint(
                ((labelSize.Width - textBoundingBox.Size.Width) * multiplier - textBoundingBox.Location.X),
                ((labelSize.Height - textBoundingBox.Size.Height) * multiplier - textBoundingBox.Location.Y)
            );
            CGPoint locationOfTouchInTextContainer = new CGPoint(locationOfTouchInLabel.X - textContainerOffset.X, locationOfTouchInLabel.Y - textContainerOffset.Y);
            nfloat partialFraction = 0;
            int indexOfCharacter = (int) layoutManager.CharacterIndexForPoint(locationOfTouchInTextContainer, textContainer, ref partialFraction);

            // Xamarin version of the function NSLocationInRange (Swift equivalent is to call ```NSLocationInRange(indexOfCharacter, targetRange);``` )
            // Credit to https://forums.xamarin.com/discussion/56484/need-to-put-html-into-a-label
            bool isInRange = (indexOfCharacter >= targetRange.Location) && (indexOfCharacter <= targetRange.Location + targetRange.Length);

            return isInRange;
        }
    }
}
