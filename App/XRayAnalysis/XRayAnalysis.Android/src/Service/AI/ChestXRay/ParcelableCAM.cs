// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Android.OS;
using Android.Runtime;
using XRayAnalysis.Droid.Service.AndroidUtils;
using XRayAnalysis.Service.AI.ChestXRay;
using Java.Interop;

namespace XRayAnalysis.Droid.Service.AI.ChestXRay
{
    /// <summary>
    /// A wrapper class around <see cref="CAM"/> to support parceling in Android.
    /// This is required to be able to pass objects of this type between Activities.
    /// </summary>
    class ParcelableCAM : Java.Lang.Object, IParcelable
    {
        private static readonly GenericParcelableCreator<ParcelableCAM> creator = new GenericParcelableCreator<ParcelableCAM>((parcel) => new ParcelableCAM(parcel));

        /// <summary>
        /// Method required by <see cref="IParcelable"/> for parceling/unparceling.
        /// Exposes the private <see cref="creator"/> field to Java code though the <see cref="ExportFieldAttribute"/>
        /// </summary>
        [ExportField("CREATOR")]
        public static GenericParcelableCreator<ParcelableCAM> GetCreator()
        {
            return creator;
        }

        /// <summary>
        /// The actual <see cref="CAM"/> object wrapped by this object.
        /// </summary>
        public CAM Value { get; }

        /// <summary>
        /// This constructor is required by <see cref="IParcelableCreator"/>
        /// </summary>
        public ParcelableCAM()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelableCAM"/> class to wrap the provided <see cref="CAM"/>.
        /// </summary>
        /// <param name="value">The <see cref="CAM"/> to wrap.</param>
        public ParcelableCAM(CAM value)
        {
            this.Value = value;
        }

        /// <summary>
        /// Reconstructs a <see cref="ParcelableCAM"/> object from the provided <see cref="Parcel"/>  
        /// </summary>
        /// <param name="parcel">The <see cref="Parcel"/> containing the data to use for reconstruction.</param>
        private ParcelableCAM(Parcel parcel)
        {
            // Read out the dimensions to initialize the array
            // (order matches the order they were written)
            int rows = parcel.ReadInt();
            int cols = parcel.ReadInt();

            float[,] camData = new float[rows, cols];

            // Deserialize the 1D array to a 2D array
            float[] camData1D = new float[rows * cols];
            parcel.ReadFloatArray(camData1D);

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    camData[row, col] = camData1D[row * cols + col];
                }
            }

            this.Value = new CAM(camData);
        }

        /// <summary>
        /// Implementation of <see cref="IParcelable.DescribeContents()"/>
        /// </summary>
        public int DescribeContents()
        {
            // Returning 0 means there's no "special objects" in the parcel
            // See: https://developer.android.com/reference/android/os/Parcelable.html#describeContents()
            return 0;
        }

        /// <summary>
        /// Implementation of <see cref="IParcelable.WriteToParcel(Parcel, ParcelableWriteFlags)"/>.
        /// Writes the data from the wrapped <see cref="CAM"/> object.
        /// </summary>
        public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
        {
            // Write the dimensions of the array for when we need to recreate it
            // (order matches the order they will be read)
            int rows = Value.Rows;
            int cols = Value.Cols;

            dest.WriteInt(rows);
            dest.WriteInt(cols);

            // Serialize the 2D array into a 1D array
            float[] camData = new float[rows * cols];
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    camData[row * cols + col] = Value[row, col];
                }
            }

            dest.WriteFloatArray(camData);
        }
    }
}