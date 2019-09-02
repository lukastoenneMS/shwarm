// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Shwarm.Grid
{
    public class GridAccessor<T>
    {
        private Grid<T> grid;

        public GridAccessor(Grid<T> grid)
        {
            this.grid = grid;
        }

        public bool GetActive(GridIndex index)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            if (grid.TryGetBlock(blockIndex, out GridBlock<T> block))
            {
                return block.GetActive(cellIndex);
            }
            else
            {
                return false;
            }
        }

        public T GetValue(GridIndex index)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            if (grid.TryGetBlock(blockIndex, out GridBlock<T> block))
            {
                return block.GetValue(cellIndex);
            }
            else
            {
                return default(T);
            }
        }

        public void GetValue(GridIndex index, out T value, out bool isActive)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            if (grid.TryGetBlock(blockIndex, out GridBlock<T> block))
            {
                block.GetValue(cellIndex, out value, out isActive);
            }
            else
            {
                value = default(T);
                isActive = false;
            }
        }

        public void SetValue(GridIndex index, T value)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            GridBlock<T> block = grid.GetOrCreateBlock(blockIndex);

            block.SetValue(cellIndex, value);
        }

        public void Deactivate(GridIndex index)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            if (grid.TryGetBlock(blockIndex, out GridBlock<T> block))
            {
                block.Deactivate(cellIndex);
            }
        }
    }
}
