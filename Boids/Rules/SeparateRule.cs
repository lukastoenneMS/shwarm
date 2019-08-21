// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    [CreateAssetMenu(fileName = "SeparateRule", menuName = "Boids/SeparateRule", order = 1)]
    public class SeparateRule : BoidRule
    {
        private readonly List<int> queryResults = new List<int>();

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            BoidSettings settings = boid.Settings;

            queryResults.Clear();
            context.Query.Radius(context.Tree, state.position, settings.SeparationDistance, queryResults);

            int count = 0;
            Vector3 steer = Vector3.zero;
            foreach (int idx in queryResults)
            {
                // Skip own point
                if (idx == boidIndex)
                {
                    continue;
                }

                BoidState qstate = context.States[idx];
                Vector3 delta = state.position - qstate.position;
                float sqrDist = delta.sqrMagnitude;
                if (sqrDist > 0.0f)
                {
                    float weight = 1.0f / sqrDist;

                    steer += delta * weight;
                    ++count;
                }
            }

            if (count > 0)
            {
                float speed = settings.MaxSpeed;
                target = new BoidTarget(steer.normalized, speed);
                priority = PriorityHigh;
                return true;
            }

            target = null;
            priority = PriorityNone;
            return false;
        }
    }
}
