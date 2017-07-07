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

using UnityEngine;

//Converts a rigidbody's velocity to the pitch property on a GvrAudioSource
public class VelocityToPitch : MonoBehaviour {
    [SerializeField] public Rigidbody source = null;
    [SerializeField] public GvrAudioSource target = null;
    [SerializeField] public float baseValue = 1f;
    [SerializeField] public Vector3 velocityMultiplier = Vector3.one;

    void Update() {
        if(source != null && target != null) {
            float mag = baseValue + Vector3.Scale(source.velocity, velocityMultiplier).magnitude;
            target.pitch = mag;
        }
    }
}