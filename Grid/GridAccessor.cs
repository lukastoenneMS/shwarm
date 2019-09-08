// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Shwarm.Grid
{
    // XXX Have a look at performance implications of using an interface here.
    // This was introduced to avoid verbose types when return accessors from trees,
    // but there may be other workarounds,
    // like making fully templated accessor types convertible to simpler ones.
    public interface IValueAccessor<T>
    {
        bool GetActive(GridIndex index);

        T GetValue(GridIndex index);

        void GetValue(GridIndex index, out T value, out bool isActive);

        void SetValue(GridIndex index, T value);

        void SetValueNoActivate(GridIndex index, T value);

        void Deactivate(GridIndex index);
    }

    public class TreeAccessor<T, BlockType> : IValueAccessor<T> where BlockType : GridBlock<T>, new()
    {
        private Tree<T, BlockType> tree;

        public TreeAccessor(Tree<T, BlockType> tree)
        {
            this.tree = tree;
        }

        public bool GetActive(GridIndex index)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            if (tree.TryGetBlock(blockIndex, out BlockType block))
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
            if (tree.TryGetBlock(blockIndex, out BlockType block))
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
            if (tree.TryGetBlock(blockIndex, out BlockType block))
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
            BlockType block = tree.GetOrCreateBlock(blockIndex);

            block.SetValue(cellIndex, value);
        }

        public void SetValueNoActivate(GridIndex index, T value)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            BlockType block = tree.GetOrCreateBlock(blockIndex);

            block.SetValueNoActivate(cellIndex, value);
        }

        public void Deactivate(GridIndex index)
        {
            BlockIndex blockIndex = IndexDetails.GridToBlockIndex(index, out int cellIndex);
            if (tree.TryGetBlock(blockIndex, out BlockType block))
            {
                block.Deactivate(cellIndex);
            }
        }
    }
}
