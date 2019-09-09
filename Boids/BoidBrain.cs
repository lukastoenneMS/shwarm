// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace Shwarm.Boids
{
    [System.Serializable]
    public class BoidSettings
    {
        /// <summary>
        /// Consider boids as neighbors when they are closer.
        /// </summary>
        public float InteractionRadius = 1.0f;

        /// <summary>
        /// Priority offset for the current rule to prevent immediate switching
        /// </summary>
        public float CurrentRuleBias = 0.0f;
    }

    [System.Serializable]
    public class BoidBrain : MonoBehaviour
    {
        [SerializeField]
        private BoidSettings settings = new BoidSettings();
        public BoidSettings Settings => settings;

        [SerializeField]
        private List<BoidRule> rules = new List<BoidRule>();
        public List<BoidRule> Rules => rules;

        private readonly List<BoidTarget> ruleTargets = new List<BoidTarget>();
        private readonly List<float> rulePriorities = new List<float>();

        private BoidContext context;

        public void Awake()
        {
            context = new BoidContext(settings);

            context.UpdateBoidParticles(GetComponentsInChildren<BoidParticle>());

            foreach (BoidRule rule in rules)
            {
                if (rule)
                {
                    rule.OnAwake();
                }
            }
        }

        public void OnDestroy()
        {
            foreach (BoidRule rule in rules)
            {
                if (rule)
                {
                    rule.OnDestroy();
                }
            }
        }

        public void OnTransformChildrenChanged()
        {
            context.UpdateBoidParticles(GetComponentsInChildren<BoidParticle>());
        }

        void FixedUpdate()
        {
            ApplyRules();
        }

        private void ApplyRules()
        {
            int numBoids = context.Boids.Length;

            context.Prepare();

            ruleTargets.Clear();
            rulePriorities.Clear();
            ruleTargets.Capacity = rules.Count;
            rulePriorities.Capacity = rules.Count;
            foreach (BoidRule rule in rules)
            {
                if (rule)
                {
                    rule.Prepare();
                }
                ruleTargets.Add(null);
                rulePriorities.Add(-1.0f);
            }

            for (int boidIndex = 0; boidIndex < context.Boids.Length; ++boidIndex)
            {
                BoidParticle boid = context.Boids[boidIndex];

                BoidState state = context.States[boidIndex];

                for (int ruleIndex = 0; ruleIndex < rules.Count; ++ruleIndex)
                {
                    BoidRule rule = rules[ruleIndex];
                    if (rule && rule.Evaluate(context, boid, boidIndex, state, out BoidTarget target, out float priority))
                    {
                        ruleTargets[ruleIndex] = target;
                        rulePriorities[ruleIndex] = priority;
                    }
                    else
                    {
                        ruleTargets[ruleIndex] = null;
                        rulePriorities[ruleIndex] = -1.0f;
                    }
                }

                // BoidTarget newTarget = SelectTargetByPriority(boid.CurrentRuleIndex);
                BoidTarget newTarget = SelectTargetByWeightedAverage(state, boid.CurrentRuleIndex);

                boid.ApplyPhysics(state, newTarget);

                BoidDebug.SetTarget(boid, state, newTarget);
            }

            context.Cleanup();
            foreach (BoidRule rule in rules)
            {
                if (rule)
                {
                    rule.Cleanup();
                }
            }
        }

        private BoidTarget SelectTargetByPriority(int currentRuleIndex)
        {
            BoidTarget newTarget = null;
            float maxPriority = -1.0f;
            for (int ruleIndex = 0; ruleIndex < rules.Count; ++ruleIndex)
            {
                BoidTarget target = ruleTargets[ruleIndex];
                if (target != null)
                {
                    float priority = rulePriorities[ruleIndex];
                    if (ruleIndex == currentRuleIndex)
                    {
                        // Add bias to the current rule's importance to avoid immediate switching
                        priority += settings.CurrentRuleBias;
                    }

                    if (priority > maxPriority)
                    {
                        maxPriority = priority;
                        newTarget = target;
                    }
                }
            }

            return newTarget;
        }

        private BoidTarget SelectTargetByWeightedAverage(BoidState state, int currentRuleIndex)
        {
            BoidTarget newTarget = null;
            float totweight = 0.0f;
            for (int ruleIndex = 0; ruleIndex < rules.Count; ++ruleIndex)
            {
                BoidTarget target = ruleTargets[ruleIndex];
                if (target != null)
                {
                    float priority = rulePriorities[ruleIndex];
                    if (ruleIndex == currentRuleIndex)
                    {
                        // Add bias to the current rule's importance to avoid immediate switching
                        priority += settings.CurrentRuleBias;
                    }

                    // Exponential weight based on priority
                    float weight = Mathf.Exp(priority);
                    totweight += weight;
                    if (newTarget == null)
                    {
                        newTarget = target;
                    }
                    else
                    {
                        newTarget.direction += target.direction * weight;
                        newTarget.speed += target.speed * weight;
                    }
                }
            }
            if (totweight > 0.0f)
            {
                newTarget.direction.Normalize();
                newTarget.speed /= totweight;
            }

            return newTarget;
        }
    }
}
