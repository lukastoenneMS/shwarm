// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shwarm.Boids
{
    [CreateAssetMenu(fileName = "AvoidBoidCollisionRule", menuName = "Boids/AvoidBoidCollisionRule", order = 1)]
    public class AvoidBoidCollisionRule : BoidRule
    {
        /// <summary>
        /// Maximum collision detection distance.
        /// </summary>
        public float maxRadius = 1.0f;

        /// <summary>
        /// Minimum distance allowed between particles.
        /// </summary>
        public float minRadius = 0.1f;

        /// <summary>
        /// Maximum number of iterations per step to try and find a non-colliding direction.
        /// </summary>
        public int maxIterations = 5;

        private readonly List<int> queryResults = new List<int>();

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            float deltaRadius = maxRadius - minRadius;
            if (deltaRadius <= 0.0f)
            {
                target = null;
                priority = PriorityNone;
                return false;
            }

            queryResults.Clear();
            context.Query.Radius(context.Tree, state.position, maxRadius, queryResults);

            Vector3 dir = state.direction;
            bool hasCorrection = false;
            float weight = 0.0f;
            for (int iter = 0; iter < maxIterations; ++iter)
            {
                int numCollisions = 0;
                float distance = 0.0f;
                float maxDistance = 0.0f;
                Vector3 gradient = Vector3.zero;
                foreach (int idx in queryResults)
                {
                    // Skip own point
                    if (idx == boidIndex)
                    {
                        continue;
                    }

                    Vector3 colliderPos = context.Tree.Points[idx];
                    Vector3 colliderDir = colliderPos - state.position;

                    if (BoidUtils.GetInsidePositiveConeDistance(dir, colliderDir, minRadius, out float coneDistance, out Vector3 coneGradient))
                    {
                        // TODO find useful metric for correction weight
                        ++numCollisions;
                        distance += coneDistance;
                        maxDistance = Mathf.Max(maxDistance, coneDistance);
                        gradient += coneGradient;
                    }
                }
                if (numCollisions > 0)
                {
                    hasCorrection = true;
                    dir -= gradient * distance;
                    weight = maxDistance;
                }
                else
                {
                    break;
                }
            }

            if (hasCorrection)
            {
                target = new BoidTarget(dir, state.velocity.magnitude);
                priority = PriorityHigh * weight;
                return true;
            }
            else
            {
                target = null;
                priority = PriorityNone;
                return false;
            }
        }
    }
}
