// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Boids;
using UnityEngine;
using System.Collections.Generic;

namespace Shwarm.Vdb
{
    public struct DataBlob
    {
        public struct BoidState
        {
            public Vector3 position;
            public Vector3 velocity;
            public Vector3 direction;
            public float roll;
            public Vector3 angularVelocity;
        }

        public struct BoidTarget
        {
            public Vector3 direction;
            public float speed;
        }

        public BoidState state;
        public BoidTarget target;
    }

    internal class DataInstanceMap
    {
        public readonly Dictionary<int, DataBlob> blobs = new Dictionary<int, DataBlob>();

        public void RecordData(int id, BoidState data)
        {
            DataBlob.BoidState state = new DataBlob.BoidState();
            state.position = data.position;
            state.velocity = data.velocity;
            state.direction = data.direction;
            state.roll = data.roll;
            state.angularVelocity = data.angularVelocity;

            DataBlob blob = GetDataBlob(id);
            blob.state = state;
            blobs[id] = blob;
        }

        public void RecordData(int id, BoidTarget data)
        {
            DataBlob.BoidTarget target = new DataBlob.BoidTarget();
            target.direction = data.direction;
            target.speed = data.speed;

            DataBlob blob = GetDataBlob(id);
            blob.target = target;
            blobs[id] = blob;
        }

        private DataBlob GetDataBlob(int id)
        {
            if (!blobs.TryGetValue(id, out DataBlob blob))
            {
                blob = new DataBlob();
                blobs.Add(id, blob);
            }
            return blob;
        }
    }

    // public class DataInstanceMap<Data, Store>
    // {
    //     private readonly Dictionary<int, Store> blobs = new Dictionary<int, Store>();

    //     public void RecordData(int id, Data data)
    //     {
    //         blobs.Add(id, )
    //     }
    // }

    public class Keyframe
    {
        public double timestamp;

        internal readonly DataInstanceMap data = new DataInstanceMap();

        public Keyframe()
        {
        }
    }
}
