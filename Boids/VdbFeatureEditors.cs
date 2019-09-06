// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR

using Shwarm.Vdb;
using UnityEditor;
using UnityEngine;

namespace Shwarm.Boids
{
    [VisualDebuggerFeatureEditor(typeof(BoidPathsFeature))]
    public class BoidPathsFeatureEditor : VisualDebuggerFeatureEditor
    {
        public override void OnGUI(VisualDebuggerFeature _feature, VisualDebuggerContext context)
        {
            var feature = _feature as BoidPathsFeature;
            feature.FramesBeforeCurrent = EditorGUILayout.IntField("Frames Before Current", feature.FramesBeforeCurrent);
            feature.FramesAfterCurrent = EditorGUILayout.IntField("Frames After Current", feature.FramesAfterCurrent);
        }
    }

    [VisualDebuggerFeatureEditor(typeof(BoidVelocityFeature))]
    public class BoidVelocityFeatureEditor : VisualDebuggerFeatureEditor
    {
        public override void OnGUI(VisualDebuggerFeature _feature, VisualDebuggerContext context)
        {
            var feature = _feature as BoidVelocityFeature;
            feature.Scale = EditorGUILayout.Slider("Scale", feature.Scale, 0.0f, 1.0f);
        }
    }

    [VisualDebuggerFeatureEditor(typeof(BoidRotationFeature))]
    public class BoidRotationFeatureEditor : VisualDebuggerFeatureEditor
    {
        public override void OnGUI(VisualDebuggerFeature _feature, VisualDebuggerContext context)
        {
            var feature = _feature as BoidRotationFeature;
            feature.Scale = EditorGUILayout.Slider("Scale", feature.Scale, 0.0f, 1.0f);
        }
    }

    [VisualDebuggerFeatureEditor(typeof(BoidTargetFeature))]
    public class BoidTargetFeatureEditor : VisualDebuggerFeatureEditor
    {
        public override void OnGUI(VisualDebuggerFeature _feature, VisualDebuggerContext context)
        {
            var feature = _feature as BoidTargetFeature;
            feature.Scale = EditorGUILayout.Slider("Scale", feature.Scale, 0.0f, 1.0f);
        }
    }
}

#endif