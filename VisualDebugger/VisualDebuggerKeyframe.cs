// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Boids;
using UnityEngine;
using System.Collections.Generic;

namespace Shwarm.Vdb
{
    public struct DataBlob
    {
        public Vector3 boidPosition;
    }

    public class DataInstanceMap
    {
        private readonly Dictionary<int, DataBlob> blobs = new Dictionary<int, DataBlob>();

        public void RecordData(int id, BoidState data)
        {
            DataBlob blob;
            blob.boidPosition = data.position;

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

        public readonly DataInstanceMap data = new DataInstanceMap();

        public Keyframe()
        {
        }
    }
}
