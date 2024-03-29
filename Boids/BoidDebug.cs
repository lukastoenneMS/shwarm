// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using Shwarm.Vdb;

namespace Shwarm.Boids
{
    public static class BoidDebug
    {
        private static VisualRecorder recorder = VisualRecorder.Instance;

        public static void SetTarget(BoidParticle particle, BoidState state, BoidTarget target)
        {
            recorder.RecordBoidTarget(state.instanceID, target);
        }

        public static void SetPhysics(BoidParticle particle, BoidState state, Vector3 force, Vector3 torque)
        {
            recorder.RecordBoidState(state.instanceID, state);
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

        public static void SetGrid(Grid.Grid<float> grid)
        {
            recorder.RecordGrid(grid);
        }
    }

    public static class VdbExtensions
    {
        public static void RecordBoidState(this VisualRecorder rec, int id, BoidState state)
        {
            if (rec.Keyframe != null)
            {
                VdbBoidState vdbState = new VdbBoidState();
                vdbState.position = state.position;
                vdbState.velocity = state.velocity;
                vdbState.direction = state.direction;
                vdbState.roll = state.roll;
                vdbState.angularVelocity = state.angularVelocity;

                var data = rec.Keyframe.GetOrCreateData<VdbBoidStateKeyframe>();
                data.Store(id, vdbState);
            }
        }

        public static void RecordBoidTarget(this VisualRecorder rec, int id, BoidTarget target)
        {
            if (rec.Keyframe != null)
            {
                VdbBoidTarget vdbTarget = new VdbBoidTarget();
                vdbTarget.direction = target.direction;
                vdbTarget.speed = target.speed;

                var data = rec.Keyframe.GetOrCreateData<VdbBoidTargetKeyframe>();
                data.Store(id, vdbTarget);
            }
        }

        public static void RecordGrid(this VisualRecorder rec, Grid.Grid<float> grid)
        {
            if (rec.Keyframe != null)
            {
                var data = rec.Keyframe.GetOrCreateData<VdbBoidGridKeyframe>();
                data.grid = grid.Copy();
            }
        }
    }
}
