// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace Boids
{
    [System.Serializable]
    public class BoidSettings
    {
        public float MinSpeed = 0.0f;
        public float MaxSpeed = 10.0f;
        public float MaxAcceleration = 0.5f;
        public float MaxBackwardAcceleration = 0.1f;
        public float MaxAngularVelocity = 90.0f;
        public float MaxAngularAcceleration = 10.0f;

        /// <summary>
        /// Consider boids as neighbors when they are closer.
        /// </summary>
        public float NeighborDistance = 1.0f;

        /// <summary>
        /// Distance that should be kept between boids.
        /// </summary>
        public float SeparationDistance = 0.5f;

        public float Banking = 1.0f;
        public float Pitch = 1.0f;
    }

    [System.Serializable]
    public class BoidParticle : MonoBehaviour
    {
        [SerializeField]
        private BoidSettings settings = new BoidSettings();
        public BoidSettings Settings => settings;

        public int CurrentRuleIndex = -1;

        public bool EnableDebugObjects = false;

        public void GetPhysicsState(BoidState state)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            Assert.IsNotNull(rb);

            state.position = rb.position;
            state.velocity = rb.velocity;
            state.direction = rb.transform.forward.normalized;
            state.roll = (rb.rotation.eulerAngles.z + 180.0f) % 360.0f - 180.0f;
            state.angularVelocity = rb.angularVelocity;
        }

        public void ApplyPhysics(BoidState state, BoidTarget target)
        {
            float dtime = Time.fixedDeltaTime;
            Quaternion uprightRotation = Quaternion.LookRotation(state.direction, Vector3.up);
            Quaternion stateRotation = uprightRotation * Quaternion.Euler(0.0f, 0.0f, state.roll);

            Vector3 predictedPosition = state.position + dtime * state.velocity;

            Vector3 targetForce = Vector3.zero;
            Quaternion targetRotation = uprightRotation;
            if (target != null)
            {
                Vector3 v = state.velocity;
                Vector3 dv = GetTargetVelocityChange(state, target);
                // Adjust velocity change to not exceed max. velocity
                targetForce = ClampedDelta(v, dv, settings.MaxSpeed);

                if (target.direction != Vector3.zero)
                {
                    targetRotation = Quaternion.LookRotation(target.direction, Vector3.up);
                }
            }

            // Limit pitch and roll to allowed range
            {
                Vector3 euler = targetRotation.eulerAngles;
                euler.x = Mathf.Clamp((euler.x + 180.0f) % 360.0f - 180.0f, -settings.Pitch, settings.Pitch);
                targetRotation = Quaternion.Euler(euler);
            }

            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(stateRotation);
            deltaRotation.ToAngleAxis(out float deltaAngle, out Vector3 deltaAxis);
            deltaAngle = (deltaAngle + 180.0f) % 360.0f - 180.0f;
            deltaAngle = Mathf.Clamp(deltaAngle, -settings.MaxAngularAcceleration * dtime, settings.MaxAngularAcceleration * dtime);

            Vector3 targetTorque = Vector3.zero;
            if (deltaAngle != 0.0f)
            {
                float targetAngVelChange = Mathf.Deg2Rad * deltaAngle / dtime;
                Vector3 angv = state.angularVelocity;
                Vector3 dangv = deltaAxis * targetAngVelChange;
                targetTorque = ClampedDelta(angv, dangv, Mathf.Deg2Rad * settings.MaxAngularVelocity);
            }

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddForce(targetForce, ForceMode.VelocityChange);
                rb.AddTorque(targetTorque, ForceMode.VelocityChange);
                BoidDebug.SetPhysics(this, state, targetForce, targetTorque);
            }
        }

        private Vector3 GetTargetVelocityChange(BoidState state, BoidTarget target)
        {
            float dtime = Time.fixedDeltaTime;
            Vector3 predictedPosition = state.position + dtime * state.velocity;

            Vector3 targetVelocityDelta = target.GetVelocity() - state.velocity;
            // Clamp velocity change to not exceed max. velocity
            targetVelocityDelta = ClampedDelta(state.velocity, targetVelocityDelta, settings.MaxSpeed);

            float projectedVelocity = Vector3.Dot(targetVelocityDelta, state.direction);
            float velocityChange = Mathf.Clamp(projectedVelocity, -settings.MaxBackwardAcceleration * dtime, settings.MaxAcceleration * dtime);

            return velocityChange * state.direction;
        }

        private static Vector3 ClampedDelta(Vector3 v, Vector3 dv, float max)
        {
            if ((v + dv).sqrMagnitude > max * max)
            {
                // Solves equation: ||v + lambda * dv|| = max
                // lambda clamped to [0, 1] to not accelerate backwards and not add more than desired velocity
                float v_v = Vector3.Dot(v, v);
                float dv_dv = Vector3.Dot(dv, dv);
                float v_dv = Vector3.Dot(v, dv);
                if (dv_dv > 0.0f)
                {
                    float dv_dv_inv = 1.0f / dv_dv;
                    float lambda = Mathf.Sqrt((max * max + v_dv * v_dv * dv_dv_inv) * dv_dv_inv) - v_dv * dv_dv_inv;
                    dv *= Mathf.Clamp(lambda, 0.0f, 1.0f);
                }
            }
            return dv;
        }
    }
}
