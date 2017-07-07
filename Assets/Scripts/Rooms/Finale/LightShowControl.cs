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
using UnityEngine.Events;


public class LightShowControl : MonoBehaviour {
    

    float lightRange = 30;

    float playTime = 1.5f;

    public Material lightsMat;
    public Material lightGlowMat;

    public AudioSampler[] audioSamplers;

    public float volumeThreshold = 0.01f;

    float coolDown = 0.5f;
    float coolDownTimer;

    public bool autoAssign;
    public string instrumentPath;

    public bool debugLog;

    [System.Serializable]
    public class LightWave {
        public bool inUse = false;
        public float timer = -1.0f;
        public int id = 0;
        public LightWave(int _id) {
            this.id = _id;
        }
    }

    LightWave[] lightWaves;

	void Start () {
        lightsMat.mainTextureOffset = new Vector2(0,0);

        lightWaves = new LightWave[4];
        lightWaves[0] = new LightWave(0);
        lightWaves[1] = new LightWave(1);
        lightWaves[2] = new LightWave(2);
        lightWaves[3] = new LightWave(3);

        if (autoAssign && instrumentPath.Length > 0) {
            GameObject instrument = GameObject.Find(instrumentPath);
            if (instrument) {
                OnEnterWhileClick[] eventHandlers = instrument.GetComponentsInChildren<OnEnterWhileClick>();
                foreach(OnEnterWhileClick eventHandler in eventHandlers) {
                    UnityAction newAction = new UnityAction(PlayLights);
                    eventHandler.AddListener(newAction);
                }
            } else {
                Debug.Log("Can't find instrument");
            }
        }
	}
	
	void Update () {

        foreach (LightWave lightWave in lightWaves) {
            if (lightWave.inUse) {
                if (lightWave.timer >= 0.0f) {
                    lightWave.timer += Time.deltaTime / playTime;
                    lightsMat.SetFloat("_Distance" + (lightWave.id + 1), lightWave.timer * (lightRange + lightRange));
                    lightGlowMat.SetFloat("_Distance" + (lightWave.id + 1), lightWave.timer * (lightRange + lightRange));
                    if (lightWave.timer > 1.0f) {
                        StopLights(lightWave.id);
                    }
                }                
            }
        }

        if (audioSamplers.Length > 0) {
            foreach (AudioSampler sampler in audioSamplers) {
                float sample = sampler.CurrentSampleAverage;
                if (debugLog)
                    Debug.Log(sample);
                if (sample > volumeThreshold)
                    PlayLights();
            }
        }

        if (coolDownTimer > 0) {
            coolDownTimer -= Time.deltaTime;
        }
	}

    public void PlayLights() {
        if (coolDownTimer > 0) 
            return;
        
        coolDownTimer = coolDown;
        foreach (LightWave lightWave in lightWaves) {
            if (!lightWave.inUse) {
                lightWave.inUse = true;
                lightWave.timer = 0.0f;
                break;
            }
        }
    }

    void StopLights(int id) {
        lightWaves[id].inUse = false;
        lightWaves[id].timer = -1.0f;
    }

    void OnApplicationQuit() {

        lightsMat.SetFloat("_Distance1", 0);
        lightsMat.SetFloat("_Distance2", 0);
        lightsMat.SetFloat("_Distance3", 0);
        lightsMat.SetFloat("_Distance4", 0);
        lightGlowMat.SetFloat("_Distance1", 0);
        lightGlowMat.SetFloat("_Distance2", 0);
        lightGlowMat.SetFloat("_Distance3", 0);
        lightGlowMat.SetFloat("_Distance4", 0);
    }
}
