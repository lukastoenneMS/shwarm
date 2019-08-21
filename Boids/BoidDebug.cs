// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;
using Shwarm.Vdb;

namespace Boids
{
    public static class BoidDebug
    {
        private static bool enableTarget = false;
        private static bool enablePhysics = false;
        private static bool enableCollision = false;
        private static bool enableSwarm = false;
        private static bool enableBoidCollision = false;

        private static Transform debugObjects = null;

        private static VisualRecorder recorder = VisualRecorder.Instance;

        // System.String.GetHashCode(): http://referencesource.microsoft.com/#mscorlib/system/string.cs,0a17bbac4851d0d4
        // System.Web.Util.StringUtil.GetStringHashCode(System.String): http://referencesource.microsoft.com/#System.Web/Util/StringUtil.cs,c97063570b4e791a
        private static int CombineHashCodes(IEnumerable<int> hashCodes)
        {
            int hash1 = (5381 << 16) + 5381;
            int hash2 = hash1;

            int i = 0;
            foreach (var hashCode in hashCodes)
            {
                if (i % 2 == 0)
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ hashCode;
                else
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ hashCode;

                ++i;
            }

            return hash1 + (hash2 * 1566083941);
        }

        private static int CombineHashCodes(int hashCodeA, int hashCodeB)
        {
            return CombineHashCodes(new int[] {hashCodeA, hashCodeB});
        }

        private static int CombineHashCodes(int hashCodeA, int hashCodeB, int hashCodeC)
        {
            return CombineHashCodes(new int[] {hashCodeA, hashCodeB, hashCodeC});
        }

        private static int CombineHashCodes(int hashCodeA, int hashCodeB, int hashCodeC, int hashCodeD)
        {
            return CombineHashCodes(new int[] {hashCodeA, hashCodeB, hashCodeC, hashCodeD});
        }

        public static int GetBoidId(BoidParticle particle)
        {
            return CombineHashCodes("BoidParticle".GetHashCode(), particle.GetInstanceID());
        }

        public static void SetTarget(BoidParticle particle, BoidState state, BoidTarget target)
        {
            if (!enableTarget || !particle.EnableDebugObjects)
            {
                return;
            }

            var debugTarget = GetOrCreatePooled("Target", PrimitiveType.Cube);
            var debugTargetDirection = GetOrCreatePooled("TargetDirection", PrimitiveType.Cube);

            if (target != null)
            {
                debugTarget.gameObject.SetActive(true);
                debugTargetDirection.gameObject.SetActive(true);
                debugTarget.position = target.direction;
                SetTransformVector(debugTargetDirection, state.position, target.direction, 0.01f);

                // Color color = Color.white * (1.0f - force) + Color.green * force;
                Color color = Color.green;
                debugTarget.GetComponent<Renderer>().material.color = color;
                debugTargetDirection.GetComponent<Renderer>().material.color = color;
            }
            else
            {
                debugTarget.gameObject.SetActive(false);
                debugTargetDirection.gameObject.SetActive(false);
            }
        }

        public static void SetPhysics(BoidParticle particle, BoidState state, Vector3 force, Vector3 torque)
        {
            recorder.RecordData(GetBoidId(particle), state);

            if (!enablePhysics || !particle.EnableDebugObjects)
            {
                return;
            }

            var forceOb = GetOrCreatePooled("Force", PrimitiveType.Cube);
            var torqueOb = GetOrCreatePooled("Torque", PrimitiveType.Cube);

            SetTransformDirection(forceOb, state.position, force, 0.01f);
            SetTransformDirection(torqueOb, state.position, torque, 0.01f);

            forceOb.GetComponent<Renderer>().material.color = Color.red;
            torqueOb.GetComponent<Renderer>().material.color = Color.blue;

            // {
            //     var dbgRoll = GetOrCreate("Roll", PrimitiveType.Cube);
            //     // float mix = state.roll / 360.0f;
            //     float mix = deltaRoll / 360.0f;
            //     SetTransformDirection(dbgRoll, state.position, Vector3.up * mix, 0.01f);
            //     Color color = Color.red * (1.0f - mix) + Color.yellow * mix;
            //     dbgRoll.GetComponent<Renderer>().material.color = color;
            // }
        }

        public static void AddSwarmPoint(BoidParticle particle, BoidState state, Vector3 point, float weight)
        {
            if (!enableSwarm || !particle.EnableDebugObjects)
            {
                return;
            }

            var swarm = GetOrCreatePooled("Swarm", PrimitiveType.Cube);

            SetTransformVector(swarm, state.position, point, 0.01f);

            Color color = Color.blue * (1.0f - weight) + Color.red * weight;
            swarm.GetComponent<Renderer>().material.color = color;
        }

