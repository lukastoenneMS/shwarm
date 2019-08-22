// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Boids;

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
            UnityEngine.Debug.Log($"keyframe: {keyframe.data.blobs.Count}");
            VisualDebugger.Instance.AddKeyframe(keyframe);
            keyframe = null;
            return VisualDebugger.Instance.NumKeyframes - 1;
        }

        public void DiscardKeyframe()
        {
            keyframe = null;
        }

        public void RecordData(int id, BoidState data)
        {
            if (keyframe != null)
            {
                keyframe.data.RecordData(id, data);
            }
        }
    }
}
