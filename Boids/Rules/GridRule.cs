// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.Grid;
using Shwarm.MathUtils;
using Shwarm.Unity;
using UnityEngine;

namespace Shwarm.Boids
{
    [CreateAssetMenu(fileName = "GridRule", menuName = "Boids/GridRule", order = 1)]
    public class GridRule : BoidRule
    {
        private Grid<float> grid;
        private IValueAccessor<float> treeAcc;

        private float time;
        private const float ChangeInterval = 0.5f;
        private const float Speed1 = 1.0f / 7.3f;
        private const float Speed2 = 1.0f / 2.8f;

        private System.Random rng;

        public GridRule()
        {
            grid = new Grid<float>();
            grid.CellSize = new float3(1, 1, 1) * 0.06f;
            treeAcc = grid.Tree.GetAccessor();
            time = 0.0f;
            rng = new System.Random(83523);
        }

        public override void Prepare()
        {
            float prevTime = time;
            time += Time.deltaTime;

            int prevStep = (int)(prevTime / ChangeInterval);
            int step = (int)(time / ChangeInterval);
            if (prevStep < step)
            {
                // float a = 2.0f * Mathf.PI * time * Speed1;
                // float b = 2.0f * Mathf.PI * time * Speed2;
                // float3 p = (Quaternion.Euler(a, 0, 0) * Quaternion.Euler(0, b, 0) * new Vector3(1, 0, 0)).ToFloat3();
                // grid.InverseTransformCenter(p, out GridIndex gridIndex);
                // treeAcc.SetValue(gridIndex, 0.8f);

                int N = 10000;
                float3 min = new float3(-1, -0.3f, -1);
                float3 max = new float3(1, 1.2f, 1);
                float3[] points = new float3[N];
                for (int i = 0; i < N; ++i)
                {
                    float a = (float)rng.NextDouble();
                    float b = (float)rng.NextDouble();
                    float c = (float)rng.NextDouble();
                    points[i] = min + (max - min).Scale(new float3(a, b, c));
                }

                Tree<int> indexTree = new Tree<int>();
                grid.Tree.Clear();
                PointCloudConverter.Convert(points, grid, indexTree);

                BoidDebug.SetGrid(grid);
            }
        }

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            target = null;
            priority = PriorityNone;
            return false;
        }
    }
}
