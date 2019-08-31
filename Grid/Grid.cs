// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Utils;

namespace Grid
{
    public static class IndexDetails
    {
        public const int BlockSize = 16;
        public const int BlockSize2 = 256;
        public const int BlockSize3 = 4096;
        public const int BlockIndexShift = 4;
        public const int BlockIndexShift2 = 8;
        public const int BlockIndexShift3 = 12;
        public const int BlockIndexMask = ~0x0000000F;
        public const int BlockIndexMask2 = ~0x000000FF;
        public const int BlockIndexMask3 = ~0x00000FFF;

        public static GridIndex BlockToGridIndex(BlockIndex blockIndex)
        {
            return new GridIndex(blockIndex.i << BlockIndexShift, blockIndex.j << BlockIndexShift, blockIndex.k << BlockIndexShift);
        }
        public static GridIndex BlockToGridIndex(BlockIndex blockIndex, GridIndex localIndex)
        {
            return BlockToGridIndex(blockIndex) + localIndex;
        }
        public static GridIndex BlockToGridIndex(BlockIndex blockIndex, int cellIndex)
        {
            return BlockToGridIndex(blockIndex) + CellToLocalIndex(cellIndex);
        }

        public static BlockIndex GridToBlockIndex(GridIndex gridIndex)
        {
            return new BlockIndex(gridIndex.i >> BlockIndexShift, gridIndex.j >> BlockIndexShift, gridIndex.k >> BlockIndexShift);
        }
        public static BlockIndex GridToBlockIndex(GridIndex gridIndex, out GridIndex localIndex)
        {
            BlockIndex blockIndex = new BlockIndex(gridIndex.i >> BlockIndexShift, gridIndex.j >> BlockIndexShift, gridIndex.k >> BlockIndexShift);
            localIndex = new GridIndex(gridIndex.i & BlockIndexMask, gridIndex.j & BlockIndexMask, gridIndex.k & BlockIndexMask);
            return blockIndex;
        }
        public static BlockIndex GridToBlockIndex(GridIndex gridIndex, out int cellIndex)
        {
            BlockIndex blockIndex = new BlockIndex(gridIndex.i >> BlockIndexShift, gridIndex.j >> BlockIndexShift, gridIndex.k >> BlockIndexShift);
            GridIndex localIndex = new GridIndex(gridIndex.i & BlockIndexMask, gridIndex.j & BlockIndexMask, gridIndex.k & BlockIndexMask);
            cellIndex = LocalToCellIndex(localIndex);
            return blockIndex;
        }

        public static GridIndex CellToLocalIndex(int cellIndex)
        {
            return new GridIndex(
                 cellIndex                      & BlockSize,
                (cellIndex >> BlockIndexShift)  & BlockSize,
                (cellIndex >> BlockIndexShift2) & BlockSize);
        }
        public static int LocalToCellIndex(GridIndex gridIndex)
        {
            return gridIndex.i + (gridIndex.j << BlockIndexShift) + (gridIndex.k << BlockIndexShift2);
        }
    }

    public struct GridIndex
    {
        public readonly int i;
        public readonly int j;
        public readonly int k;

        public GridIndex(int i, int j, int k)
        {
            this.i = i;
            this.j = j;
            this.k = k;
        }

        public override int GetHashCode()
        {
            return HashUtils.CombineHashCodes(i, j, k);
        }

        public static GridIndex operator +(GridIndex a, GridIndex b)
        {
            return new GridIndex(a.i + b.i, a.j + b.j, a.k + b.k);
        }

        public static GridIndex operator -(GridIndex a, GridIndex b)
        {
            return new GridIndex(a.i - b.i, a.j - b.j, a.k - b.k);
        }
    }

    public struct BlockIndex
    {
        public readonly int i;
        public readonly int j;
        public readonly int k;

        public BlockIndex(int i, int j, int k)
        {
            this.i = i;
            this.j = j;
            this.k = k;
        }

        public override int GetHashCode()
        {
            return HashUtils.CombineHashCodes(i, j, k);
        }
    }

    public class GridBlock<T>
    {
        // TODO use Unity NativeArray here (or move to C++ ...)
        public readonly byte[] active;
        public readonly T[] cells;

        public GridBlock()
        {
            active = new byte[IndexDetails.BlockSize3 >> 3];
            cells = new T[IndexDetails.BlockSize3];
        }

        public bool GetActive(int cellIndex)
        {
            return (active[cellIndex >> 3] & (byte)(1 << (cellIndex & 0xFF))) != 0;
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
            active[cellIndex >> 3] |= (byte)(1 << (cellIndex & 0xFF));
        }

        public void Deactivate(int cellIndex)
        {
            active[cellIndex >> 3] &= (byte)(~(1 << (cellIndex & 0xFF)));
        }
    }

    public class Grid<T>
    {
        private Dictionary<BlockIndex, GridBlock<T>> blocks;
        internal Dictionary<BlockIndex, GridBlock<T>> Blocks => blocks;

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
    }

    public static class GridIterator
    {
        public static IEnumerator<Tuple<GridIndex, T>> GetCells<T>(Grid<T> grid)
        {
            foreach (var blockItem in grid.Blocks)
            {
                GridIndex baseCellIndex = IndexDetails.BlockToGridIndex(blockItem.Key);

                for (int i = 0; i < IndexDetails.BlockSize3; ++i)
                {
                    GridIndex gridIndex = baseCellIndex + IndexDetails.CellToLocalIndex(i);
                    yield return Tuple.Create(gridIndex, blockItem.Value.cells[i]);
                }
            }
        }
    }

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

    public static class RandomGridGenerator
    {
        public static Grid<T> Generate<T>(int[] size)
        {
            var grid = new Grid<T>();
            
            return grid;
        }
    }
}
