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

namespace AudioEngineer.Rooms.Basement {
    public class CompletionPillar : MonoBehaviour {
        [SerializeField] Color startColor;
        [SerializeField] Color endColor;
        [SerializeField] Transform center;
        [SerializeField] float timeScale = 1f;
        [SerializeField] AnimationCurve curve;

        [NonSerialized] float currentTime = 0f;
        [NonSerialized] MaterialPropertySetter setter;

        void OnEnable() {
            currentTime = Mathf.Atan2(center.position.y - transform.position.y, center.position.x - transform.position.x);
            currentTime = currentTime / Mathf.PI * 0.5f;
            setter = GetComponentInChildren<MaterialPropertySetter>();
        }
        void Update(){
            currentTime += timeScale * Time.deltaTime;
            float t = curve.Evaluate(currentTime);
            curve.preWrapMode = WrapMode.Loop;
            curve.postWrapMode = WrapMode.Loop;
            setter.value_color = Color.Lerp(startColor, endColor, t);
        }
    }
}