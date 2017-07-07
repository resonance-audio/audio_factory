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

public class FeederPipe : MonoBehaviour {

    public bool pipeOn;

    float pipeATimer = 2.0f;
    float pipeADuration = 3.0f;
    Material pipeAMat;
    Vector2 scrollAStart = new Vector2(0.0f, -0.5f);
    Vector2 scrollAMid = new Vector2(0.0f, 0.1f);
    Vector2 scrollAEnd = new Vector2(0.0f, 1.0f);

    float pipeBTimer = -1.0f;
    float pipeBDuration = 3.5f;
    Material pipeBMat;
    Vector2 scrollBStart = new Vector2(0.0f, -0.25f);
    Vector2 scrollBEnd = new Vector2(0.0f, 1.0f);


    SeedGeneratorHandle handle;

    void Start () {
        pipeOn = true;
        pipeATimer = 0.99f;

    }

    public void AddPipeObj(Transform _pipeA, Transform _pipeB) {
        pipeAMat = _pipeA.GetComponent<Renderer>().material;
        pipeBMat = _pipeB.GetComponent<Renderer>().material;
    }

    public void TurnOnWater() {

        pipeOn = true;
        pipeATimer = 0;
    }

    public void TurnOffWater() {
        if (!pipeOn)
            return;
        pipeOn = false;
        pipeATimer = 0;        
    }

    public void UsePipe() {
        TurnOffWater();
        pipeBTimer = 0;
    }

    void Update () {

        if (pipeATimer < 1) {
            pipeATimer += Time.deltaTime / pipeADuration;
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(pipeATimer));

            if (pipeOn) {
                pipeAMat.mainTextureOffset = Vector2.Lerp(scrollAStart, scrollAMid, amount);
            } else {
                pipeAMat.mainTextureOffset = Vector2.Lerp(scrollAMid, scrollAEnd, amount);
            }

            if (pipeATimer >= 1) {
                if (pipeOn)
                    handle.IsReady = true;
            } 
        }

        if (pipeBTimer >= 0) {
            pipeBTimer += Time.deltaTime / pipeBDuration;
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(pipeBTimer));
            pipeBMat.mainTextureOffset = Vector2.Lerp(scrollBStart, scrollBEnd, amount);
            if (pipeBTimer > 1)
                pipeBTimer = -1;
        }
    }

    public void AddHandle(SeedGeneratorHandle _handle) {
        handle = _handle;
    }
}
