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
using System.Collections.Generic;

public class ElevatorLightControl : MonoBehaviour {

    List<ElevatorLightRing> lightRings = new List<ElevatorLightRing>();

    public float lightInterval = 0.25f;
    public float lightDuration = 0.5f;
    public float lightFadeTime = 0.5f;
    public float lightRingDelay = 1.0f;
    public bool doubleLights = false;

    public bool useAbsoluteDelays;
    public float[] absoluteDelays;
    public float volumeMultiplier = 1f;

    bool isOn = true;

    public string pathToLights;
    public ElevatorLight.StereoClip[] LightSounds;

    public float lowerRange = 6.0f;
    public float upperRange = 6.0f;

    // 1=.125
    //2= .375;

    public bool IsOn
    {
        get { return isOn; }
        set { 
            if (isOn == value)
                return;
            
            isOn = value;
            fadeTimer = Mathf.Clamp01(fadeTimer);
        }
    }

    float fadeTimer = 0.0f;
    float fadeDuration = 3.0f;

    public float Volume {
        get { return Mathf.Clamp01(fadeTimer) * volumeMultiplier; }
    }

	void Start () {

        IsOn = false;

        bool noLightRing = false;
        int ringIndex = 0;
        Transform lightRingObj;
        ElevatorLightRing newLightRing;

        while (!noLightRing)
        {
            ringIndex++;
            lightRingObj = transform.Find(string.Format("{0}/lights/light_ring_{1}", pathToLights, ringIndex.ToString()));
            if (!lightRingObj) {
                noLightRing = true;
                break;
            }
            newLightRing = lightRingObj.gameObject.AddComponent<ElevatorLightRing>();
            newLightRing.Interval = lightInterval;
            newLightRing.Duration = lightDuration;
            newLightRing.FadeTime = lightFadeTime;
            newLightRing.DoubleLights = doubleLights;

            if (useAbsoluteDelays) {
                newLightRing.StartDelay = absoluteDelays[Mathf.Clamp(ringIndex - 1, 0, absoluteDelays.Length -1)];
            } else {
                newLightRing.StartDelay = lightRingDelay * (ringIndex - 1);
            }

            newLightRing.Init(this, LightSounds);
            lightRings.Add(newLightRing);
        }
	}
	
	void Update () {
        float cameraHeight = Camera.main.transform.position.y;
        IsOn = cameraHeight > transform.position.y - lowerRange && cameraHeight < transform.position.y + upperRange;

        if (fadeTimer >= 0.0f && fadeTimer <= 1.0f) {
            fadeTimer += Time.deltaTime / (isOn ? fadeDuration : -fadeDuration);
        }
	}
}
