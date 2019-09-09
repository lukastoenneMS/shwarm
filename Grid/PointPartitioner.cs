// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;

using IJobParallelFor = Unity.Jobs.IJobParallelFor;

namespace Shwarm.Grid
{
    /// <summary>
    /// This operator sorts points into per-block bins based on grid-space positions.
    /// </summary>
    public class PointBinningOp : IJobParallelFor
    {
        /// <summary>
        /// Map of per-block index lists, containing indices of all points within a block.
        /// </summary>
        private Dictionary<BlockIndex, NativeArray<int>> indexListMap;

        public void Execute(int i)
        {
            result[i] = a[i] + b[i];
        }
    }

    public class PointPartitioner
    {

    }
}
