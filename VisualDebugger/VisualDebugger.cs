// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Shwarm.Vdb
{
    public class VisualDebugger : Singleton<VisualDebugger>
    {
        private static readonly Dictionary<int, Type> featureTypes = new Dictionary<int, Type>();

        static VisualDebugger()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in asm.GetTypes())
                {
                    VisualDebuggerFeatureAttribute attr = (VisualDebuggerFeatureAttribute)Attribute.GetCustomAttribute(type, typeof(VisualDebuggerFeatureAttribute));
                    if (attr != null)
                    {
                        featureTypes.Add(attr.Priority, type);
                    }
                }
            }
        }

        private VisualDebuggerFeature[] features;
        internal VisualDebuggerFeature[] Features => features;

        public VisualDebugger()
        {
            features = new VisualDebuggerFeature[featureTypes.Count];
            int i = 0;
            foreach (var ft in featureTypes)
            {
                features[i++] = (VisualDebuggerFeature)Activator.CreateInstance(ft.Value);
            }
        }

        private readonly List<Keyframe> keyframes = new List<Keyframe>();
        public int NumKeyframes => keyframes.Count;

        public Keyframe GetKeyframe(int index)
        {
            return keyframes[index];
        }

        public bool TryGetKeyframeData<T>(int index, out T data) where T : class, IKeyframeFeature
        {
            Keyframe keyframe = keyframes[index];
            return keyframe.TryGetData<T>(out data);
        }

        public bool TryGetLatestKeyframeData<T>(int index, out T data, out int latestIndex) where T : class, IKeyframeFeature
        {
            latestIndex = index;
            while (index >= 0)
            {
                Keyframe keyframe = keyframes[index];
                if (keyframe.TryGetData<T>(out data))
                {
                    return true;
                }

                --index;
            }
            latestIndex = 0;
            data = null;
            return false;
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
                if (onlyPoints && !feature.IsPointFeature)
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
        void DrawCube(int id, Vector3 position, Quaternion rotation, float pickSize, float size, Color color, Color selectionColor);
        void DrawLine(Vector3 a, Vector3 b, Color color);
        void DrawLines(Vector3[] segments, Color color);
        void DrawArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, Color color);
    }

    public abstract class VisualDebuggerFeature
    {
        public abstract string Name { get; }

        private bool enabled = false;
        public bool Enabled { get => enabled; set => enabled = value; }

        public virtual bool IsPointFeature => false;

        public abstract void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter);

        public virtual IEnumerator<int> GetIds(Shwarm.Vdb.Keyframe keyframe) { yield break; }
    }

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class VisualDebuggerFeatureAttribute : System.Attribute
    {
        private int priority = 0;
        public int Priority => priority;

        public VisualDebuggerFeatureAttribute(int priority)
        {
            this.priority = priority;
        }
    }
}
