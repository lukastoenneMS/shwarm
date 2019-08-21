// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    [CreateAssetMenu(fileName = "SwarmRule", menuName = "Boids/SwarmRule", order = 1)]
    public class SwarmRule : BoidRule
    {
        /// <summary>
        /// Maximum distance of other boids to influence the swarm searching behavior.
        /// </summary>
        public float maxRadius = 3.0f;

        /// <summary>
        /// Minimum distance to activate swarm behavior.
        /// </summary>
        public float minRadius = 1.0f;

        /// <summary>
        /// Weight preference of boids in the front vs. boids in the back
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float forwardAsymmetry = 0.5f;

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

            float totweight = 0.0f;
            Vector3 pos_mean = Vector3.zero;
            float pos_var = 0.0f;
            Vector3 dir_mean = Vector3.zero;
            float speed_mean = 0.0f;
            foreach (int idx in queryResults)
            {
                // Skip own point
                if (idx == boidIndex)
                {
                    continue;
                }

                BoidState queryState = context.States[idx];
                Vector3 delta = queryState.position - state.position;
                float weight = GetVisibility(state, queryState.position);

                totweight += weight;
                pos_mean += delta * weight;
                pos_var += delta.sqrMagnitude * weight;
                dir_mean += queryState.direction * weight;
                speed_mean += Vector3.Dot(queryState.velocity, queryState.direction) * weight;
            }

            if (totweight > 0.0f)
            {
                float norm = 1.0f / totweight;
                pos_mean *= norm;
                pos_var = pos_var * norm - pos_mean.sqrMagnitude;
                dir_mean *= norm;

                // Adjust direction when swimming against the flow
                // Otherwise gradually increase swarm behavior, i.e. swim to the center of the swarm
                float followFactor = Mathf.SmoothStep(1.0f, 0.0f, 0.5f + 0.5f * Vector3.Dot(state.direction, dir_mean.normalized));

                // TODO scale such that follow factor is 1 when variance gets smaller than threshold
                // float swarmSizeFactor = 1.0f / Mathf.Sqrt(2.0f * Mathf.PI * pos_var);
                // float swarmSizeFactor = 1.0f;
                // followFactor = Mathf.Min(swarmSizeFactor * Mathf.Exp(-0.5f * (pos_mean - state.position).sqrMagnitude / pos_var), 1.0f);

                Vector3 dir = (pos_mean - state.position).normalized * (1.0f - followFactor) + dir_mean.normalized * followFactor;
                // float speed = boid.Settings.MaxSpeed * (1.0f - followFactor) + speed_mean * followFactor;
                float speed = boid.Settings.MaxSpeed * (1.0f - followFactor);
                target = new BoidTarget(dir, speed);
                priority = PriorityMedium;
            }

            target = null;
            priority = PriorityNone;
            return false;
        }

        private float GetVisibility(BoidState state, Vector3 point)
        {
            // TODO arbitrary function to increase weight of forward targets
            Vector3 delta = point - state.position;
            float vis = 0.5f * 0.5f * Vector3.Dot(delta.normalized, state.direction);
            vis = 0.3f + 0.7f * vis;

            // Distance falloff
            if (minRadius < maxRadius)
            {
                vis *= Mathf.SmoothStep(1.0f, 0.0f, (delta.magnitude - minRadius) / (maxRadius - minRadius));
            }

            return vis;
        }
    }
}
