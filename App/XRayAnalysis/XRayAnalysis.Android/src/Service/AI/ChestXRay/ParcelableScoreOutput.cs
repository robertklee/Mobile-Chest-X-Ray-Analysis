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
    /// A wrapper class around <see cref="ScoreOutput"/> to support parceling in Android.
    /// This is required to be able to pass objects of this type between Activities.
    /// </summary>
    public class ParcelableScoreOutput : Java.Lang.Object, IParcelable
    {
        private static readonly GenericParcelableCreator<ParcelableScoreOutput> creator = new GenericParcelableCreator<ParcelableScoreOutput>((parcel) => new ParcelableScoreOutput(parcel));

        /// <summary>
        /// Method required by <see cref="IParcelable"/> for parceling/unparceling.
        /// Exposes the private <see cref="creator"/> field to Java code though the <see cref="ExportFieldAttribute"/>
        /// </summary>
        [ExportField("CREATOR")]
        public static GenericParcelableCreator<ParcelableScoreOutput> GetCreator()
        {
            return creator;
        }

        /// <summary>
        /// The actual <see cref="ScoreOutput"/> object wrapped by this object.
        /// </summary>
        public ScoreOutput Value { get; }

        /// <summary>
        /// This constructor is required by <see cref="IParcelableCreator"/>
        /// </summary>
        public ParcelableScoreOutput()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParcelableScoreOutput"/> class to wrap the provided <see cref="ScoreOutput"/>.
        /// </summary>
        /// <param name="value">The <see cref="ScoreOutput"/> to wrap.</param>
        public ParcelableScoreOutput(ScoreOutput value)
        {
            this.Value = value;
        }
        
        private ParcelableScoreOutput(Parcel parcel)
        {
            float likelihood = parcel.ReadFloat();

            this.Value = new ScoreOutput(likelihood);
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
        /// Writes the data from the wrapped <see cref="ScoreOutput"/> object.
        /// </summary>
        public void WriteToParcel(Parcel dest, [GeneratedEnum] ParcelableWriteFlags flags)
        {
            dest.WriteFloat(this.Value.Likelihood);
        }
    }
}