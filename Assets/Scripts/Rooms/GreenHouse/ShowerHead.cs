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

public class ShowerHead : MonoBehaviour {

    bool showerOn;

    BambooGroup bambooGroup;

    ParticleSystem showerSystem;
    ParticleSystem mistSystem;

    float showerTimer = 0.0f;

    float showerEmissionRate = 300.0f;

    float mistEmissionRate = 10.0f;

    GvrAudioSource showerHeadAudio;
    GvrAudioSource showerWaterLeftAudio;
    GvrAudioSource showerWaterRightAudio;
    GvrAudioSource showerPayOff;

    Renderer showerHeadRenderer;

    bool showerHeadReady;
  
    public bool ShowerOn {
        get { return showerOn; }
        set { 
            showerOn = value; 
            showerTimer = Mathf.Clamp01(showerTimer);
            if (showerOn) {
                showerHeadAudio.Play();
                showerWaterLeftAudio.Play();
                showerWaterRightAudio.Play();
                bambooGroup.LightUp();
                showerPayOff.Play();
            }
        }
    }

	void Start () {
        showerSystem = gameObject.GetComponent<ParticleSystem>();
        mistSystem = transform.Find("Mist").GetComponent<ParticleSystem>();

        showerHeadAudio = transform.Find("ShowerSound").GetComponent<GvrAudioSource>();
        showerWaterLeftAudio = transform.Find("ShowerSound/ShowerSprayLeft").GetComponent<GvrAudioSource>();
        showerWaterRightAudio = transform.Find("ShowerSound/ShowerSprayRight").GetComponent<GvrAudioSource>();
        showerPayOff = transform.Find("ShowerSound/ShowerPayOff").GetComponent<GvrAudioSource>();

        ParticleSystem.EmissionModule emission = showerSystem.emission;
        emission.rateOverTime = 0;

        ParticleSystem.EmissionModule mistEmission = mistSystem.emission;
        mistEmission.rateOverTime = 0;

        showerHeadAudio.volume = 0;
        showerWaterLeftAudio.volume = 0;
        showerWaterRightAudio.volume = 0;
	}

    public void AddRenderer(Renderer _showerHeadRenderer) {
        showerHeadRenderer = _showerHeadRenderer;
        SetShowerHeadReady();
    }

    public void AddBamboo(BambooGroup _bambooGroup) {
        bambooGroup = _bambooGroup;
        SetShowerHeadReady();
    }

    void SetShowerHeadReady() {
        showerHeadReady = showerHeadRenderer && bambooGroup;

        if (showerHeadReady)
            Debug.Log(transform.name + " is now ready");
    }
	
	void Update () {

        if (!showerHeadReady)
            return;

        if (showerTimer >= 0.0f && showerTimer <= 1.0f) {

            showerTimer += Time.deltaTime * (showerOn ? 1 : -5);

            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(showerTimer));
            float soundAmount = amount;

            float showerRate =  Mathf.Lerp(0, showerEmissionRate, amount);
            float mistRate =  Mathf.Lerp(0, mistEmissionRate, amount);

            ParticleSystem.EmissionModule emission = showerSystem.emission;
            emission.rateOverTime = showerRate;

            ParticleSystem.EmissionModule mistEmission = mistSystem.emission;
            mistEmission.rateOverTime = mistRate;
            showerHeadAudio.volume = soundAmount;
            showerWaterLeftAudio.volume = soundAmount;
            showerWaterRightAudio.volume = soundAmount;

            if (!showerOn && showerTimer < 0.0f) {
                showerHeadAudio.Stop();
                showerWaterLeftAudio.Stop();
                showerWaterRightAudio.Stop();
            }
            Color color = Color.Lerp(Color.black, Color.white, amount * 2);
            showerHeadRenderer.material.SetColor("_EmissionColor", color);

        }
	}
}
