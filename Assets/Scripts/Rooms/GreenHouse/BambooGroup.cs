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

public class BambooGroup : MonoBehaviour {

    public Material matStalk;
    public Material matLeaves;

    public GreenhouseControl control;

    public ParticleSystem fireFlies;
    GvrAudioSource fireFlyLeftSound;
    GvrAudioSource fireFlyRightSound;

    PlantGroup plantGroup;

    ShowerHead showerHead;

    AnimationCurve leafFade;

    float stalkTimer = -2;
    float timeForStalks = 20;
    float darkenTime = 1.0f;

    bool pastHalfWay;
    bool done;

    int groupID;

    void Start() {
        matStalk.SetColor("_EmissionColor", Color.black);
        matLeaves.SetColor("_EmissionColor", Color.black);
        groupID = int.Parse(transform.name.Substring(transform.name.Length - 1)) - 1;

        fireFlyLeftSound = fireFlies.transform.Find("FireFlyLeft").GetComponent<GvrAudioSource>();
        fireFlyRightSound = fireFlies.transform.Find("FireFlyRight").GetComponent<GvrAudioSource>();
    }

    public void AddShowerHead(ShowerHead _showerHead)
    {
        showerHead = _showerHead;
    }

    public void AddLeafFadeCurve(AnimationCurve fade) {
        leafFade = fade;
    }
	
	void Update () {

        if (stalkTimer >= 0.0f && stalkTimer <= 1.0f) {
            stalkTimer += Time.deltaTime /  (stalkTimer < 0.5f ? timeForStalks : timeForStalks * 0.25f);

            float stalkAmount = Mathf.Lerp(1.0f, -1.0f, Mathf.Clamp01(stalkTimer));
            Color stalkColor = Color.Lerp(Color.black, Color.white, Mathf.Clamp01(stalkTimer * 3));
            matStalk.SetColor("_EmissionColor", stalkColor);
            matStalk.mainTextureOffset = new Vector2 (0, stalkAmount);
            float leafAmount = Mathf.Lerp(0.0f, 1.0f, Mathf.Clamp01((stalkTimer * 2) - 0.5f));
            matLeaves.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white, leafFade.Evaluate(leafAmount)));

            if (!pastHalfWay && stalkTimer > 0.5f){
                HalfwayDone();
            }

            if (!done && stalkTimer > 1.0)
                Done();
        }

        if (pastHalfWay) {
            darkenTime -= Time.deltaTime / 5.0f;
            Color darkColor = Color.Lerp(Color.white, Color.black, 1 - darkenTime);
            matStalk.SetColor("_EmissionColor", darkColor);
        }
	}

    public void LightUp() {
        stalkTimer = 0.0f;
    }

    void HalfwayDone() {
        pastHalfWay = true;
        showerHead.ShowerOn = false;
        plantGroup.LightUp();
        control.BambooDone(groupID);
    }

    public void AddPlants(PlantGroup group) {
        plantGroup = group;
    }

    void Done() {
        done = true;
        stalkTimer = -2;
        fireFlies.Play();
        fireFlyLeftSound.Play();
        fireFlyRightSound.Play();

    }

    void OnApplicationQuit() {
        matStalk.SetColor("_EmissionColor", Color.white);
        matLeaves.SetColor("_EmissionColor", Color.white);    
        matStalk.mainTextureOffset = new Vector2 (0, -1);
    }
}
