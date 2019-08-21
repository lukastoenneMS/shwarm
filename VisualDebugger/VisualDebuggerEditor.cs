// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEditor;

namespace Shwarm.Vdb
{
    [CustomEditor(typeof(VisualDebuggerComponent))]
    public class VisualDebuggerEditor : Editor
    {
        private SerializedProperty currentFrame;

        private VisualDebugger vdb;
        private VisualDebuggerComponent component;

        void OnEnable()
        {
            currentFrame = serializedObject.FindProperty ("currentFrame");
            vdb = VisualDebugger.Instance;
            component = target as VisualDebuggerComponent;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (!Application.isPlaying)
            {
                int newFrame = DrawKeyframeSlider();
                bool clearKeyframes = GUILayout.Button("Clear Keyframes");

                if (newFrame != component.CurrentFrame)
                {
                    component.CurrentFrame = newFrame;
                }
                if (clearKeyframes)
                {
                    component.ClearKeyframes();
                }
            }
            else
            {
                GUI.enabled = false;

                DrawKeyframeSlider();
            }

            serializedObject.ApplyModifiedProperties ();
        }

        private int DrawKeyframeSlider()
        {
            int maxFrame = Mathf.Max(vdb.NumKeyframes - 1, 0);

            GUILayout.BeginHorizontal();
            int newFrame = EditorGUILayout.IntSlider(component.CurrentFrame, 0, maxFrame);
            EditorGUILayout.LabelField($"/ {vdb.NumKeyframes}", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            return newFrame;
        }
    }
}
