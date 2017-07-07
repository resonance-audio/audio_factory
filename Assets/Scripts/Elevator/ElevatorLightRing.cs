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

public class ElevatorLightRing : MonoBehaviour {

    List<ElevatorLight> lights = new List<ElevatorLight>();
    ElevatorLightControl control;

    int lightRingId = 0;
    int lightIndex = 0;
    float lightTimer = 1.0f;

    float startDelay = 0;

    float interval = 1.0f;
    float duration = 0.5f;
    float fadeTime = 0.5f;
    bool doubleLights;

    ElevatorLight.StereoClip[] lightTones;

    public float Interval
    {
        set { interval = value; }
    }

    public float Duration
    {
        set { duration = value; }
    }

    public float FadeTime
    {
        set { fadeTime = value; }
    }

    public float StartDelay
    {
        set { startDelay = value; }
    }

    public bool DoubleLights
    {
        set { doubleLights = value; }
    }

    public void Init (ElevatorLightControl _control, ElevatorLight.StereoClip[] sounds) {
        control = _control;
        lightTones = sounds;

        lightRingId = int.Parse(transform.name.Substring(transform.name.Length - 1));
        AddLights();
	}

    void AddLights()
    {
        bool noLight = false;
        int lightNumber = 0;
        Transform newLightObj;
        ElevatorLight newLight;
        while (!noLight)
        {
            lightNumber++;
            newLightObj = transform.Find(string.Format("light_{0}_{1}", lightRingId.ToString(), lightNumber.ToString()));
            if (!newLightObj)
            {
                noLight = true;
                break;
            }
            newLight = newLightObj.gameObject.AddComponent<ElevatorLight>();
            newLight.Duration = duration;
            newLight.FadeTime = fadeTime;
            newLight.Init(control, lightTones[(lightNumber + lightRingId)%lightTones.Length], lightRingId, lightNumber);
            lights.Add(newLight);
        }
    }
	
	void Update () {
        if (control.Volume < 0.05f)
            return;

        if (startDelay > 0)
        {
            startDelay -= Time.deltaTime;
            return;
        }

        lightTimer -= Time.deltaTime;
        if (lightTimer < 0) {
            lightIndex = (lightIndex + 1) % lights.Count;

            lights[lightIndex].IsOn = true;
            if (doubleLights) {
                int halfIndex = (lightIndex + (lights.Count / 2)) % lights.Count;
                lights[halfIndex].IsOn = true;
            }
            lightTimer += interval;
        }
	}
}
