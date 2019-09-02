// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using System;
using System.Collections.Generic;

namespace Shwarm.Grid
{
    public class GridBlock<T>
    {
        private const int ActiveTypeMask = 0x1f;
        private const int ActiveTypeShift = 5;

        // TODO use Unity NativeArray here (or move to C++ ...)
        public readonly int[] active;
        public readonly T[] cells;

        private int activeCount;
        public int ActiveCount => activeCount;

        public GridBlock()
        {
            active = new int[IndexDetails.BlockSize3 >> 3];
            activeCount = 0;
            cells = new T[IndexDetails.BlockSize3];
        }

        public GridBlock<T> Copy()
        {
            GridBlock<T> result = new GridBlock<T>();
            System.Buffer.BlockCopy(this.active, 0, result.active, 0, this.active.Length);
            System.Buffer.BlockCopy(this.cells, 0, result.cells, 0, this.cells.Length);
            result.activeCount = this.activeCount;
            return result;
        }

        public bool GetActive(int cellIndex)
        {
            int activeIndex = cellIndex >> ActiveTypeShift;
            int activeMask = 1 << (cellIndex & ActiveTypeMask);
            int currentActiveBits = active[activeIndex] & activeMask;
            return currentActiveBits != 0;
        }

        public T GetValue(int cellIndex)
        {
            return cells[cellIndex];
        }

        public void GetValue(int cellIndex, out T value, out bool isActive)
        {
            value = GetValue(cellIndex);
            isActive = GetActive(cellIndex);
        }

        public void SetValue(int cellIndex, T value)
        {
            cells[cellIndex] = value;

            int activeIndex = cellIndex >> ActiveTypeShift;
            int activeMask = 1 << (cellIndex & ActiveTypeMask);
            int currentActiveBits = active[activeIndex] & activeMask;
            activeCount = (currentActiveBits == 0 ? activeCount + 1 : activeCount);
            active[activeIndex] = currentActiveBits | activeMask;
        }

        public void Deactivate(int cellIndex)
        {
            int activeIndex = cellIndex >> ActiveTypeShift;
            int activeMask = 1 << (cellIndex & ActiveTypeMask);
            int currentActiveBits = active[activeIndex] & activeMask;
            activeCount = (currentActiveBits == 0 ? activeCount : activeCount - 1);
            active[activeIndex] = currentActiveBits & ~activeMask;
        }
    }

    public class Grid<T>
    {
        private readonly Dictionary<BlockIndex, GridBlock<T>> blocks;
        internal Dictionary<BlockIndex, GridBlock<T>> Blocks => blocks;

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

            blocks = new Dictionary<BlockIndex, GridBlock<T>>();
        }

        public Grid<T> Copy()
        {
            Grid<T> result = new Grid<T>();

            result.origin = origin;
            result.cellSize = cellSize;
            result.invCellSize = invCellSize;

            foreach (var item in blocks)
            {
                result.blocks.Add(item.Key, item.Value.Copy());
            }
            return result;
        }

        public GridAccessor<T> GetAccessor()
        {
            return new GridAccessor<T>(this);
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
            gridIndex = new GridIndex((int)pGrid.x, (int)pGrid.y, (int)pGrid.z);
            cellOffset = new float3(
                pGrid.x - (float)Math.Floor(pGrid.x),
                pGrid.y - (float)Math.Floor(pGrid.y),
                pGrid.z - (float)Math.Floor(pGrid.z));
        }
        public void InverseTransformCorner(float3 point, out GridIndex gridIndex)
        {
            InverseTransformCorner(point, out float3 pGrid);
            gridIndex = new GridIndex((int)pGrid.x, (int)pGrid.y, (int)pGrid.z);
        }

        public void InverseTransformCenter(float3 point, out float3 gridIndex)
        {
            gridIndex = (point - origin).Scale(invCellSize) - cellCenterOffset;
        }
        public void InverseTransformCenter(float3 point, out GridIndex gridIndex, out float3 cellOffset)
        {
            InverseTransformCenter(point, out float3 pGrid);
            gridIndex = new GridIndex((int)pGrid.x, (int)pGrid.y, (int)pGrid.z);
            cellOffset = new float3(
                pGrid.x - (float)Math.Floor(pGrid.x),
                pGrid.y - (float)Math.Floor(pGrid.y),
                pGrid.z - (float)Math.Floor(pGrid.z));
        }
        public void InverseTransformCenter(float3 point, out GridIndex gridIndex)
        {
            InverseTransformCenter(point, out float3 pGrid);
            gridIndex = new GridIndex((int)pGrid.x, (int)pGrid.y, (int)pGrid.z);
        }
    }

    public static class RandomGridGenerator
    {
        public static Grid<T> Generate<T>(int[] size)
        {
            var grid = new Grid<T>();
            
            return grid;
        }
    }
}
