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
using System.Collections.Generic;

public class PlantGroup : MonoBehaviour {

    Material plantMat;

    float plantTimer = -2;
    float timeForPlant = 3;


    void Start() {
        plantMat = gameObject.GetComponent<Renderer>().material;
    }

    void Update () {
        if (plantTimer >= 0.0f && plantTimer <= 1.0f) {

            plantTimer += Time.deltaTime / timeForPlant;

            Color leafColor = Color.Lerp (Color.black, Color.white, plantTimer);
            plantMat.SetColor("_EmissionColor", leafColor);

            if (plantTimer > 1.0)
                Done();
        }
    }


    public void LightUp() {
        plantTimer = 0.0f;
    }

    void Done() {

    }
  
    void OnApplicationQuit() {
        plantMat.SetColor("_EmissionColor", Color.black);
    }
}
