// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Math = System.Math;
using System;

namespace Shwarm.MathUtils
{
    public struct Transform
    {
        private float3 origin;
        public float3 Origin { get => origin; set => origin = value; }

        private float3 cellSize;
        private float3 invCellSize;
        public float3 CellSize
        {
            get => cellSize;
            set => cellSize = GetInvCellSize(value);
        }

        private static readonly float3 cellCenterOffset = new float3(0.5f, 0.5f, 0.5f);

        private static float3 GetInvCellSize(float3 cellSize)
        {
            if (cellSize.x <= 0.0f || cellSize.y <= 0.0f || cellSize.z <= 0.0f)
            {
                throw new ArgumentException("Cell size must not be greater than zero");
            }

            return new float3(1.0f/cellSize.x, 1.0f/cellSize.y, 1.0f/cellSize.z);
        }

        public Transform(float3 cellSize, float3 origin)
        {
            this.origin = origin;
            this.cellSize = cellSize;
            this.invCellSize = GetInvCellSize(cellSize);
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
}
