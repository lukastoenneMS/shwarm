// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;
using Unity.Collections;

using IJob = Unity.Jobs.IJob;
using IJobParallelFor = Unity.Jobs.IJobParallelFor;

namespace Shwarm.Grid
{
    /// <summary>
    /// Transforms a list of points into grid index space.
    /// </summary>
    public struct PointToGridJob : IJobParallelFor
    {
        [ReadOnly]
        private NativeArray<float3> worldPoints;

        [WriteOnly]
        private NativeArray<float3> gridPoints;
        public NativeArray<float3> GridPoints => gridPoints;

        [WriteOnly]
        private NativeArray<GridIndex> gridIndices;
        public NativeArray<GridIndex> GridIndices => gridIndices;

        [WriteOnly]
        private NativeArray<BlockIndex> blockIndices;
        public NativeArray<BlockIndex> BlockIndices => blockIndices;

        [WriteOnly]
        private NativeArray<int> cellIndices;
        public NativeArray<int> CellIndices => cellIndices;

        private Transform transform;

        public PointToGridJob(NativeArray<float3> worldPoints, Transform transform)
        {
            this.worldPoints = worldPoints;
            this.transform = transform;

            int count = worldPoints.Length;
            this.gridPoints = new NativeArray<float3>(count, Unity.Collections.Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            this.gridIndices = new NativeArray<GridIndex>(count, Unity.Collections.Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            this.blockIndices = new NativeArray<BlockIndex>(count, Unity.Collections.Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            this.cellIndices = new NativeArray<int>(count, Unity.Collections.Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        }

        public void Execute(int i)
        {
            transform.InverseTransformCorner(worldPoints[i], out float3 gridPoint);
            gridPoints[i] = gridPoint;
            GridIndex gridIndex = new GridIndex(gridPoint);
            gridIndices[i] = gridIndex;
            blockIndices[i] = IndexDetails.GridToBlockIndex(gridIndex, out GridIndex localIndex);
            cellIndices[i] = IndexDetails.LocalToCellIndex(localIndex);
        }

        public void DisposeResults()
        {
            gridPoints.Dispose();
            gridIndices.Dispose();
            blockIndices.Dispose();
            cellIndices.Dispose();
        }
    }

    /// <summary>
    /// Count the number of points in each block.
    /// </summary>
    public struct PointPartitionCountJob : IJob
    {
        [ReadOnly]
        private NativeArray<BlockIndex> blockIndices;

        private NativeHashMap<BlockIndex, int> blockCounts;
        public NativeHashMap<BlockIndex, int> BlockCounts => blockCounts;

        [WriteOnly]
        private NativeHashMap<BlockIndex, int> blockOffsets;
        public NativeHashMap<BlockIndex, int> BlockOffsets => blockOffsets;

        public PointPartitionCountJob(NativeArray<BlockIndex> blockIndices)
        {
            this.blockIndices = blockIndices;
            this.blockCounts = new NativeHashMap<BlockIndex, int>(64, Allocator.TempJob);
            this.blockOffsets = new NativeHashMap<BlockIndex, int>(64, Allocator.TempJob);
        }

        public void Execute()
        {
            int N = blockIndices.Length;
            for (int i = 0; i < N; ++i)
            {
                BlockIndex blockIndex = blockIndices[i];

                if (blockCounts.ContainsKey(blockIndex))
                {
                    blockCounts[blockIndex] += 1;
                }
                else
                {
                    blockCounts[blockIndex] = 1;
                }
            }

            var blocks = blockCounts.GetKeyArray(Allocator.Temp);
            var counts = blockCounts.GetValueArray(Allocator.Temp);
            int offset = 0;
            for (int i = 0; i < blocks.Length; ++i)
            {
                blockOffsets[blocks[i]] = offset;
                offset += counts[i];
            }
            blocks.Dispose();
            counts.Dispose();
        }

        public void DisposeResults()
        {
            blockCounts.Dispose();
            blockOffsets.Dispose();
        }
    }

    /// <summary>
    /// This operator sorts points into per-block bins based on grid-space positions.
    /// </summary>
    public struct PointPartitionJob : IJobParallelFor
    {
        [ReadOnly]
        private NativeArray<BlockIndex> blockIndices;

        [ReadOnly]
        private NativeHashMap<BlockIndex, int> blockOffsets;

        [WriteOnly]
        private NativeArray<int> pointIndices;
        public NativeArray<int> PointIndices => pointIndices;

        private NativeHashMap<BlockIndex, NativeArray<int>> pointIndicesAlt;
        public NativeHashMap<BlockIndex, NativeArray<int>> PointIndicesAlt => pointIndicesAlt;

        private NativeHashMap<BlockIndex, int> blockNext;

        public PointPartitionJob(NativeArray<BlockIndex> blockIndices, NativeHashMap<BlockIndex, int> blockOffsets)
        {
            this.blockIndices = blockIndices;
            this.blockOffsets = blockOffsets;
            this.pointIndices = new NativeArray<int>(blockIndices.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            this.pointIndicesAlt = new NativeHashMap<BlockIndex, NativeArray<int>>(blockOffsets.Length, Allocator.TempJob);
            this.blockNext = new NativeHashMap<BlockIndex, int>(blockOffsets.Length, Allocator.TempJob);
        }

        public void Execute(int i)
        {
            BlockIndex blockIndex = blockIndices[i];
            if (!blockNext.TryGetValue(blockIndex, out int next))
            {
                next = blockOffsets[blockIndex];
            }
            if (pointIndicesAlt.TryGetValue(blockIndex, out NativeArray<int> blockPoints))
            {
                blockPoints[0] = 3;
            }

            pointIndices[next] = i;

            blockNext[blockIndex] = next + 1;
        }
    }

    public struct TestContainer
    {
        public TestContainer(int length, Allocator allocator)
        {
            points = new NativeArray<int>(length, allocator, NativeArrayOptions.UninitializedMemory);
        }

        public NativeArray<int> points;
    }

    /// <summary>
    /// This operator sorts points into per-block bins based on grid-space positions.
    /// </summary>
    public struct PointPartitionJob2 : IJobParallelFor
    {
        [ReadOnly]
        private NativeArray<BlockIndex> blockIndices;

        private NativeHashMap<BlockIndex, TestContainer> blockPoints;

        private NativeHashMap<BlockIndex, int> blockNext;

        public PointPartitionJob2(NativeArray<BlockIndex> blockIndices, NativeHashMap<BlockIndex, TestContainer> blockPoints)
        {
            this.blockIndices = blockIndices;
            this.blockPoints = blockPoints;

            var blocks = blockPoints.GetKeyArray(Allocator.TempJob);
            int numBlocks = blockPoints.Length;
            this.blockNext = new NativeHashMap<BlockIndex, int>(numBlocks, Allocator.TempJob);

            for (int i = 0; i < numBlocks; ++i)
            {
                this.blockNext.TryAdd(blocks[i], 0);
            }

            blocks.Dispose();
        }

        public void Execute(int i)
        {
            BlockIndex blockIndex = blockIndices[i];

            int next = blockNext[blockIndex];
            var blockList = blockPoints[blockIndex];
            blockList.points[next] = i;

            blockNext[blockIndex] = next + 1;
        }

        public void DisposeResults()
        {
            blockNext.Dispose();
        }
    }

    public class PointPartitioner
    {

    }
}
