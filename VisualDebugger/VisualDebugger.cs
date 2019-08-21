// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;

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
    }
}
