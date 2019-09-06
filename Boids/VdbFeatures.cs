// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using Shwarm.Unity;
using Shwarm.Vdb;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shwarm.Boids
{
    public struct VdbBoidState
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 direction;
        public float roll;
        public Vector3 angularVelocity;
    }
    public class VdbBoidStateKeyframe : InstanceKeyframeFeature<VdbBoidState> { }

    public struct VdbBoidTarget
    {
        public Vector3 direction;
        public float speed;
    }
    public class VdbBoidTargetKeyframe : InstanceKeyframeFeature<VdbBoidTarget> { }

    public class VdbBoidGridKeyframe : IKeyframeFeature
    {
        public Grid.Grid<float> grid;
    }


    [VisualDebuggerFeature(10)]
    public class BoidIdsFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid IDs";

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            if (vdb.TryGetKeyframeData<VdbBoidStateKeyframe>(currentFrame, out var data))
            {
                foreach (var item in data)
                {
                    int id = item.Key;
                    VdbBoidState state = item.Value;
                    if (filter(id))
                    {
                        renderer.DrawText(state.position + new Vector3(0, 0.02f, 0.0f), id.ToString(), Color.white);
                    }
                }
            }
        }

        public override IEnumerator<int> GetIds(Shwarm.Vdb.Keyframe keyframe)
        {
            if (keyframe.TryGetData<VdbBoidStateKeyframe>(out var data))
            {
                foreach (var item in data)
                {
                    yield return item.Key;
                }
            }
        }
    }

    [VisualDebuggerFeature(20)]
    public class BoidPositionsFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Positions";

        public override bool IsPointFeature => true;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            if (vdb.TryGetKeyframeData<VdbBoidStateKeyframe>(currentFrame, out var data))
            {
                foreach (var item in data)
                {
                    int id = item.Key;
                    VdbBoidState state = item.Value;
                    if (filter(id))
                    {
                        renderer.DrawPoint(id, state.position, 0.01f, 0.02f, Color.white, new Color(1.0f, 0.5f, 0.0f));
                    }
                }
            }
        }
    }

    [VisualDebuggerFeature(30)]
    public class BoidPathsFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Paths";

        public int FramesBeforeCurrent = -1;
        public int FramesAfterCurrent = -1;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            int numFrames = vdb.NumKeyframes;
            int firstFrame = FramesBeforeCurrent >= 0 ? Mathf.Max(0, currentFrame - FramesBeforeCurrent) : 0;
            int lastFrame = FramesAfterCurrent >= 0 ? Mathf.Min(numFrames - 1, currentFrame + FramesAfterCurrent) : numFrames - 1;
            if (lastFrame <= firstFrame)
            {
                return;
            }

            int numSegments = 0;
            for (int frame = firstFrame + 1; frame <= lastFrame; ++frame)
            {
                if (vdb.TryGetKeyframeData<VdbBoidStateKeyframe>(frame, out var data)
                 && vdb.TryGetKeyframeData<VdbBoidStateKeyframe>(frame - 1, out var prevData))
                {
                    foreach (var item in data)
                    {
                        int id = item.Key;
                        VdbBoidState state = item.Value;
                        if (filter(id))
                        {
                            if (prevData.TryGetValue(id, out VdbBoidState prevState))
                            {
                                numSegments += 2;
                            }
                        }
                    }
                }
            }

            Vector3[] segments = new Vector3[numSegments];

            int seg = 0;
            for (int frame = firstFrame + 1; frame <= lastFrame; ++frame)
            {
                if (vdb.TryGetKeyframeData<VdbBoidStateKeyframe>(frame, out var data)
                 && vdb.TryGetKeyframeData<VdbBoidStateKeyframe>(frame - 1, out var prevData))
                {
                    foreach (var item in data)
                    {
                        int id = item.Key;
                        VdbBoidState state = item.Value;
                        if (filter(id))
                        {
                            if (prevData.TryGetValue(id, out VdbBoidState prevState))
                            {
                                segments[seg] = prevState.position;
                                segments[seg + 1] = state.position;
                                seg += 2;
                            }
                        }
                    }
                }
            }

            renderer.DrawLines(segments, Color.gray);
        }
    }

    [VisualDebuggerFeature(40)]
    public class BoidVelocityFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Velocities";

        public float Scale = 1.0f;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            if (vdb.TryGetKeyframeData<VdbBoidStateKeyframe>(currentFrame, out var data))
            {
                foreach (var item in data)
                {
                    int id = item.Key;
                    VdbBoidState state = item.Value;
                    if (filter(id))
                    {
                        renderer.DrawLine(state.position, state.position + state.velocity * Scale, Color.yellow);
                    }
                }
            }
        }
    }

    [VisualDebuggerFeature(50)]
    public class BoidRotationFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Rotation";

        public float Scale = 1.0f;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            if (vdb.TryGetKeyframeData<VdbBoidStateKeyframe>(currentFrame, out var data))
            {
                foreach (var item in data)
                {
                    int id = item.Key;
                    VdbBoidState state = item.Value;
                    if (filter(id))
                    {
                        renderer.DrawLine(state.position, state.position + state.direction * Scale, Color.blue);
                        renderer.DrawArc(state.position, state.position + state.direction, Vector3.up, state.roll, Scale, Color.green);
                    }
                }
            }
        }
    }

    [VisualDebuggerFeature(60)]
    public class BoidTargetFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Targets";

        public float Scale = 1.0f;

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            if (vdb.TryGetKeyframeData<VdbBoidStateKeyframe>(currentFrame, out var stateData)
             && vdb.TryGetKeyframeData<VdbBoidTargetKeyframe>(currentFrame, out var targetData))
            {
                foreach (var item in targetData)
                {
                    int id = item.Key;
                    VdbBoidTarget target = item.Value;
                    if (filter(id))
                    {
                        if (stateData.TryGetValue(id, out VdbBoidState state))
                        {
                            renderer.DrawLine(state.position, state.position + target.direction * Scale, Color.cyan);
                        }
                    }
                }
            }
        }
    }

    [VisualDebuggerFeature(70)]
    public class BoidGridFeature : VisualDebuggerFeature
    {
        public override string Name => "Boid Grid";

        public override void Render(VisualDebugger vdb, IVisualDebuggerRenderer renderer, int currentFrame, Predicate<int> filter)
        {
            if (vdb.TryGetLatestKeyframeData<VdbBoidGridKeyframe>(currentFrame, out var gridData, out int frame))
            {
                Grid.TreeAccessor<float> acc = gridData.grid.Tree.GetAccessor();

                float3 cellSize = gridData.grid.CellSize;
                float cellScale = Mathf.Min(cellSize.x, cellSize.y, cellSize.z);

                for (var iter = Grid.GridIterator.GetCells(gridData.grid); iter.MoveNext(); )
                {
                    Vector3 center = gridData.grid.TransformCenter(iter.Current.Item1).ToVector3();
                    float size = cellScale;
                    float pickSize = size * 1.2f;
                    renderer.DrawCube(0, new Vector3(center.x, center.y, center.z), Quaternion.identity, pickSize, size, Color.white, new Color(1.0f, 0.5f, 0.0f));
                    // Debug.Log($"First cell: {iter.Current.Item1} -> {iter.Current.Item2}");
                }
            }
        }
    }
}
