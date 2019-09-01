// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Shwarm.Vdb
{
    public struct VdbBoidState
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 direction;
        public float roll;
        public Vector3 angularVelocity;
    }
    public class VdbBoidStateKeyframe : InstanceKeyframeFeature<VdbBoidState> { }

    public struct VdbBoidTarget
    {
        public Vector3 direction;
        public float speed;
    }
    public class VdbBoidTargetKeyframe : InstanceKeyframeFeature<VdbBoidTarget> { }


    public class BoidIdsFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid IDs";

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            Keyframe keyframe = vdb.GetKeyframe(currentFrame);
            if (keyframe.TryGetData<VdbBoidStateKeyframe>(out var data))
            {
                foreach (var item in data)
                {
                    int id = item.Key;
                    VdbBoidState state = item.Value;
                    if (filter(id))
                    {
                        renderer.DrawText(state.position + new Vector3(0, 0.02f, 0.0f), id.ToString(), Color.white);
                    }
                }
            }
        }
    }

    public class BoidPositionsFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Positions";

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            Keyframe keyframe = vdb.GetKeyframe(currentFrame);
            if (keyframe.TryGetData<VdbBoidStateKeyframe>(out var data))
            {
                foreach (var item in data)
                {
                    int id = item.Key;
                    VdbBoidState state = item.Value;
                    if (filter(id))
                    {
                        renderer.DrawPoint(id, state.position, 0.01f, 0.02f, Color.white, new Color(1.0f, 0.5f, 0.0f));
                    }
                }
            }
        }
    }

    public class BoidPathsFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Paths";

        public int FramesBeforeCurrent = -1;
        public int FramesAfterCurrent = -1;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            int numFrames = vdb.NumKeyframes;
            int firstFrame = FramesBeforeCurrent >= 0 ? Mathf.Max(0, currentFrame - FramesBeforeCurrent) : 0;
            int lastFrame = FramesAfterCurrent >= 0 ? Mathf.Min(numFrames - 1, currentFrame + FramesAfterCurrent) : numFrames - 1;
            if (lastFrame <= firstFrame)
            {
                return;
            }

            int numSegments = 0;
            Keyframe prevKeyframe = vdb.GetKeyframe(firstFrame);
            for (int frame = firstFrame + 1; frame <= lastFrame; ++frame)
            {
                Keyframe keyframe = vdb.GetKeyframe(frame);
                if (keyframe.TryGetData<VdbBoidStateKeyframe>(out var data)
                 && prevKeyframe.TryGetData<VdbBoidStateKeyframe>(out var prevData))
                {
                    foreach (var item in data)
                    {
                        int id = item.Key;
                        VdbBoidState state = item.Value;
                        if (filter(id))
                        {
                            if (prevData.TryGetValue(id, out VdbBoidState prevState))
                            {
                                numSegments += 2;
                            }
                        }
                    }
                }

                prevKeyframe = keyframe;
            }

            Vector3[] segments = new Vector3[numSegments];

            int seg = 0;
            prevKeyframe = vdb.GetKeyframe(firstFrame);
            for (int frame = firstFrame + 1; frame <= lastFrame; ++frame)
            {
                Keyframe keyframe = vdb.GetKeyframe(frame);
                if (keyframe.TryGetData<VdbBoidStateKeyframe>(out var data)
                 && prevKeyframe.TryGetData<VdbBoidStateKeyframe>(out var prevData))
                {
                    foreach (var item in data)
                    {
                        int id = item.Key;
                        VdbBoidState state = item.Value;
                        if (filter(id))
                        {
                            if (prevData.TryGetValue(id, out VdbBoidState prevState))
                            {
                                segments[seg] = prevState.position;
                                segments[seg + 1] = state.position;
                                seg += 2;
                            }
                        }
                    }
                }

                prevKeyframe = keyframe;
            }

            renderer.DrawLines(segments, Color.gray);
        }
    }

    public class BoidVelocityFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Velocities";

        public float Scale = 1.0f;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            Keyframe keyframe = vdb.GetKeyframe(currentFrame);
            if (keyframe.TryGetData<VdbBoidStateKeyframe>(out var data))
            {
                foreach (var item in data)
                {
                    int id = item.Key;
                    VdbBoidState state = item.Value;
                    if (filter(id))
                    {
                        renderer.DrawLine(state.position, state.position + state.velocity * Scale, Color.yellow);
                    }
                }
            }
        }
    }

    public class BoidRotationFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Rotation";

        public float Scale = 1.0f;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            Keyframe keyframe = vdb.GetKeyframe(currentFrame);
            if (keyframe.TryGetData<VdbBoidStateKeyframe>(out var data))
            {
                foreach (var item in data)
                {
                    int id = item.Key;
                    VdbBoidState state = item.Value;
                    if (filter(id))
                    {
                        renderer.DrawLine(state.position, state.position + state.direction * Scale, Color.blue);
                        renderer.DrawArc(state.position, state.position + state.direction, Vector3.up, state.roll, Scale, Color.green);
                    }
                }
            }
        }
    }

    public class BoidTargetFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Targets";

        public float Scale = 1.0f;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            Keyframe keyframe = vdb.GetKeyframe(currentFrame);
            if (keyframe.TryGetData<VdbBoidStateKeyframe>(out var stateData)
             && keyframe.TryGetData<VdbBoidTargetKeyframe>(out var targetData))
            {
                foreach (var item in targetData)
                {
                    int id = item.Key;
                    VdbBoidTarget target = item.Value;
                    if (filter(id))
                    {
                        if (stateData.TryGetValue(id, out VdbBoidState state))
                        {
                            renderer.DrawLine(state.position, state.position + target.direction * Scale, Color.cyan);
                        }
                    }
                }
            }
        }
    }
}
