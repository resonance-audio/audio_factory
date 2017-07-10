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

public class AudioColorScaler : MonoBehaviour {
    [SerializeField] public MaterialPropertySetter target = null;
    [SerializeField] public AudioSampler sampler = null;
    [SerializeField] public Color baseColor = Color.white;
    [SerializeField] public Color color = Color.white;
    [SerializeField] public AnimationCurve curve = null;
    [SerializeField] public float multiplier = 1f;

    void Update() {
        if(target == null || sampler == null || curve == null) return;
        float sampledValue = sampler.CurrentSampleAverage;
        float curveValue = curve.Evaluate(sampledValue);
        target.value_color = baseColor + color * multiplier * curveValue;
    }
}