// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using ImageViews.Photo;

namespace XRayAnalysis.Droid.Results
{
    /// <summary>
    /// Custom ImageView that uses the PhotoView library to support zooming.
    /// This class provides more precise control over when the zoom is reset,
    /// which is not provided by the library's built in PhotoView class.
    /// </summary>
    [Register("XRayAnalysis.Droid.Results.ZoomMaintainingPhotoView")]
    public class ZoomMaintainingPhotoView : ImageView
    {
        private PhotoViewAttacher attacher;

        /// <summary>
        /// Instantiates a new <see cref="ZoomMaintainingPhotoView"/>.
        /// Overrides <see cref="PhotoView(Context)"/>
        /// </summary>
        public ZoomMaintainingPhotoView(Context context) : base(context)
        {
            Initialize();
        }

        /// <summary>
        /// Instantiates a new <see cref="ZoomMaintainingPhotoView"/>. For use internally by Android when inflating XML layouts.
        /// Overrides <see cref="PhotoView(Context, IAttributeSet)"/>
        /// </summary>
        public ZoomMaintainingPhotoView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        /// <summary>
        /// Instantiates a new <see cref="ZoomMaintainingPhotoView"/>. For use internally by Android when inflating XML layouts.
        /// Overrides <see cref="PhotoView(Context, IAttributeSet, int)"/>
        /// </summary>
        public ZoomMaintainingPhotoView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize();
        }

        /// <summary>
        /// Sets the <see cref="Bitmap"/> displayed by this ImageView.
        /// </summary>
        /// <param name="bitmap">The Bitmap to display.</param>
        /// <param name="resetZoom">
        ///     If <c>true</c> the zoom level will be reset, otherwise zoom will be maintained
        ///     under the assumption the new bitmap is the same size as the previous one.
        /// </param>
        public void SetImageBitmap(Bitmap bitmap, bool resetZoom)
        {
            base.SetImageBitmap(bitmap);

            if (resetZoom)
            {
                // This effectively resets the zoom level
                attacher.Update();
            }
        }

        /// <summary>
        /// Override of <see cref="ImageView.SetFrame"/> that also updates the PhotoViewAdapter.
        /// This is required to allow this view to resize smoothly.
        /// </summary>
        /// <param name="l">Left.</param>
        /// <param name="t">Top.</param>
        /// <param name="r">Right.</param>
        /// <param name="b">Bottom.</param>
        /// <returns><c>true</c>, if the position/size changed, <c>false</c> otherwise.</returns>
        protected override bool SetFrame(int l, int t, int r, int b)
        {
            bool changed = base.SetFrame(l, t, r, b);
            if (changed)
            {
                // This has the side effect of resetting the zoom level
                attacher.Update();
            }
            return changed;
        }

        private void Initialize()
        {
            attacher = new PhotoViewAttacher(this);
            SetScaleType(ScaleType.Matrix);
        }
    }
}
