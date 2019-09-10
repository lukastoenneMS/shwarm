// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Utils;

namespace Shwarm.MathUtils
{
    public static class IndexDetails
    {
        public const int BlockSize = 16;
        public const int BlockSize2 = 256;
        public const int BlockSize3 = 4096;
        public const int BlockLog2Dim = 4;
        public const int BlockLog2Dim2 = 8;
        public const int BlockLog2Dim3 = 12;

        public static GridIndex BlockToGridIndex(BlockIndex blockIndex)
        {
            return new GridIndex(blockIndex.i << BlockLog2Dim, blockIndex.j << BlockLog2Dim, blockIndex.k << BlockLog2Dim);
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
            return new BlockIndex(gridIndex.i >> BlockLog2Dim, gridIndex.j >> BlockLog2Dim, gridIndex.k >> BlockLog2Dim);
        }
        public static BlockIndex GridToBlockIndex(GridIndex gridIndex, out GridIndex localIndex)
        {
            int blockMask = (1 << BlockLog2Dim) - 1;
            BlockIndex blockIndex = new BlockIndex(gridIndex.i >> BlockLog2Dim, gridIndex.j >> BlockLog2Dim, gridIndex.k >> BlockLog2Dim);
            localIndex = new GridIndex(gridIndex.i & blockMask, gridIndex.j & blockMask, gridIndex.k & blockMask);
            return blockIndex;
        }
        public static BlockIndex GridToBlockIndex(GridIndex gridIndex, out int cellIndex)
        {
            int blockMask = (1 << BlockLog2Dim) - 1;
            BlockIndex blockIndex = new BlockIndex(gridIndex.i >> BlockLog2Dim, gridIndex.j >> BlockLog2Dim, gridIndex.k >> BlockLog2Dim);
            GridIndex localIndex = new GridIndex(gridIndex.i & blockMask, gridIndex.j & blockMask, gridIndex.k & blockMask);
            cellIndex = LocalToCellIndex(localIndex);
            return blockIndex;
        }

        public static GridIndex CellToLocalIndex(int cellIndex)
        {
            int blockMask = (1 << BlockLog2Dim) - 1;
            return new GridIndex(
                 cellIndex                   & blockMask,
                (cellIndex >> BlockLog2Dim)  & blockMask,
                (cellIndex >> BlockLog2Dim2) & blockMask);
        }
        public static int LocalToCellIndex(GridIndex gridIndex)
        {
            return gridIndex.i + (gridIndex.j << BlockLog2Dim) + (gridIndex.k << BlockLog2Dim2);
        }
    }

    public struct GridIndex : IEquatable<GridIndex>
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

        public bool Equals(GridIndex other)
        {
            return this.i == other.i && this.j == other.j && this.k == other.k;
        }
    }

    public struct BlockIndex : IEquatable<BlockIndex>
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

        public bool Equals(BlockIndex other)
        {
            return this.i == other.i && this.j == other.j && this.k == other.k;
        }
    }
}
