// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shwarm.Boids
{
    [CreateAssetMenu(fileName = "CohesionRule", menuName = "Boids/CohesionRule", order = 1)]
    public class CohesionRule : BoidRule
    {
        private readonly List<int> queryResults = new List<int>();

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            BoidSettings settings = boid.Settings;

            queryResults.Clear();
            context.Query.Radius(context.Tree, state.position, settings.NeighborDistance, queryResults);

            int count = 0;
            Vector3 center = Vector3.zero;
            foreach (int idx in queryResults)
            {
                // Skip own point
                if (idx == boidIndex)
                {
                    continue;
                }

                BoidState qstate = context.States[idx];
                center += qstate.position;
                ++count;
            }

            if (count > 0)
            {
                center /= (float)count;
                float speed = settings.MaxSpeed;
                target = new BoidTarget((center - state.position).normalized, speed);
                priority = PriorityMedium;
                return true;
            }

            target = null;
            priority = PriorityNone;
            return false;
        }
    }
}
