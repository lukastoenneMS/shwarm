// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using Shwarm.Conversions;
using Shwarm.MathUtils;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;

using Physics = UnityEngine.Physics;
using Vector3 = UnityEngine.Vector3;
using RaycastHit = UnityEngine.RaycastHit;
using QueryTriggerInteraction = UnityEngine.QueryTriggerInteraction;

namespace Shwarm.Boids
{
    public class BoidState
    {
        public int instanceID;

        public Vector3 position;
        public Vector3 velocity;
        public Vector3 direction;
        public float roll;
        public Vector3 angularVelocity;

        public bool allowRaycast;
        public RaycastHit hitInfo;

        public BoidState(BoidParticle boid)
        {
            instanceID = boid.GetInstanceID();
            boid.GetPhysicsState(this);
        }
    }

    public class BoidContext
    {
        private readonly BoidSettings settings;
        public BoidSettings Settings => settings;

        private const int maxPointsPerLeafNode = 32;

        private KDTree tree;
        public KDTree Tree => tree;
        private KDQuery query;
        public KDQuery Query => query;

        private readonly Grid.Grid<float> testGrid;
        public Grid.Grid<float> TestGrid => testGrid;

        private BoidState[] states = new BoidState[0];
        public BoidState[] States => states;

        private BoidParticle[] boids = new BoidParticle[0];
        public BoidParticle[] Boids => boids;

        public BoidContext(BoidSettings settings)
        {
            this.settings = settings;

            int maxPointsPerLeafNode = 32;
            tree = new KDTree(maxPointsPerLeafNode);
            query = new KDQuery();

            float cellSize = settings.InteractionRadius * 2.0f;
            var xform = new Transform(float3.One * cellSize, float3.Zero);
            testGrid = new Grid.Grid<float>();
            testGrid.Transform = xform;
        }

