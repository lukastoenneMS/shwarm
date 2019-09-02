// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.Boids;

namespace Shwarm.Vdb
{
    public class VisualRecorder : Singleton<VisualRecorder>
    {
        private Keyframe keyframe;

        public void BeginKeyframe()
        {
            keyframe = new Keyframe();
        }

        public int CommitKeyframe()
        {
            VisualDebugger.Instance.AddKeyframe(keyframe);
            keyframe = null;
            return VisualDebugger.Instance.NumKeyframes - 1;
        }

        public void DiscardKeyframe()
        {
            keyframe = null;
        }

        public void RecordBoidState(int id, BoidState state)
        {
            if (keyframe != null)
            {
                VdbBoidState vdbState = new VdbBoidState();
                vdbState.position = state.position;
                vdbState.velocity = state.velocity;
                vdbState.direction = state.direction;
                vdbState.roll = state.roll;
                vdbState.angularVelocity = state.angularVelocity;

                var data = keyframe.GetOrCreateData<VdbBoidStateKeyframe>();
                data.Store(id, vdbState);
            }
        }

        public void RecordBoidTarget(int id, BoidTarget target)
        {
            if (keyframe != null)
            {
                VdbBoidTarget vdbTarget = new VdbBoidTarget();
                vdbTarget.direction = target.direction;
                vdbTarget.speed = target.speed;

                var data = keyframe.GetOrCreateData<VdbBoidTargetKeyframe>();
                data.Store(id, vdbTarget);
            }
        }

        public void RecordGrid(Grid.Grid<float> grid)
        {
            if (keyframe != null)
            {
                var data = keyframe.GetOrCreateData<VdbBoidGridKeyframe>();
                data.grid = grid.Copy();
            }
        }
    }
}
