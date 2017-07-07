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

public class CurveTest : MonoBehaviour {

 
    public AnimationCurve curve;
    void Start () {
        foreach (Keyframe key in curve.keys) {
            Debug.Log(string.Format("Key: time: {0}, value: {1}, in: {2}, out: {3}", key.time, key.value, key.inTangent, key.outTangent ));

        }
	}
}
