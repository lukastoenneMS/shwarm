// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;

namespace Shwarm.Grid
{
    public class Grid<T, BlockType> where BlockType : GridBlock<T>, new()
    {
        private Tree<T, BlockType> tree;
        public Tree<T, BlockType> Tree => tree;

        private Transform transform;
        public Transform Transform { get => transform; set => transform = value; }

        public Grid()
        {
            transform = new Transform(float3.One, float3.Zero);
            tree = new Tree<T, BlockType>();
        }

        public Grid(Grid<T, BlockType> other)
        {
            this.transform = other.transform;
            this.tree = other.tree.Copy();
        }

        public Grid<T, BlockType> Copy()
        {
            return new Grid<T, BlockType>(this);
        }
    }

    public class Grid<T> : Grid<T, GridBlock<T>>
    {
        public Grid()
        {
        }

        public Grid(Grid<T> other)
            : base(other)
        {
        }

        public new Grid<T> Copy()
        {
            return new Grid<T>(this);
        }
    }
}
