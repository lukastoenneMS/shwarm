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
    public struct PointPartitionJob : IJobParallelFor
    {
        [ReadOnly]
        private NativeArray<float3> points;

        private NativeHashMap<BlockIndex, NativeList<int>> indexListMap;
        /// <summary>
        /// Map of per-block index lists, containing indices of all points within a block.
        /// </summary>
        public NativeHashMap<BlockIndex, NativeList<int>> IndexListMap => indexListMap;

        private readonly int bucketLog2Dim;
        private readonly int bucketMask;

        public PointPartitionJob(NativeArray<float3> points, int bucketLog2Dim)
        {
            this.points = points;
            this.indexListMap = new NativeHashMap<BlockIndex, NativeList<int>>();
            this.bucketLog2Dim = bucketLog2Dim;
            this.bucketMask = (1 << bucketLog2Dim) - 1;
        }

        public void Execute(int i)
        {
            float3 p = points[i];
            GridIndex gridIndex = new GridIndex(p);
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(gridIndex);

            if (!indexListMap.TryGetValue(blockIndex, out NativeList<int> indexList))
            {
                indexList = new NativeList<int>(Allocator.TempJob);
                indexListMap.TryAdd(blockIndex, indexList);
            }

            indexList.Add(i);
        }
    }

    public class PointPartitioner
    {

    }
}
