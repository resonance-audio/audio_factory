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

public class Ease {

    static public float InOutSine(float t) {
        return -0.5f * (Mathf.Cos(Mathf.PI * t) - 1.0f);
    }

    static public float Linear(float t) {
        return t;
    }

    static public float InQuad(float t) {
        return t*t;
    }

    static public float OutQuad(float t) {
        return t*(2-t);
    }

    static public float InOutQuad(float t) {
        return t<0.5f ? 2*t*t : -1+(4-2*t)*t;
    }

    static public float InCubic(float t) {
        return t*t*t;
    }

    static public float OutCubic(float t) {
        return (--t)*t*t+1;
    }

    static public float InOutCubic(float t) {
        return t<0.5f ? 4*t*t*t : (t-1)*(2*t-2)*(2*t-2)+1;
    }

    static public float InQuart(float t) {
        return t*t*t*t;
    }

    static public float OutQuart(float t) {
        return 1-(--t)*t*t*t;
    }

    static public float InOutQuart(float t) {
        return t<0.5f ? 8*t*t*t*t : 1-8*(--t)*t*t*t;
    }

    static public float InQuint(float t) {
        return t*t*t*t*t;
    }

    static public float OutQuint(float t) {
        return 1+(--t)*t*t*t*t;
    }

    static public float InOutQuint(float t) {
        return t<0.5f ? 16*t*t*t*t*t : 1+16*(--t)*t*t*t*t;
    }
        
}
