// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Shwarm.Vdb
{
    [CustomEditor(typeof(VisualDebuggerComponent))]
    public class VisualDebuggerEditor : Editor
    {
        private VisualDebugger vdb;
        private VisualDebuggerComponent component;

        private delegate void DrawFeatureGUI(VisualDebuggerEditor editor, VisualDebuggerFeature feature);

        private static readonly Dictionary<System.Type, DrawFeatureGUI> drawFeatureGuiRegistry = new Dictionary<System.Type, DrawFeatureGUI>()
        {
            { typeof(BoidPathsFeature), (editor, feature) => editor.DrawFeatureGUIImpl(feature as BoidPathsFeature) },
        };

        void OnEnable()
        {
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


            foreach (var feature in vdb.Features)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                bool newEnabled = EditorGUILayout.Toggle(feature.Name, feature.Enabled);
                EditorGUILayout.EndVertical();

                if (newEnabled != feature.Enabled)
                {
                    feature.Enabled = newEnabled;
                }

                if (drawFeatureGuiRegistry.TryGetValue(feature.GetType(), out DrawFeatureGUI guiFn))
                {
                    guiFn(this, feature);
                }
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

        private void DrawFeatureGUIImpl(BoidPathsFeature feature)
        {
            feature.FramesBeforeCurrent = EditorGUILayout.IntField("Frames Before Current", feature.FramesBeforeCurrent);
            feature.FramesAfterCurrent = EditorGUILayout.IntField("Frames After Current", feature.FramesAfterCurrent);
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
        }

        public void DrawLine(int id, Vector3 a, Vector3 b)
        {
            Handles.DrawLine(a, b);
        }

        public void DrawLines(Vector3[] segments)
        {
            Debug.Assert((segments.Length & 1) == 0, "Segments list must have an even number of points");
            Handles.DrawLines(segments);
        }
    }
}
