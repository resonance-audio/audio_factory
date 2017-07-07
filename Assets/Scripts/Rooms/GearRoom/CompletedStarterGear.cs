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

﻿using UnityEngine;
using System.Collections;

public class CompletedStarterGear : MonoBehaviour {


    public Renderer[] gears;

    public AnimationCurve flashCurve;

    Color startColor = Color.blue;
    public Color flashColor = new Color(1.0f, 0.85f, 0.45f);
    float flashOffset = 0.1f;

    float timer = 0.0f;
    float duration = 2.0f;

	void Start () {
        startColor = gears[0].material.GetColor("_EmissionColor");
	
	}
	
	void Update () {
        if (timer < 1 + (gears.Length * flashOffset)) {
            timer += Time.deltaTime / duration;
            for(int i = 0; i < gears.Length; i++) {
                float amount = flashCurve.Evaluate(timer - (i*flashOffset));
                gears[i].material.SetColor("_EmissionColor", Color.Lerp(startColor, flashColor, amount));
            }
        }
	}
}
