// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;

namespace Shwarm.Grid
{
    public class Tree<T, BlockType> where BlockType : GridBlock<T>, new()
    {
        private readonly Dictionary<BlockIndex, BlockType> blocks;
        internal Dictionary<BlockIndex, BlockType> Blocks => blocks;

        public Tree()
        {
            blocks = new Dictionary<BlockIndex, BlockType>();
        }

        public Tree(Tree<T, BlockType> other)
        {
            blocks = new Dictionary<BlockIndex, BlockType>();
            foreach (var item in other.blocks)
            {
                this.blocks.Add(item.Key, (BlockType)item.Value.Copy());
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

        internal bool TryGetBlock(BlockIndex blockIndex, out BlockType block)
        {
            return blocks.TryGetValue(blockIndex, out block);
        }

        internal BlockType GetOrCreateBlock(BlockIndex blockIndex)
        {
            if (!blocks.TryGetValue(blockIndex, out BlockType block))
            {
                block = new BlockType();
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
