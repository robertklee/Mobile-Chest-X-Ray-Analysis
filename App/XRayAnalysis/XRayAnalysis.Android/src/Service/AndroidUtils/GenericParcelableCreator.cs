// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Android.OS;

namespace XRayAnalysis.Droid.Service.AndroidUtils
{
    /// <summary>
    /// Generic implementation of <see cref="IParcelableCreator"/> that can be re-used for any <see cref="IParcelable"/> type.
    /// Based on: http://dan.clarke.name/2012/09/implementing-iparcelable-in-mono-for-android/
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="IParcelable"/> this class creates.</typeparam>
    public sealed class GenericParcelableCreator<T> : Java.Lang.Object, IParcelableCreator where T : Java.Lang.Object, new()
    {
        private readonly Func<Parcel, T> createFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericParcelableCreator{T}"/> class.
        /// </summary>
        /// <param name='createFromParcelFunc'>
        /// Func that creates an instance of T, populated with the values from the parcel parameter
        /// </param>
        public GenericParcelableCreator(Func<Parcel, T> createFromParcelFunc)
        {
            createFunc = createFromParcelFunc;
        }
        
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            return createFunc(source);
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new T[size];
        }
    }
}