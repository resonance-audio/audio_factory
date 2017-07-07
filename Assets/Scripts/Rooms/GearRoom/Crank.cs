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

namespace AudioEngineer.Rooms.Gears {
    public class Crank : MonoBehaviour {
        [SerializeField] Transform masterRotation = null;
        [SerializeField] float initialOffset = 0f;
        [SerializeField] float multiplier = 1f;
        [SerializeField] Animator targetAnimator = null;
        [SerializeField] RotationMode mode = default(RotationMode);
        [SerializeField] bool worldRotation = false;
        [SerializeField] string animStateName = "main";
        [SerializeField] CompletedCrank[] completed = null;

        [NonSerialized] float lastTime = 0f;

        public enum RotationMode {
            X = 0,
            Y = 1,
            Z = 2,
        }

        void Update() {
            if(masterRotation == null || targetAnimator == null) return;

            float normalizedTime = initialOffset + ReadRotation() * multiplier;
            float timeSpeed = (normalizedTime - lastTime)/Time.deltaTime;
            lastTime = normalizedTime;

            if(completed != null) {
                foreach(var comp in completed) {
                    if(comp != null) {
                        comp.startSpeed = timeSpeed;
                    }
                }
            }
            targetAnimator.Play(animStateName, 0, normalizedTime);
        }

        float ReadRotation() {
            Vector3 vec = (worldRotation ? masterRotation.eulerAngles : masterRotation.localEulerAngles);
            switch(mode){
                default: 
                case RotationMode.X : return vec.x;
                case RotationMode.Y : return vec.y;
                case RotationMode.Z : return vec.z;
            }
        }
    }
}