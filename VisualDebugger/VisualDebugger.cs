// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shwarm.Vdb
{
    public class VisualDebugger : Singleton<VisualDebugger>
    {
        public readonly VisualDebuggerFeature[] Features = new VisualDebuggerFeature[]
        {
            new BoidIdsFeature(),
            new BoidPositionsFeature() { Enabled=true },
            new BoidPathsFeature(),
            new BoidVelocityFeature() { Enabled=true },
            new BoidRotationFeature(),
            new BoidTargetFeature(),
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

        public void Render(IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter, bool onlyPoints)
        {
            if (currentFrame < 0 || currentFrame >= keyframes.Count)
            {
                return;
            }

            foreach (var feature in Features)
            {
                if (onlyPoints && !(feature is BoidPositionsFeature))
                {
                    continue;
                }

                if (feature.Enabled)
                {
                    feature.Render(this, renderer, currentFrame, filter);
                }
            }
        }
    }

    public interface IVisualDebuggerRenderer
    {
        void DrawText(Vector3 position, string text, Color color);
        void DrawPoint(int id, Vector3 position, float size, float pickSize, Color color, Color selectionColor);
        void DrawLine(Vector3 a, Vector3 b, Color color);
        void DrawLines(Vector3[] segments, Color color);
        void DrawArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Color color);
    }

    public abstract class VisualDebuggerFeature
    {
        public abstract string Name { get; }

        private bool enabled = false;
        public bool Enabled { get => enabled; set => enabled = value; }

        public abstract void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter);
    }
}
