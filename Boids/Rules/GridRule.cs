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

        public GridRule()
        {
            grid = new Grid<float>();
            gridAcc = grid.GetAccessor();
        }

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            gridAcc.SetValue(new GridIndex(1,2,3), 8345.2f);

            target = null;
            priority = PriorityNone;
            return false;
        }
    }
}
