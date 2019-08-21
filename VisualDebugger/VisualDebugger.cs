// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace Shwarm.Vdb
{
    public class VisualDebugger : Singleton<VisualDebugger>
    {
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

            Keyframe keyframe = keyframes[currentFrame];
            foreach (var blob in keyframe.data.blobs)
            {
                renderer.DrawPoint(blob.Key, blob.Value.boidPosition, 0.01f);
            }
        }
    }

    public interface IVisualDebuggerRenderer
    {
        void DrawPoint(int id, Vector3 p, float size);
    }
}
