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

            MonoScript monoScript = MonoScript.FromMonoBehaviour(component);
            int currentExecutionOrder = MonoImporter.GetExecutionOrder(monoScript);
            int newExecutionOrder = EditorGUILayout.IntField("Execution Order", currentExecutionOrder);

            if (newExecutionOrder != currentExecutionOrder)
            {
                MonoImporter.SetExecutionOrder(monoScript, newExecutionOrder);
            }

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
            int newFrame = EditorGUILayout.IntSlider("Frame", component.CurrentFrame, 0, maxFrame);
            // TODO how to make this shrink?
            // EditorGUILayout.LabelField($"/ {vdb.NumKeyframes}", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            return newFrame;
        }

        void OnSceneGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                SceneGUIRenderer renderer = new SceneGUIRenderer(SceneView.lastActiveSceneView);
                vdb.Render(renderer, component.CurrentFrame);
            }
        }
    }

    internal class SceneGUIRenderer : IVisualDebuggerRenderer
    {
        SceneView sceneView;

        public SceneGUIRenderer(SceneView sceneView)
        {
            this.sceneView = sceneView;
        }

        public void DrawPoint(int id, Vector3 p, float size)
        {
            Handles.RectangleHandleCap(id, p, sceneView.rotation, size, EventType.Repaint);
            // Handles.DrawLine(Vector3.zero, p);
        }
    }
}
