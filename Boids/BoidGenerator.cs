// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    public class BoidGenerator
    {
        public float radius = 2.5f;

        private static Vector3 sampleUnitSphereSurface(System.Random rng)
        {
            double z = 2.0 * rng.NextDouble() - 1.0;
            double theta = rng.NextDouble() * 2.0 * Math.PI;
            double xy = Math.Sqrt(1.0 - z*z);
            double x = xy * Math.Cos(theta);
            double y = xy * Math.Sin(theta);
            return new Vector3((float)x, (float)y, (float)z);
        }

        private static Vector3 sampleUnitSphereVolume(System.Random rng)
        {
            double x, y, z;
            do {
                x = 2.0 * rng.NextDouble() - 1.0;
                y = 2.0 * rng.NextDouble() - 1.0;
                z = 2.0 * rng.NextDouble() - 1.0;
            }
            while (x*x + y*y + z*z > 1.0);
            return new Vector3((float)x, (float)y, (float)z);
        }

        public void CreateBoids(Transform parent, GameObject prefab, int count)
        {
            if (!prefab.GetComponent<BoidParticle>())
            {
                Debug.LogWarning("No BoidParticle component found in the boid prefab");
            }

            System.Random rng = new System.Random(63948);

            for (int i = 0; i < count; ++i)
            {
                GameObject go = UnityEngine.Object.Instantiate(prefab);
                go.transform.parent = parent;
                go.transform.position = sampleUnitSphereVolume(rng) * radius;
                go.transform.rotation = Quaternion.FromToRotation(new Vector3(1, 0, 0), sampleUnitSphereSurface(rng));
                go.transform.localScale.Set(1.0f, 1.0f, 1.0f);

                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb)
                {
                    rb.useGravity = false;
                }
            }
        }
    }
}
