// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Math = System.Math;

namespace Shwarm.MathUtils
{
    public struct float3
    {
        public float x;
        public float y;
        public float z;

        public float3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static float3 operator + (float3 a, float3 b)
        {
            return new float3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static float3 operator - (float3 a, float3 b)
        {
            return new float3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static float3 operator * (float3 a, float s)
        {
            return new float3(a.x * s, a.y * s, a.z * s);
        }

        public static float3 operator * (float s, float3 b)
        {
            return new float3(b.x * s, b.y * s, b.z * s);
        }

        public float3 Scale(float3 v)
        {
            return new float3(this.x * v.x, this.y * v.y, this.z * v.z);
        }

        public float NormSqr()
        {
            return x*x + y*y + z*z;
        }

        public float Norm()
        {
            return (float)Math.Sqrt(NormSqr());
        }
    }
}
