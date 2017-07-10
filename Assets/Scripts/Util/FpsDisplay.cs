/* Copyright 2017 Google Inc. All rights reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */



using System;
using UnityEngine;
using UnityEngine.Events;

public class FpsDisplay : MonoBehaviour {
    [Serializable]
    public class UnityEvent_String : UnityEvent<string> {
    }

    [SerializeField] public UnityEvent_String onUpdate = null;
    [SerializeField] public bool round = false;

    void Update() {
        if(onUpdate != null) {
            float fps = 1f / Time.smoothDeltaTime;
            if(round) fps = Mathf.Round(fps);

            onUpdate.Invoke(fps.ToString());
        }
    }
}