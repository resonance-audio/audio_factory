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

public class Pipe : MonoBehaviour {

    public bool pipeOn;

    float pipeTimer = -1.0f;
    float pipeDuration = 3.0f;

    Material pipeMat;

    SplinePath pipePath;

    GvrAudioSource pipeSound;
    Transform pipeSoundPos;

    ShowerHead showerHead;

    bool pipeReady;
    bool pipeDone;

    public AnimationCurve pipeVolume;


    public bool PipeOn {
        get { return pipeOn; }
        set { 
            pipeOn = value;

            if (pipeOn) {
                pipeTimer = 0.0f;
                pipeMat.mainTextureOffset = new Vector2(0, 1);
                pipeSound.Play();
            } else {
                pipeSound.Stop();
            }
        }
    }

    void Start () {
        pipeSound = GetComponent<GvrAudioSource>();
    }

    public void AddMaterials(Material newMat) {
        pipeMat = newMat;
        SetPipeReady();
    }

    public void AddPaths(SplinePath newPath) {
        pipePath = newPath;
        SetPipeReady();
    }

    public void AddShowerHead(ShowerHead _showerHead) {
        showerHead = _showerHead;
        SetPipeReady();
    }

    void SetPipeReady() {
        pipeReady = pipeMat && pipePath && showerHead;

        if (pipeReady)
            Debug.Log(transform.name + " is now ready");
    }

    void Update () {
        if (!pipeReady)
            return;
        
        if (pipeOn && pipeTimer >= 0.0f && pipeTimer <= 1.0f) {
            pipeTimer += Time.deltaTime / pipeDuration;

            if (pipePath)
                transform.position = pipePath.Evaluate(Mathf.Clamp01(pipeTimer));

            float scrollAmount = Mathf.Lerp(1, -0.1f, pipeTimer);
            pipeMat.mainTextureOffset = new Vector2(0, scrollAmount);

            if (pipeSound)
                pipeSound.volume = pipeVolume.Evaluate(pipeTimer);
            
            if (!pipeDone && pipeTimer > 1.0f)
                PipeComplete();
        }

        Color pipeColor = Color.Lerp(pipeMat.GetColor("_EmissionColor"), pipeOn ? Color.white : Color.black, Time.deltaTime * 3);
        pipeMat.SetColor("_EmissionColor", pipeColor);
    }

    void PipeComplete() {
        showerHead.ShowerOn = true;
        pipeDone = true;
    }
}
