// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;

namespace Shwarm.Vdb
{
    public class VisualDebuggerContext
    {
        public VisualDebugger vdb;
        public int currentFrame = 0;
        public int selection = 0;
    }

    public abstract class VisualDebuggerFeatureEditor
    {
        public abstract void OnGUI(VisualDebuggerFeature feature, VisualDebuggerContext context);
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class VisualDebuggerFeatureEditorAttribute : System.Attribute
    {
        private Type featureType;
        public Type FeatureType => featureType;

        public VisualDebuggerFeatureEditorAttribute(Type featureType)
        {
            this.featureType = featureType;
        }
    }

    [CustomEditor(typeof(VisualDebuggerComponent))]
    public class VisualDebuggerEditor : Editor
    {
        private VisualDebugger vdb;
        private VisualDebuggerComponent component;

        private delegate void DrawFeatureGUI(VisualDebuggerEditor editor, VisualDebuggerFeature feature);

        private bool showFilter = false;

        enum FilterMode
        {
            None,
            Solo,
            Mute,
        }

        private readonly HashSet<int> filterIds = new HashSet<int>();
        private FilterMode filterMode = FilterMode.Solo;
        private static readonly string[] filterNames = Enum.GetNames(typeof(FilterMode));
        int selection;

        private bool showItemList = false;
        private Vector2 itemListScroll = Vector2.zero;

        private static readonly Dictionary<Type, VisualDebuggerFeatureEditor> featureEditors = new Dictionary<Type, VisualDebuggerFeatureEditor>();

        static VisualDebuggerEditor()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    VisualDebuggerFeatureEditorAttribute attr = (VisualDebuggerFeatureEditorAttribute)Attribute.GetCustomAttribute(type, typeof(VisualDebuggerFeatureEditorAttribute));
                    if (attr != null)
                    {
                        featureEditors.Add(attr.FeatureType, (VisualDebuggerFeatureEditor)Activator.CreateInstance(type));
                    }
                }
            }
        }

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

                if (featureEditors.TryGetValue(feature.GetType(), out VisualDebuggerFeatureEditor editor))
                {
                    var context = new VisualDebuggerContext() { vdb = vdb, currentFrame = component.CurrentFrame };
                    editor.OnGUI(feature, context);
                }
            }

            DrawFilter();
            DrawItems();

            if (GUI.changed) {
                serializedObject.ApplyModifiedProperties();
            }
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

        private void DrawFilter()
        {
            showFilter = EditorGUILayout.Foldout(showFilter, "Filter");
            if (showFilter)
            {
                EditorGUILayout.BeginHorizontal();
                bool setFilter = GUILayout.Button("Set");
                bool addFilter = GUILayout.Button("Add");
                bool clearFilter = GUILayout.Button("Clear");
                EditorGUILayout.EndHorizontal();

                if (setFilter)
                {
                    filterIds.Clear();
                    if (selection != 0)
                    {
                        filterIds.Add(selection);
                    }
                }
                if (addFilter)
                {
                    if (selection != 0)
                    {
                        filterIds.Add(selection);
                    }
                }
                if (clearFilter)
                {
                    filterIds.Clear();
                }

                filterMode = (FilterMode)GUILayout.SelectionGrid((int)filterMode, filterNames, filterNames.Length);

                EditorGUILayout.LabelField("IDs: " + string.Join(" ", filterIds));
            }
        }

        private void DrawItems()
        {
            showItemList = EditorGUILayout.Foldout(showItemList, "Items");
            if (!showItemList)
            {
                return;
            }
            if (component.CurrentFrame < 0 || component.CurrentFrame >= vdb.NumKeyframes)
            {
                return;
            }

            Shwarm.Vdb.Keyframe keyframe = vdb.GetKeyframe(component.CurrentFrame);

            itemListScroll = GUILayout.BeginScrollView(itemListScroll);
            EditorGUI.indentLevel = 1;

            foreach (var feature in vdb.Features)
            {
                for (var iter = feature.GetIds(keyframe); iter.MoveNext(); )
                {
                    int id = iter.Current;
                    GUI.SetNextControlName("id" + id);
                    EditorGUILayout.SelectableLabel(id.ToString(), GUILayout.Height(18));
                }
            }

            string selectedName = GUI.GetNameOfFocusedControl();
            if (selectedName.StartsWith("id"))
            {
                string idName = selectedName.Substring("id".Length);
                if (int.TryParse(idName, out int id))
                {
                    selection = id;
                }
            }

            GUILayout.EndScrollView();
            EditorGUI.indentLevel = 0;
        }

        void OnSceneGUI()
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            SceneGUIRenderer renderer = new SceneGUIRenderer(SceneView.lastActiveSceneView, selection);
            bool onlyPoints = (Event.current.type != EventType.Repaint);
            vdb.Render(renderer, component.CurrentFrame, IdFilterPredicate, onlyPoints);

            if (renderer.Selection != selection)
            {
                selection = renderer.Selection;
            }
        }

        bool IdFilterPredicate(int id)
        {
            if (filterIds.Count == 0)
            {
                return true;
            }

            switch (filterMode)
            {
                case FilterMode.None:
                    return true;

                case FilterMode.Solo:
                    return filterIds.Contains(id);

                case FilterMode.Mute:
                    return !filterIds.Contains(id);
            }
            return false;
        }
    }

    internal class SceneGUIRenderer : IVisualDebuggerRenderer
    {
        private SceneView sceneView;
        private int selection;
        public int Selection => selection;

        public SceneGUIRenderer(SceneView sceneView, int selection)
        {
            this.sceneView = sceneView;
            this.selection = selection;
        }

        public void DrawText(Vector3 position, string text, Color color)
        {
            Handles.color = color;
            Handles.Label(position, text);
        }

        public void DrawPoint(int id, Vector3 position, float size, float pickSize, Color color, Color selectionColor)
        {
            Handles.color = (id == selection ? selectionColor : color);
            if (Handles.Button(position, sceneView.rotation, size, pickSize, Handles.DotHandleCap))
            {
                selection = id;
            }
        }

        public void DrawCube(int id, Vector3 position, Quaternion rotation, float pickSize, float size, Color color, Color selectionColor)
        {
            Handles.color = (id == selection ? selectionColor : color);
            if (Handles.Button(position, rotation, size, pickSize, Handles.CubeHandleCap))
            {
                selection = id;
            }
        }

        public void DrawLine(Vector3 a, Vector3 b, Color color)
        {
            Handles.color = color;
            Handles.DrawLine(a, b);
        }

        public void DrawLines(Vector3[] segments, Color color)
        {
            Handles.color = color;
            Debug.Assert((segments.Length & 1) == 0, "Segments list must have an even number of points");
            Handles.DrawLines(segments);
        }

        public void DrawArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Color color)
        {
            Handles.color = color;
            Handles.DrawSolidArc(center, normal, from, angle, radius);
        }
    }
}

#endif