// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Shwarm.Boids
{
    [CreateAssetMenu(fileName = "AvoidCollisionRule", menuName = "Boids/AvoidCollisionRule", order = 1)]
    public class AvoidCollisionRule : BoidRule
    {
        /// <summary>
        /// Maximum distance at which collisions will be detected.
        /// </summary>
        public float DetectionDistance = 1.0f;

        /// <summary>
        /// Layers to include in collision detection.
        /// </summary>
        public LayerMask Layers = UnityEngine.Physics.DefaultRaycastLayers;

        private Vector3 randomDir = Vector3.zero;
        private float lastRandomDirTime = 0.0f;
        private const float randomDirInterval = 1.5f;

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            BoidSettings settings = boid.Settings;
            int count = 0;
            Vector3 steer = Vector3.zero;

            if (context.RequestSphereCast(state, state.position, settings.SeparationDistance, state.direction, DetectionDistance, Layers, QueryTriggerInteraction.Ignore))
            {
                if (GetAvoidanceSteering(boid, state, out Vector3 collSteer))
                {
                    BoidDebug.AddCollisionPoint(boid, state, state.hitInfo.point, collSteer);
                    steer += collSteer;
                    ++count;
                }
            }
            else if (context.RequestRayCast(state, state.position, state.direction, DetectionDistance, Layers, QueryTriggerInteraction.Ignore))
            {
                if (GetAvoidanceSteering(boid, state, out Vector3 collSteer))
                {
                    BoidDebug.AddCollisionPoint(boid, state, state.hitInfo.point, collSteer);
                    steer += collSteer;
                    ++count;
                }
            }

            if (count > 0)
            {
                float speed = settings.MaxSpeed;
                target = new BoidTarget(steer.normalized, speed);
                priority = PriorityMedium;
                return true;
            }

            target = null;
            priority = PriorityNone;
            return false;
        }

        private bool GetAvoidanceSteering(BoidParticle boid, BoidState state, out Vector3 steer)
        {
            if (boid.EnableDebugObjects)
            {

            }
            if (state.hitInfo.collider != null && state.hitInfo.distance > 0.0f)
            {
                Vector3 delta = state.position - state.hitInfo.point;
                float sqrDist = delta.sqrMagnitude;
                if (sqrDist > 0.0f)
                {
                    float weight = boid.Settings.SeparationDistance / sqrDist;
                    // steer = delta.normalized;
                    steer = Vector3.ProjectOnPlane(delta, state.direction).normalized;
                    if (steer == Vector3.zero)
                    {
                        steer = Vector3.ProjectOnPlane(GetRandomDirection(), state.direction).normalized;
                    }

                    steer *= weight;
                    return true;
                }
            }

            steer = Vector3.zero;
            return false;
        }

        private Vector3 GetRandomDirection()
        {
            // Try to escape by chosing a random direction
            if (Time.time >= lastRandomDirTime + randomDirInterval)
            {
                randomDir = UnityEngine.Random.onUnitSphere;
                lastRandomDirTime = Time.time;
            }

            return randomDir;
        }
    }
}
