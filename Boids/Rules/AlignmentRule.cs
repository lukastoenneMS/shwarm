// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shwarm.Boids
{
    [CreateAssetMenu(fileName = "AlignmentRule", menuName = "Boids/AlignmentRule", order = 1)]
    public class AlignmentRule : BoidRule
    {
        private readonly List<int> queryResults = new List<int>();

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            BoidSettings settings = boid.Settings;

            queryResults.Clear();
            context.Query.Radius(context.Tree, state.position, settings.NeighborDistance, queryResults);

            int count = 0;
            Vector3 direction = Vector3.zero;
            float speed = 0.0f;
            foreach (int idx in queryResults)
            {
                // Skip own point
                if (idx == boidIndex)
                {
                    continue;
                }

                BoidState qstate = context.States[idx];
                direction += qstate.direction;
                speed += Vector3.Dot(qstate.velocity, qstate.direction);
                ++count;
            }

            if (count > 0)
            {
                speed /= count;
                target = new BoidTarget(direction.normalized, speed);
                priority = PriorityMedium;
                return true;
            }

            target = null;
            priority = PriorityNone;
            return false;
        }
    }
}
