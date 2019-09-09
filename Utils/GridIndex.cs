// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Utils;

namespace Shwarm.MathUtils
{
    public static class IndexDetails
    {
        public const int BlockSize = 16;
        public const int BlockSize2 = 256;
        public const int BlockSize3 = 4096;
        public const int BlockIndexMask = 0x0000000F;
        public const int BlockIndexMask2 = 0x000000FF;
        public const int BlockIndexMask3 = 0x00000FFF;
        public const int BlockIndexShift = 4;
        public const int BlockIndexShift2 = 8;
        public const int BlockIndexShift3 = 12;

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
                 cellIndex                      & BlockIndexMask,
                (cellIndex >> BlockIndexShift)  & BlockIndexMask,
                (cellIndex >> BlockIndexShift2) & BlockIndexMask);
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

        public override string ToString()
        {
            return "GridIndex(" + i + ", " + j + ", " + k + ")";
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

        public override string ToString()
        {
            return "BlockIndex(" + i + ", " + j + ", " + k + ")";
        }
    }
}
