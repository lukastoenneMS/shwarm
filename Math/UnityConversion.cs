// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Shwarm.MathUtils;
using UnityEngine;

namespace Shwarm.Unity
{
    public static class UnityMathExtensions
    {
        public static Vector3 ToVector3(this float3 t)
        {
            return new Vector3(t.x, t.y, t.z);
        }

        public static float3 ToFloat3(this Vector3 t)
        {
            return new float3(t.x, t.y, t.z);
        }
    }
}
