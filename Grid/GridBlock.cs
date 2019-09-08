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

        public virtual GridBlock<T> Copy()
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

        public void SetValueNoActivate(int cellIndex, T value)
        {
            cells[cellIndex] = value;
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
}
