// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Shwarm.MathUtils
{
    public struct NativeArray<T> : IDisposable, IEnumerable<T>, IEquatable<NativeArray<T>>, IEnumerable where T : struct
    {
        private Unity.Collections.NativeArray<T> impl;

        public int Length => impl.Length;

        public NativeArray(T[] array, Unity.Collections.Allocator allocator)
        {
            impl = new Unity.Collections.NativeArray<T>(array, allocator);
        }

        public NativeArray(NativeArray<T> array, Unity.Collections.Allocator allocator)
        {
            impl = new Unity.Collections.NativeArray<T>(array.impl, allocator);
        }

        public NativeArray(int length, Unity.Collections.Allocator allocator, Unity.Collections.NativeArrayOptions options = 0)
        {
            impl = new Unity.Collections.NativeArray<T>(length, allocator, options);
        }

        public void CopyFrom(T[] array)
        {
            impl.CopyFrom(array);
        }

        public void CopyFrom(NativeArray<T> array)
        {
            impl.CopyFrom(array.impl);
        }

        public void CopyTo(T[] array)
        {
            impl.CopyTo(array);
        }

        public void CopyTo(NativeArray<T> array)
        {
            impl.CopyTo(array.impl);
        }

        public T this[int i]
        {
            get => impl[i];
            set => impl[i] = value;
        }

        public void Dispose()
        {
            impl.Dispose();
        }

        public bool Equals(NativeArray<T> other)
        {
            return impl.Equals(other);
        }

        public override int GetHashCode()
        {
            return impl.GetHashCode();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return impl.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }
    }
}
