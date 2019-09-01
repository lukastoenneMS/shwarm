// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using Grid;

namespace Boids
{
    [CreateAssetMenu(fileName = "GridRule", menuName = "Boids/GridRule", order = 1)]
    public class GridRule : BoidRule
    {
        private Grid<float> grid;
        private GridAccessor<float> gridAcc;

        private float time;
        public float ChangeInterval = 2.0f;

        public GridRule()
        {
            grid = new Grid<float>();
            gridAcc = grid.GetAccessor();
            time = 0.0f;
        }

        public override void Prepare()
        {
            float prevTime = time;
            time += Time.deltaTime;


            int prevStep = (int)(prevTime / ChangeInterval);
            int step = (int)(time / ChangeInterval);
            if (prevStep < step)
            {
                gridAcc.SetValue(new GridIndex(step,2,3), time);
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
