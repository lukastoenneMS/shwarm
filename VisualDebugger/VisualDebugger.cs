// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace Shwarm.Vdb
{
    public class VisualDebugger : Singleton<VisualDebugger>
    {
        public readonly VisualDebuggerFeature[] Features = new VisualDebuggerFeature[]
        {
            new BoidPositionsFeature(),
            new BoidPathsFeature(),
        };

        private readonly List<Keyframe> keyframes = new List<Keyframe>();

        public int NumKeyframes => keyframes.Count;

        public Keyframe GetKeyframe(int index)
        {
            return keyframes[index];
        }

        public void AddKeyframe(Keyframe keyframe)
        {
            keyframes.Add(keyframe);
        }

        public void ClearKeyframes()
        {
            keyframes.Clear();
        }

        public void Render(IVisualDebuggerRenderer renderer, int currentFrame)
        {
            if (currentFrame < 0 || currentFrame >= keyframes.Count)
            {
                return;
            }

            foreach (var feature in Features)
            {
                if (feature.Enabled)
                {
                    feature.Render(this, renderer, currentFrame);
                }
            }
        }
    }

    public interface IVisualDebuggerRenderer
    {
        void DrawPoint(int id, Vector3 p, float size);
        void DrawLine(int id, Vector3 a, Vector3 b);
        void DrawLines(Vector3[] segments);
    }

    public abstract class VisualDebuggerFeature
    {
        public abstract string Name { get; }

        private bool enabled = false;
        public bool Enabled { get => enabled; set => enabled = value; }

        public abstract void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame);
    }

    public class BoidPositionsFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Positions";

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame)
        {
            Keyframe keyframe = vdb.GetKeyframe(currentFrame);

            foreach (var blob in keyframe.data.blobs)
            {
                renderer.DrawPoint(blob.Key, blob.Value.boidPosition, 0.01f);
            }
        }
    }

    public class BoidPathsFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Paths";

        public int FramesBeforeCurrent = -1;
        public int FramesAfterCurrent = -1;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame)
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
                    if (prevKeyframe.data.blobs.TryGetValue(blob.Key, out DataBlob prevData))
                    {
                        numSegments += 2;
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
                    if (prevKeyframe.data.blobs.TryGetValue(blob.Key, out DataBlob prevData))
                    {
                        segments[seg] = prevData.boidPosition;
                        segments[seg + 1] = blob.Value.boidPosition;
                        seg += 2;
                    }
                }

                prevKeyframe = keyframe;
            }

            renderer.DrawLines(segments);
        }
    }
}
