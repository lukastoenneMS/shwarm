// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;
using Unity.Collections;

using IJobParallelFor = Unity.Jobs.IJobParallelFor;

namespace Shwarm.Grid
{
    /// <summary>
    /// Transforms a list of points into index space.
    /// </summary>
    public struct PointToIndexCornerJob : IJobParallelFor
    {
        [ReadOnly]
        private NativeArray<float3> points;

        [WriteOnly]
        private NativeArray<float3> indices;
        public NativeArray<float3> Indices => indices;

        private Transform transform;

        public PointToIndexCornerJob(NativeArray<float3> points, Transform transform)
        {
            this.points = points;
            this.transform = transform;
            this.indices = new NativeArray<float3>(points.Length, Unity.Collections.Allocator.TempJob);
        }

        public void Execute(int i)
        {
            transform.InverseTransformCorner(points[i], out float3 index);
            indices[i] = index;
        }
    }

    /// <summary>
    /// This operator sorts points into per-block bins based on grid-space positions.
    /// </summary>
    public struct PointBinningJob : IJobParallelFor
    {
        [ReadOnly]
        private NativeArray<float3> points;

        private NativeHashMap<BlockIndex, NativeArray<int>> indexListMap;
        /// <summary>
        /// Map of per-block index lists, containing indices of all points within a block.
        /// </summary>
        public NativeHashMap<BlockIndex, NativeArray<int>> IndexListMap => indexListMap;

        private readonly int binLog2Dim;
        private readonly int binMask;

        public PointBinningJob(NativeArray<float3> points, int binLog2Dim)
        {
            this.points = points;
            this.indexListMap = new NativeHashMap<BlockIndex, NativeArray<int>>();
            this.binLog2Dim = binLog2Dim + IndexDetails.BlockLog2Dim;
            this.binMask = (1 << binLog2Dim) - 1;
        }

        public void Execute(int i)
        {
            
            // result[i] = a[i] + b[i];
        }
    }

    public class PointPartitioner
    {

    }
}
