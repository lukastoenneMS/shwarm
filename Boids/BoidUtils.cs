// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;

namespace Boids
{
    public static class BoidUtils
    {
        /// <summary>
        /// Compute a distance field inside the positive cone, including gradient.
        /// </summary>
        public static bool GetInsidePositiveConeDistance(Vector3 dir, Vector3 coneDir, float coneRadius, out float distance, out Vector3 gradient)
        {
            float sqrRadius = coneRadius * coneRadius;
            Vector3 normConeDir = coneDir.normalized;

            // Length to the cone side
            float sqrConeSide = coneDir.sqrMagnitude - sqrRadius;
            // Special case: inside the sphere
            if (sqrConeSide <= 0.0f)
            {
                float coneDistance = coneDir.magnitude;
                distance = 1.0f - coneDistance / coneRadius;
                gradient = normConeDir;
                return true;
            }

            // Length of dir vector in direction of the cone
            float coneDot = Vector3.Dot(dir, normConeDir);
            // Negative cone case, ignore
            if (coneDot <= 0.0f)
            {
                distance = 0.0f;
                gradient = Vector3.zero;
                return false;
            }

            // Split
            Vector3 conePart = coneDot * normConeDir;
            Vector3 orthoPart = dir - conePart;
            Debug.Assert(Mathf.Abs(Vector3.Dot(conePart, orthoPart)) < 0.01f);

            float sqrOrtho = orthoPart.sqrMagnitude;
            float sqrCone = conePart.sqrMagnitude;
            if (sqrOrtho > 0.0f && sqrCone > 0.0f)
            {
                float sqrTanConeAngle = sqrRadius / (coneDir.sqrMagnitude - sqrRadius);
                float sqrOrthoNew = sqrCone * sqrTanConeAngle;
                float sqrScale = sqrOrtho / sqrOrthoNew;
                if (sqrScale < 1.0f)
                {
                    // scale < 1: vector is inside the cone
                    float scale = Mathf.Sqrt(sqrScale);
                    distance = 1.0f - scale;
                    gradient = -orthoPart.normalized;
                    return true;
                }
                else
                {
                    // scale >= 1: vector is outside the cone
                    distance = 0.0f;
                    gradient = Vector3.zero;
                    return false;
                }
            }
            else
            {
                /// Unlikely corner case: direction coincides exactly with cone direction, no gradient
                distance = 1.0f;
                gradient = Vector3.zero;
                return false;
            }
        }
    }
}
