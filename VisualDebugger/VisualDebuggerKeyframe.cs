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

        public BoidState boid;
    }

    internal class DataInstanceMap
    {
        public readonly Dictionary<int, DataBlob> blobs = new Dictionary<int, DataBlob>();

        public void RecordData(int id, BoidState data)
        {
            DataBlob blob = new DataBlob();
            blob.boid.position = data.position;
            blob.boid.velocity = data.velocity;
            blob.boid.direction = data.direction;
            blob.boid.roll = data.roll;
            blob.boid.angularVelocity = data.angularVelocity;

            blobs[id] = blob;
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
