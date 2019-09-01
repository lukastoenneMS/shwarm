// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using Shwarm.Vdb;

namespace Boids
{
    public static class BoidDebug
    {
        private static VisualRecorder recorder = VisualRecorder.Instance;

        public static void SetTarget(BoidParticle particle, BoidState state, BoidTarget target)
        {
            recorder.RecordData(state.instanceID, target);
        }

        public static void SetPhysics(BoidParticle particle, BoidState state, Vector3 force, Vector3 torque)
        {
            recorder.RecordData(state.instanceID, state);
        }

        public static void AddSwarmPoint(BoidParticle particle, BoidState state, Vector3 point, float weight)
        {
        }

        public static void AddCollisionPoint(BoidParticle particle, BoidState state, Vector3 hitPoint, Vector3 hitNormal)
        {
        }

        public static void AddBoidCollisionCone(BoidParticle particle, BoidState state, Vector3 dir, Vector3 colliderDir, float radius)
        {
        }
    }
}