        public void UpdateBoidParticles(BoidParticle[] newBoids)
        {
            boids = newBoids;

            var compareInstanceIDs = new Comparison<BoidParticle>((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
            Array.Sort(boids, compareInstanceIDs);

            BoidState[] oldStates = states;
            states = new BoidState[boids.Length];

            int oldIdx = 0;
            for (int i = 0; i < boids.Length; ++i)
            {
                BoidParticle boid = boids[i];
                int instanceID = boid.GetInstanceID();

                // Discard states without boid particle
                while (oldIdx < oldStates.Length && instanceID > oldStates[oldIdx].instanceID)
                {
                    ++oldIdx;
                }

                if (oldIdx < oldStates.Length && instanceID == oldStates[oldIdx].instanceID)
                {
                    // Copy existing states for matching particles
                    states[i] = oldStates[oldIdx];
                }
                else
                {
                    // Add new states for boid particles without state
                    states[i] = new BoidState(boid);
                }
            }
        }

        public void Prepare()
        {
            int numBoids = boids.Length;

            var xform = testGrid.Transform;

            NativeArray<float3> worldPoints = new NativeArray<float3>(numBoids, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < numBoids; ++i)
            {
                boids[i].GetPhysicsState(states[i]);
                worldPoints[i] = states[i].position.ToFloat3();
            }

            { /// TEST
                Grid.Tree<int> indexTree = new Grid.Tree<int>();
                testGrid.Tree.Clear();
                float3[] simplePoints = new float3[numBoids];
                worldPoints.CopyTo(simplePoints);
                Grid.PointCloudConverter.Convert(simplePoints, testGrid.Tree, indexTree, testGrid.Transform);
                BoidDebug.SetGrid(testGrid);
            } /// TEST

            int batchCount = 64;

            // Transforms world-space points into grid coordinates
            var pointToIndexJob = new Grid.PointToGridJob(worldPoints, xform);
            JobHandle pointToIndexJobHandle = pointToIndexJob.Schedule(numBoids, batchCount);
            var blockIndices = pointToIndexJob.BlockIndices;

            // Count the number of points in each block, in preparation for allocating sorting bins
            var pointPartitionCountJob = new Grid.PointPartitionCountJob(blockIndices);
            JobHandle pointPartitionCountJobHandle = pointPartitionCountJob.Schedule(pointToIndexJobHandle);
            var blockCounts = pointPartitionCountJob.BlockCounts;
            var blockOffsets = pointPartitionCountJob.BlockOffsets;

            // Radix-sort the points into per-block bins, in a combined output list
            // var pointPartitionJob = new Grid.PointPartitionJob(blockIndices, blockOffsets);
            // JobHandle pointPartitionJobHandle = pointPartitionJob.Schedule(numBoids, batchCount, pointPartitionCountJobHandle);
            // var pointIndices = pointPartitionJob.PointIndices;

            NativeHashMap<BlockIndex, Grid.TestContainer> blockPoints;
            {// REMOVE! should be allocated by pointPartitionCountJob
                pointPartitionCountJobHandle.Complete();
                var blocks = blockCounts.GetKeyArray(Allocator.Temp);
                var counts = blockCounts.GetValueArray(Allocator.Temp);
                blockPoints = new NativeHashMap<BlockIndex, Grid.TestContainer>(blocks.Length, Allocator.Temp);
                for (int i = 0; i < blocks.Length; ++i)
                {
                    blockPoints.TryAdd(blocks[i], new Grid.TestContainer(counts[i], Allocator.TempJob));
                }
                blocks.Dispose();
                counts.Dispose();
            }

            var pointPartitionJob = new Grid.PointPartitionJob2(blockIndices, blockPoints);
            JobHandle pointPartitionJobHandle = pointPartitionJob.Schedule(numBoids, batchCount, pointPartitionCountJobHandle);

            pointPartitionJobHandle.Complete();

            // for (int i = 0; i < numBoids; ++i)
            // {
            //     float3 pw = worldPoints[i];
            //     float3 pi = gridPoints[i];
            //     UnityEngine.Debug.Log($"({pw.x:F3}, {pw.y:F3}, {pw.z:F3}) -> ({pi.x:F3}, {pi.y:F3}, {pi.z:F3})");
            // }

            // {
            //     var blocks = blockCounts.GetKeyArray(Allocator.Temp);
            //     var counts = blockCounts.GetValueArray(Allocator.Temp);
            //     UnityEngine.Debug.Log($"Blocks: " + string.Join(" ", Enumerable.Range(0, blocks.Length).Select(i => $"[{blocks[i]}]:{counts[i]}")));
            //     blocks.Dispose();
            //     counts.Dispose();
            // }

            {
                var blocks = blockPoints.GetKeyArray(Allocator.Temp);
                var lists = blockPoints.GetValueArray(Allocator.Temp);
                UnityEngine.Debug.Log(
                    $"Blocks: " + string.Join("\n", Enumerable.Range(0, blocks.Length).Select(
                        i => $"{blocks[i]}: {string.Join(" ", lists[i].points)}")
                    )
                );
                blocks.Dispose();
                lists.Dispose();
            }

            worldPoints.Dispose();
            pointToIndexJob.DisposeResults();
            pointPartitionCountJob.DisposeResults();
            pointPartitionJob.DisposeResults();

            tree.SetCount(numBoids);
            for (int i = 0; i < numBoids; ++i)
            {
                boids[i].GetPhysicsState(states[i]);
                tree.Points[i] = states[i].position;
            }
            tree.Rebuild();

            // Enable raycast based on priority queue and budget
            for (int i = 0; i < numBoids; ++i)
            {
                states[i].allowRaycast = false;
            }
            // TODO implement priority queue
            int raycastBudget = 10;
            System.Random rand = new System.Random();
            for (int i = 0; i < raycastBudget; ++i)
            {
                int r = rand.Next() % numBoids;
                states[r].allowRaycast = true;
            }
        }

        public void Cleanup()
        {
        }

        public bool RequestRayCast(BoidState state, Vector3 origin, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            if (state.allowRaycast)
            {
                Physics.Raycast(origin, direction, out state.hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }
            return state.hitInfo.collider != null;
        }

        public bool RequestSphereCast(BoidState state, Vector3 origin, float radius, Vector3 direction, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            if (state.allowRaycast)
            {
                Physics.SphereCast(origin, radius, direction, out state.hitInfo, maxDistance, layerMask, queryTriggerInteraction);
            }
            return state.hitInfo.collider != null;
        }
    }
}
