// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;

namespace Shwarm.Grid
{
    public class Tree<T>
    {
        private readonly Dictionary<BlockIndex, GridBlock<T>> blocks;
        internal Dictionary<BlockIndex, GridBlock<T>> Blocks => blocks;

        public Tree()
        {
            blocks = new Dictionary<BlockIndex, GridBlock<T>>();
        }

        public Tree<T> Copy()
        {
            Tree<T> result = new Tree<T>();
            foreach (var item in blocks)
            {
                result.blocks.Add(item.Key, item.Value.Copy());
            }
            return result;
        }

        public TreeAccessor<T> GetAccessor()
        {
            return new TreeAccessor<T>(this);
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
}
