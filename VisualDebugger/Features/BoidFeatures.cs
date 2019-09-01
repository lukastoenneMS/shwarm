// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Shwarm.Vdb
{
    public class BoidIdsFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid IDs";

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            Keyframe keyframe = vdb.GetKeyframe(currentFrame);

            foreach (var blob in keyframe.data.blobs)
            {
                if (filter(blob.Key))
                {
                    renderer.DrawText(blob.Value.state.position + new Vector3(0, 0.02f, 0.0f), blob.Key.ToString(), Color.white);
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

            foreach (var blob in keyframe.data.blobs)
            {
                if (filter(blob.Key))
                {
                    renderer.DrawPoint(blob.Key, blob.Value.state.position, 0.01f, 0.02f, Color.white, new Color(1.0f, 0.5f, 0.0f));
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
                foreach (var blob in keyframe.data.blobs)
                {
                    if (filter(blob.Key))
                    {
                        if (prevKeyframe.data.blobs.TryGetValue(blob.Key, out DataBlob prevData))
                        {
                            numSegments += 2;
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
                foreach (var blob in keyframe.data.blobs)
                {
                    if (filter(blob.Key))
                    {
                        if (prevKeyframe.data.blobs.TryGetValue(blob.Key, out DataBlob prevData))
                        {
                            segments[seg] = prevData.state.position;
                            segments[seg + 1] = blob.Value.state.position;
                            seg += 2;
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

            foreach (var blob in keyframe.data.blobs)
            {
                if (filter(blob.Key))
                {
                    var state = blob.Value.state;
                    renderer.DrawLine(state.position, state.position + state.velocity * Scale, Color.yellow);
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

            foreach (var blob in keyframe.data.blobs)
            {
                if (filter(blob.Key))
                {
                    var state = blob.Value.state;
                    renderer.DrawLine(state.position, state.position + state.direction * Scale, Color.blue);
                    renderer.DrawArc(state.position, state.position + state.direction, Vector3.up, state.roll, Scale, Color.green);
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

            foreach (var blob in keyframe.data.blobs)
            {
                if (filter(blob.Key))
                {
                    var state = blob.Value.state;
                    var target = blob.Value.target;
                    renderer.DrawLine(state.position, state.position + target.direction * Scale, Color.cyan);
                }
            }
        }
    }
}
