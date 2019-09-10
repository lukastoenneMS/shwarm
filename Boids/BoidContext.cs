// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using Shwarm.Conversions;
using Shwarm.MathUtils;
using System;
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

            float cellSize = settings.InteractionRadius;
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


            NativeArray<float3> points = new NativeArray<float3>(numBoids, Unity.Collections.Allocator.Persistent);
            for (int i = 0; i < numBoids; ++i)
            {
                boids[i].GetPhysicsState(states[i]);
                points[i] = states[i].position.ToFloat3();
            }

            { /// TEST
                Grid.Tree<int> indexTree = new Grid.Tree<int>();
                testGrid.Tree.Clear();
                float3[] simplePoints = new float3[points.Length];
                points.CopyTo(simplePoints);
                Grid.PointCloudConverter.Convert(simplePoints, testGrid.Tree, indexTree, testGrid.Transform);
                BoidDebug.SetGrid(testGrid);
            } /// TEST

            var pointToIndexJob = new Grid.PointToIndexCornerJob(points, xform);

            JobHandle pointToIndexJobHandle = pointToIndexJob.Schedule(points.Length, 64);
            pointToIndexJobHandle.Complete();

            var result = pointToIndexJob.Indices;
            for (int i = 0; i < numBoids; ++i)
            {
                float3 pw = points[i];
                float3 pi = result[i];
                // UnityEngine.Debug.Log($"({pw.x:F3}, {pw.y:F3}, {pw.z:F3}) -> ({pi.x:F3}, {pi.y:F3}, {pi.z:F3})");
            }

            points.Dispose();
            result.Dispose();

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
