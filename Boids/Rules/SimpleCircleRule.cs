// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    [CreateAssetMenu(fileName = "SimpleCircleRule", menuName = "Boids/SimpleCircleRule", order = 1)]
    public class SimpleCircleRule : BoidRule
    {
        public float radius = 1.0f;
        public Vector3 center = Vector3.zero;

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            Vector3 localPos = state.position - center;
            Vector3 goal = new Vector3(localPos.x, 0.0f, localPos.z);
            goal = goal.normalized * radius + center;

            target = new BoidTarget(goal - state.position, boid.Settings.MaxSpeed);
            priority = PriorityLow;
            return true;
        }
    }
}
