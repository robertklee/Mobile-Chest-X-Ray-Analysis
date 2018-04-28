// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.Constraints;
using Android.Support.V4.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using ImageViews.Photo;

namespace XRayAnalysis.Droid.Results
{
    [Register("XRayAnalysis.Droid.Results.XRayDisplayView")]
    public class XRayDisplayView : ConstraintLayout
    {
        // References to small and large dimensions for text and label text for xray display
        private static readonly int DisplayModeTextSmallLabel = Resource.Dimension.xray_display_heading_text_size;
        private static readonly int DisplayModeTextLargeLabel = Resource.Dimension.xray_display_heading_text_size_large;
        private static readonly int DisplayModeTextSmall = Resource.Dimension.xray_display_text_size;
        private static readonly int DisplayModeTextLarge = Resource.Dimension.xray_display_text_size_large;

        // Sub-Views
        private TextView filenameView;
        private TextView dateView;
        private ZoomMaintainingPhotoView xRayImageView;
        private TextView filenameLabelView;
        private TextView dateLabelView;

        /// <summary>
        /// Controls whether the view is rendered in "small" or "large" mode
        /// </summary>
        private DisplayMode textDisplayMode;

        public DisplayMode TextDisplayMode
        {
            get { return textDisplayMode; }
            set
            {
                textDisplayMode = value;

                switch (textDisplayMode)
                {
                    case DisplayMode.Small:
                        // Set text size for small display
                        SetViewTextSize(filenameLabelView, DisplayModeTextSmallLabel);
                        SetViewTextSize(filenameView, DisplayModeTextSmall);
                        SetViewTextSize(dateLabelView, DisplayModeTextSmallLabel);
                        SetViewTextSize(dateView, DisplayModeTextSmall);
                        break;

                    case DisplayMode.Large:
                        // Set text size for large display
                        SetViewTextSize(filenameLabelView, DisplayModeTextLargeLabel);
                        SetViewTextSize(filenameView, DisplayModeTextLarge);
                        SetViewTextSize(dateLabelView, DisplayModeTextLargeLabel);
                        SetViewTextSize(dateView, DisplayModeTextLarge);
                        break;

                    default:
                        // Should never happen
                        throw new ArgumentException("displayMode must be one of...");
                }
            }
        }

        private string filename;

        /// <summary>
        /// Name of the file.
        /// </summary>
        public string Filename
        {
            get { return filename; }
            set
            {
                filename = value;
                filenameView.Text = filename;
            }
        }

        private string date;

        /// <summary>
        /// Date of the image.
        /// </summary>
        public string Date
        {
            get { return date; }
            set
            {
                date = value;
                dateView.Text = date;
            }
        }

        private string filenameLabel;

        /// <summary>
        /// Label for the filename.
        /// </summary>
        public string FilenameLabel
        {
            get { return filenameLabel; }
            set
            {
                filenameLabel = value;
                filenameLabelView.Text = filenameLabel;
            }
        }

        private string dateLabel;

        /// <summary>
        /// Label for the date.
        /// </summary>
        public string DateLabel
        {
            get { return dateLabel; }
            set
            {
                dateLabel = value;
                dateLabelView.Text = dateLabel;
            }
        }
      
        private Bitmap xRayImage;

        /// <summary>
        /// Image of the x-ray.
        /// </summary>
        public Bitmap XRayImage
        {
            get { return xRayImage; }
            set
            {
                xRayImage = value;
                xRayImageView.SetImageBitmap(xRayImage);
            }
        }

        /// <summary>
        /// Instantiates a new <see cref="XRayDisplayView"/>.
        /// Overrides <see cref="ConstraintLayout(Context)"/>
        /// </summary>
        public XRayDisplayView(Context context) : base(context)
        {
            Initialize(context, null, 0);
        }

        /// <summary>
        /// Instantiates a new <see cref="XRayDisplayView"/>. For use internally by Android when inflating XML layouts.
        /// Overrides <see cref="ConstraintLayout(Context, IAttributeSet)"/>
        /// </summary>
        public XRayDisplayView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(context, attrs, 0);
        }

        /// <summary>
        /// Instantiates a new <see cref="XRayDisplayView"/>. For use internally by Android when inflating XML layouts.
        /// Overrides <see cref="ConstraintLayout(Context, IAttributeSet, int)"/>
        /// </summary>
        public XRayDisplayView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize(context, attrs, defStyle);
        }

        public void SetXRayImageResetZoom(Bitmap bitmap)
        {
            xRayImage = bitmap;
            xRayImageView.SetImageBitmap(bitmap, true);
        }

        private void Initialize(Context context, IAttributeSet attrs, int defStyle)
        {
            if (context == null)
            {
                return;
            }

            TypedArray typedArray = context.ObtainStyledAttributes(attrs, Resource.Styleable.XRayDisplayView);

            InflateLayout(context);
            InitializeAttributes(typedArray);
        }
          
        /// <summary>
        /// Inflates the layout into this View
        /// </summary>
        /// <param name="context">A <see cref="Context"/> to use to inflate the layout</param>
        private void InflateLayout(Context context)
        {
            LayoutInflater inflater = LayoutInflater.FromContext(context);
            inflater.Inflate(Resource.Layout.view_xray_display, this);

            filenameView = FindViewById<TextView>(Resource.Id.txt_filename);
            dateView = FindViewById<TextView>(Resource.Id.txt_date);
            filenameLabelView = FindViewById<TextView>(Resource.Id.txt_filename_label);
            dateLabelView = FindViewById<TextView>(Resource.Id.txt_date_label);
            xRayImageView = FindViewById<ZoomMaintainingPhotoView>(Resource.Id.image_xray);
        }

        /// <summary>
        /// Reads any attributes that were assigned in XML and sets properties appropriately
        /// </summary>
        /// <param name="typedArray">The attributes to apply to the view</param>
        private void InitializeAttributes(TypedArray typedArray)
        {
            string attrFilename = typedArray.GetString(Resource.Styleable.XRayDisplayView_filename);
            Filename = attrFilename ?? ""; //if it is null it will be assigned an empty sting

            string attrDate = typedArray.GetString(Resource.Styleable.XRayDisplayView_date);
            Date = attrDate ?? ""; 

            string attrFilenameLabel = typedArray.GetString(Resource.Styleable.XRayDisplayView_filename_label);
            FilenameLabel = attrFilenameLabel ?? "";

            TextDisplayMode = (DisplayMode)typedArray.GetInt(Resource.Styleable.ConditionResultView_condition_displayMode, (int)DisplayMode.Small);
        }

        /// <summary>
        /// Uses the text size from a dimention and converts it to the correct size to set the proper text size for a view
        /// </summary>
        /// <param name="view">The view for the text size to be set in</param>
        /// <param name="dimenResId">The refernce to resource id for the text size dimensi
        private void SetViewTextSize(TextView view, int dimenResId)
        {
            view.SetTextSize(ComplexUnitType.Sp, Resources.GetDimension(dimenResId) / Resources.DisplayMetrics.Density);
        }

        public enum DisplayMode
        {
            // These integers must match those declared under "condition_displayMode" in attrs.xml

            Small = 0,
            Large = 1
        }
    }
}
