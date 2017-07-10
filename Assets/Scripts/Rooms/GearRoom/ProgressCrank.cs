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

namespace AudioEngineer.Rooms.Gears {

    //Component which increments a progress-value as a lever is turned
    public class ProgressCrank : MonoBehaviour {

#region SERIALIZED_FIELDS
        [SerializeField] public InteractableLever lever = null;
        [SerializeField] public float crankMultiplier = 1f;
        [SerializeField] public float requiredValue = 360f;
        [SerializeField] public float valueDecay = 90f;
        [SerializeField] public UnityEvent onReachedRequiredValue = default(UnityEvent);
        [SerializeField] float currentValue = 0f;
        [SerializeField] public Transform progressGauge = null;
        [SerializeField] public Vector3 progressGaugeStart = Vector3.zero;
        [SerializeField] public Vector3 progressGaugeEnd = Vector3.zero;
#endregion SERIALIZED_FIELDS

#region NON_SERIALIZED_FIELDS
        [NonSerialized] float? lastAngle = null;
        [NonSerialized] bool hasReachedRequiredValue = false;
#endregion NON_SERIALIZED_FIELDS

#region METHODS
        void Update() {
            if(lever == null) return;
            if(crankMultiplier == 0f) return;

            //update the progress gauge
            if(progressGauge != null) {
                if(hasReachedRequiredValue){
                    progressGauge.localEulerAngles = progressGaugeEnd;
                }else{
                    float progressLerp = Mathf.Abs(currentValue / requiredValue);
                    progressGauge.localEulerAngles = Vector3.Lerp(progressGaugeStart, progressGaugeEnd, progressLerp);
                }
            }

            if(!hasReachedRequiredValue) {

                float decay = valueDecay * Time.deltaTime;
                float currAngle = lever.Angle;
                if(lastAngle == null) lastAngle = currAngle;
                float angleDelta = Mathf.DeltaAngle(lastAngle.Value, currAngle);
                
                currentValue += crankMultiplier * angleDelta;
                lastAngle = currAngle;

                if(requiredValue > 0f) {
                    currentValue -= decay;
                    if(currentValue < 0f) {
                        currentValue = 0f;
                    }
                    if(currentValue >= requiredValue) {
                        hasReachedRequiredValue = true;
                    }
                }else if(requiredValue < 0f) {
                    currentValue += decay;
                    if(currentValue > 0f) {
                        currentValue = 0f;
                    }
                    if(currentValue <= requiredValue) {
                        hasReachedRequiredValue = true;
                    }
                }

                if(hasReachedRequiredValue && onReachedRequiredValue != null) {
                    onReachedRequiredValue.Invoke();
                }
            }
        }
    }
#endregion METHODS
}