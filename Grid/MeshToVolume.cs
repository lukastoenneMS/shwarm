// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System.Collections.Generic;

namespace Shwarm.Grid
{
    public static class PointCloudConverter
    {
        public static void Convert(IReadOnlyCollection<float3> points, Grid<float> distGrid, Tree<int> indexTree)
        {
            TreeAccessor<float> distAcc = distGrid.Tree.GetAccessor();
            TreeAccessor<int> indexAcc = indexTree.GetAccessor();

            int index = 0; 
            foreach (float3 p in points)
            {
                distGrid.InverseTransformCenter(p, out GridIndex gridIndex, out float3 cellOffset);

                bool curActive = indexAcc.GetActive(gridIndex);
                if (curActive)
                {
                    float dist = cellOffset.Norm();
                    float curDist = distAcc.GetValue(gridIndex);
                    if (dist < curDist)
                    {
                        indexAcc.SetValueNoActivate(gridIndex, index);
                        distAcc.SetValueNoActivate(gridIndex, cellOffset.Norm());
                    }
                }
                else
                {
                    indexAcc.SetValue(gridIndex, index);
                    distAcc.SetValue(gridIndex, cellOffset.Norm());
                }

                ++index;
            }
        }
    }
}
