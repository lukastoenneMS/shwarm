// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Shwarm.Vdb
{
    public interface IKeyframeFeature
    {
    }

    public class InstanceKeyframeFeature<T> : IEnumerable<KeyValuePair<int, T>>, IKeyframeFeature
    {
        private readonly Dictionary<int, T> instanceData = new Dictionary<int, T>();

        public void Store(int id, T data)
        {
            instanceData[id] = data;
        }

        public bool TryGetValue(int id, out T data)
        {
            return instanceData.TryGetValue(id, out data);
        }

        // Must implement GetEnumerator, which returns a new StreamReaderEnumerator.
        public IEnumerator<KeyValuePair<int, T>> GetEnumerator()
        {
            return instanceData.GetEnumerator();
        }

        // Must also implement IEnumerable.GetEnumerator, but implement as a private method.
        private IEnumerator GetEnumerator1()
        {
            return this.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }
    }

    public class Keyframe
    {
        public double timestamp;

        private readonly Dictionary<Type, object> featureData = new Dictionary<Type, object>();

        public Keyframe()
        {
        }

        public bool TryGetData<T>(out T data) where T : class, IKeyframeFeature
        {
            if (featureData.TryGetValue(typeof(T), out object value))
            {
                data = value as T;
                return true;
            }
            data = null;
            return false;
        }

        public T GetOrCreateData<T>() where T : class, IKeyframeFeature, new()
        {
            T data;
            if (featureData.TryGetValue(typeof(T), out object value))
            {
                data = value as T;
            }
            else
            {
                data = new T();
                featureData.Add(typeof(T), data);
            }
            return data;
        }
    }
}
