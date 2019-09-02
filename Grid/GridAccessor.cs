// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Shwarm.Grid
{
    public class TreeAccessor<T>
    {
        private Tree<T> tree;

        public TreeAccessor(Tree<T> tree)
        {
            this.tree = tree;
        }

        public bool GetActive(GridIndex index)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            if (tree.TryGetBlock(blockIndex, out GridBlock<T> block))
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
            if (tree.TryGetBlock(blockIndex, out GridBlock<T> block))
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
            if (tree.TryGetBlock(blockIndex, out GridBlock<T> block))
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
            GridBlock<T> block = tree.GetOrCreateBlock(blockIndex);

            block.SetValue(cellIndex, value);
        }

        public void Deactivate(GridIndex index)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            if (tree.TryGetBlock(blockIndex, out GridBlock<T> block))
            {
                block.Deactivate(cellIndex);
            }
        }
    }
}
