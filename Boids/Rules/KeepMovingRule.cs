// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Boids
{
    /// <summary>
    /// Rule to keep moving in the current direction at minimum speed.
    /// </summary>
    [CreateAssetMenu(fileName = "KeepMovingRule", menuName = "Boids/KeepMovingRule", order = 1)]
    public class KeepMovingRule : BoidRule
    {
        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            if (state.direction == Vector3.zero)
            {
                target = null;
                priority = PriorityNone;
                return false;
            }

            target = new BoidTarget(state.direction, boid.Settings.MinSpeed);
            priority = PriorityLow;
            return true;
        }
    }
}
