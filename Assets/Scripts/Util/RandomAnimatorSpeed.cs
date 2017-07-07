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
using System;
using System.Collections.Generic;

public class RandomAnimatorSpeed : MonoBehaviour {
    [SerializeField] public Vector2 speedBounds = new Vector2(1f, 1f);
    [SerializeField] public Vector2 accelBounds = new Vector2(-1f, 1f);
    [SerializeField] public float sharpness = 1f;

    [NonSerialized] float accel = 0f;

    void Update() {
        var animator = GetComponent<Animator>();
        if(animator == null) return;

        accel += UnityEngine.Random.Range(-sharpness, sharpness);
        accel = Mathf.Clamp(accel, accelBounds.x, accelBounds.y);
        animator.speed = Mathf.Clamp(animator.speed+accel, speedBounds.x, speedBounds.y);
    }
}