        public static void AddCollisionPoint(BoidParticle particle, BoidState state, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (!enableCollision || !particle.EnableDebugObjects)
            {
                return;
            }

            var collisionPoint = GetOrCreatePooled("CollisionPoint", PrimitiveType.Cube);
            var collisionNormal = GetOrCreatePooled("CollisionNormal", PrimitiveType.Cube);

            SetTransformVector(collisionPoint, state.position, hitPoint, 0.005f);
            SetTransformDirection(collisionNormal, hitPoint, hitNormal, 0.01f);

            collisionPoint.GetComponent<Renderer>().material.color = Color.cyan;
            collisionNormal.GetComponent<Renderer>().material.color = Color.yellow;
        }

        public static void AddBoidCollisionCone(BoidParticle particle, BoidState state, Vector3 dir, Vector3 colliderDir, float radius)
        {
            if (!enableBoidCollision || !particle.EnableDebugObjects)
            {
                return;
            }

            var collisionFwd = GetOrCreatePooled("CollisionFwd", PrimitiveType.Cube);
            var collisionDir = GetOrCreatePooled("CollisionDir", PrimitiveType.Cube);
            var collisionSphere = GetOrCreatePooled("CollisionSphere", PrimitiveType.Sphere);

            SetTransformDirection(collisionFwd, state.position, dir, 0.01f);
            SetTransformDirection(collisionDir, state.position, colliderDir, 0.005f);
            collisionSphere.position = state.position + colliderDir;
            collisionSphere.localScale = Vector3.one * radius * 2.0f;

            collisionFwd.GetComponent<Renderer>().material.color = Color.cyan;
            collisionDir.GetComponent<Renderer>().sharedMaterial = (Material)Resources.Load("DebugCollision", typeof(Material));
            collisionSphere.GetComponent<Renderer>().sharedMaterial = (Material)Resources.Load("DebugCollision", typeof(Material));
        }

        public static void AddCustomVector(Vector3 from, Vector3 to, Color color)
        {
            var ob = GetOrCreatePooled("CustomVector", PrimitiveType.Cube);

            SetTransformVector(ob, from, to, 0.01f);

            ob.GetComponent<Renderer>().material.color = color;
        }

        public static void AddCustomDirection(Vector3 origin, Vector3 direction, Color color)
        {
            var ob = GetOrCreatePooled("CustomDirection", PrimitiveType.Cube);

            SetTransformDirection(ob, origin, direction, 0.01f);

            ob.GetComponent<Renderer>().material.color = color;
        }

        public static void ClearAll()
        {
            if (debugObjects)
            {
                for (int i = 0; i < debugObjects.childCount; ++i)
                {
                    var child = debugObjects.GetChild(i);
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static Transform GetOrCreatePooled(string name, PrimitiveType prim)
        {
            EnsureDebugObjects();

            for (int i = 0; i < debugObjects.childCount; ++i)
            {
                var child = debugObjects.GetChild(i);
                if (child.name == name && !child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                    return child;
                }
            }

            return CreateDebugPrimitive(name, prim);
        }

        private static void ClearPool(string name)
        {
            if (debugObjects)
            {
                for (int i = 0; i < debugObjects.childCount; ++i)
                {
                    var child = debugObjects.GetChild(i);
                    if (child.name == name)
                    {
                        child.gameObject.SetActive(false);
                    }
                }
            }
        }

        private static void SetTransformVector(Transform dbg, Vector3 from, Vector3 to, float size)
        {
            Vector3 direction = to - from;
            dbg.position = 0.5f * (from + to);
            dbg.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
            dbg.localScale = new Vector3(size, size, direction.magnitude);
        }

        private static void SetTransformDirection(Transform dbg, Vector3 origin, Vector3 direction, float size)
        {
            dbg.position = origin + 0.5f * direction;
            dbg.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
            dbg.localScale = new Vector3(size, size, direction.magnitude);
        }

        private static Transform CreateDebugPrimitive(string name, PrimitiveType prim)
        {
            var dbg = GameObject.CreatePrimitive(prim).transform;
            // We don't want a collider on debug objects
            dbg.GetComponent<Collider>().enabled = false;
            dbg.name = name;
            dbg.parent = debugObjects;
            dbg.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            return dbg;
        }

        private static Transform EnsureDebugObjects()
        {
            if (!debugObjects)
            {
                debugObjects = new GameObject("Debugging").transform;
            }
            return debugObjects;
        }
    }
}
