// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Shwarm.Vdb
{
    public class VisualRecorder : Singleton<VisualRecorder>
    {
        private Keyframe keyframe;
        public Keyframe Keyframe => keyframe;

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
    }
}
