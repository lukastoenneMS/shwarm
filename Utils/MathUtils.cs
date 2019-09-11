// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Math = System.Math;

namespace Shwarm.MathUtils
{
    public static class MathUtils
    {
        public static int iFloor(float x)
        {
            return x >= 0.0f ? (int)x : (int)x - 1;
        }
    }
}
