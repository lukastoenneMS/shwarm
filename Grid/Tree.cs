// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;

namespace Shwarm.Grid
{
    public class Tree<T, BlockType>
    {
        private readonly Dictionary<BlockIndex, GridBlock<T>> blocks;
        internal Dictionary<BlockIndex, GridBlock<T>> Blocks => blocks;

        public Tree()
        {
            blocks = new Dictionary<BlockIndex, GridBlock<T>>();
        }

        public Tree(Tree<T, BlockType> other)
        {
            foreach (var item in other.blocks)
            {
                this.blocks.Add(item.Key, item.Value.Copy());
            }
        }

        public Tree<T, BlockType> Copy()
        {
            return new Tree<T, BlockType>(this);
        }

        public TreeAccessor<T, BlockType> GetAccessor()
        {
            return new TreeAccessor<T, BlockType>(this);
        }

        internal bool TryGetBlock(BlockIndex blockIndex, out GridBlock<T> block)
        {
            return blocks.TryGetValue(blockIndex, out block);
        }

        internal GridBlock<T> GetOrCreateBlock(BlockIndex blockIndex)
        {
            if (!blocks.TryGetValue(blockIndex, out GridBlock<T> block))
            {
                block = new GridBlock<T>();
                blocks.Add(blockIndex, block);
            }
            return block;
        }

        public void Clear()
        {
            blocks.Clear();
        }
    }

    public class Tree<T> : Tree<T, GridBlock<T>>
    {
        public Tree()
        {
        }

        public Tree(Tree<T> other)
            : base(other)
        {
        }

        public new Tree<T> Copy()
        {
            return new Tree<T>(this);
        }
    }
}
