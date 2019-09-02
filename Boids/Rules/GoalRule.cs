// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Shwarm.Boids
{
    [CreateAssetMenu(fileName = "GoalRule", menuName = "Boids/GoalRule", order = 1)]
    public class GoalRule : BoidRule
    {
        public GameObject goal = null;
        public Vector3 goalVector = Vector3.zero;

        /// <summary>
        /// Distance to keep from the goal position.
        /// </summary>
        public float Distance = 1.0f;

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            Vector3 currentGoal = goalVector;
            if (goal)
            {
                currentGoal = goal.transform.position;
            }

            Vector3 delta = currentGoal - state.position;
            Vector3 direction = (delta - delta.normalized * Distance).normalized;

            target = new BoidTarget(direction, boid.Settings.MinSpeed);
            priority = PriorityMedium;
            return true;
        }
    }
}
