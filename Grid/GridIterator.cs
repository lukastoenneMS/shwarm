// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;

namespace Shwarm.Grid
{
    public static class GridIterator
    {
        public static IEnumerator<Tuple<GridIndex, T>> GetCells<T>(Grid<T> grid)
        {
            foreach (var blockItem in grid.Tree.Blocks)
            {
                GridIndex baseCellIndex = IndexDetails.BlockToGridIndex(blockItem.Key);

                for (int cellIndex = 0; cellIndex < IndexDetails.BlockSize3; ++cellIndex)
                {
                    if (blockItem.Value.GetActive(cellIndex))
                    {
                        GridIndex gridIndex = baseCellIndex + IndexDetails.CellToLocalIndex(cellIndex);
                        yield return Tuple.Create(gridIndex, blockItem.Value.cells[cellIndex]);
                    }
                }
            }
        }
    }
}
