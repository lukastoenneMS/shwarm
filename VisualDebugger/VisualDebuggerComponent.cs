// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace Shwarm.Vdb
{
    [Serializable]
    public class VisualDebuggerComponent : MonoBehaviour
    {
        private VisualRecorder recorder;

        [SerializeField]
        private int currentFrame = 0;
        public int CurrentFrame { get => currentFrame; set => currentFrame = value; }

        void Awake()
        {
            recorder = VisualRecorder.Instance;
        }

        void Start()
        {
            recorder.BeginKeyframe();
        }

        void FixedUpdate()
        {
            currentFrame = recorder.CommitKeyframe();

            recorder.BeginKeyframe();
        }

        void OnEnable()
        {
            recorder.BeginKeyframe();
        }

        void OnDisable()
        {
            recorder.DiscardKeyframe();
        }

        public void ClearKeyframes()
        {
            VisualDebugger.Instance.ClearKeyframes();
            currentFrame = 0;
        }
    }
}
