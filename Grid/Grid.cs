// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;

namespace Shwarm.Grid
{
    public class Grid<T, BlockType>
    {
        private Tree<T, BlockType> tree;
        public Tree<T, BlockType> Tree => tree;

        private float3 origin;
        public float3 Origin { get => origin; set => origin = value; }

        private float3 cellSize;
        private float3 invCellSize;
        public float3 CellSize
        {
            get => cellSize;
            set
            {
                if (cellSize.x <= 0.0f || cellSize.y <= 0.0f || cellSize.z <= 0.0f)
                {
                    throw new ArgumentException("Cell size must not be greater than zero");
                }

                cellSize = value;
                invCellSize = new float3(1.0f/value.x, 1.0f/value.y, 1.0f/value.z);
            }
        }

        private readonly float3 cellCenterOffset = new float3(0.5f, 0.5f, 0.5f);

        public Grid()
        {
            origin = new float3(0.0f, 0.0f, 0.0f);
            cellSize = new float3(1.0f, 1.0f, 1.0f);
            invCellSize = new float3(1.0f, 1.0f, 1.0f);

            tree = new Tree<T, BlockType>();
        }

        public Grid(Grid<T, BlockType> other)
        {
            this.origin = other.origin;
            this.cellSize = other.cellSize;
            this.invCellSize = other.invCellSize;
            this.tree = other.tree.Copy();
        }

        public Grid<T, BlockType> Copy()
        {
            return new Grid<T, BlockType>(this);
        }

        public float3 TransformCorner(float3 gridIndex)
        {
            return gridIndex.Scale(cellSize) + origin;
        }
        public float3 TransformCorner(GridIndex gridIndex)
        {
            return TransformCorner(new float3(gridIndex.i, gridIndex.j, gridIndex.k));
        }

        public float3 TransformCenter(float3 gridIndex)
        {
            return (gridIndex + cellCenterOffset).Scale(cellSize) + origin;
        }
        public float3 TransformCenter(GridIndex gridIndex)
        {
            return TransformCenter(new float3(gridIndex.i, gridIndex.j, gridIndex.k));
        }

        public void InverseTransformCorner(float3 point, out float3 gridIndex)
        {
            gridIndex = (point - origin).Scale(invCellSize);
        }
        public void InverseTransformCorner(float3 point, out GridIndex gridIndex, out float3 cellOffset)
        {
            InverseTransformCorner(point, out float3 pGrid);
            gridIndex = new GridIndex(iFloor(pGrid.x), iFloor(pGrid.y), iFloor(pGrid.z));
            cellOffset = new float3(
                pGrid.x - (float)Math.Floor(pGrid.x),
                pGrid.y - (float)Math.Floor(pGrid.y),
                pGrid.z - (float)Math.Floor(pGrid.z));
        }
        public void InverseTransformCorner(float3 point, out GridIndex gridIndex)
        {
            InverseTransformCorner(point, out float3 pGrid);
            gridIndex = new GridIndex(iFloor(pGrid.x), iFloor(pGrid.y), iFloor(pGrid.z));
        }

        public void InverseTransformCenter(float3 point, out float3 gridIndex)
        {
            gridIndex = (point - origin).Scale(invCellSize) - cellCenterOffset;
        }
        public void InverseTransformCenter(float3 point, out GridIndex gridIndex, out float3 cellOffset)
        {
            InverseTransformCenter(point, out float3 pGrid);
            gridIndex = new GridIndex(iFloor(pGrid.x), iFloor(pGrid.y), iFloor(pGrid.z));
            cellOffset = new float3(
                pGrid.x - (float)Math.Floor(pGrid.x),
                pGrid.y - (float)Math.Floor(pGrid.y),
                pGrid.z - (float)Math.Floor(pGrid.z));
        }
        public void InverseTransformCenter(float3 point, out GridIndex gridIndex)
        {
            InverseTransformCenter(point, out float3 pGrid);
            gridIndex = new GridIndex(iFloor(pGrid.x), iFloor(pGrid.y), iFloor(pGrid.z));
        }

        private int iFloor(float x)
        {
            return x >= 0.0f ? (int)x : (int)x - 1;
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
