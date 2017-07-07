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

public class OverheadLights : MonoBehaviour {

    bool lightsOn;
    float lightsTimer;

    GvrAudioSource leftSounds;
    GvrAudioSource rightSounds;

    public AudioClip lightsOnLeft;
    public AudioClip lightsOnRight;

    float lightsOnDelay = 0.0f;
    bool lightsOnPlayed;

    public AudioClip powerUpLeft;
    public AudioClip powerUpRight;

    GvrAudioSource finalPayOffSource;

    float powerUpDelay = 0.50f;
    bool powerUpPlayed;

    float glassTimer;

    float loopDelay = 1.0f;
    bool loopPlayed;

    float completeDelay = 2.0f;
    bool complete;

    Renderer glass;
    bool glassOn;

    public Color glassDarkColor = Color.black;
    public Color glassLightColor = Color.white;

	void Start () {
        
        leftSounds = transform.Find("LeftSounds").GetComponent<GvrAudioSource>();
        rightSounds = transform.Find("RightSounds").GetComponent<GvrAudioSource>();
        FindGlass();
        LightsOn = false;
	}

    public bool LightsOn {
        get { return lightsOn; }
        set { 
            lightsOn = value; 
            lightsTimer = 0.0f;
            lightsOnPlayed = false;
            powerUpPlayed = false;
            loopPlayed = false;
            if (!lightsOn) {
                leftSounds.Stop();
                rightSounds.Stop();   
                glassTimer = Mathf.Clamp01(glassTimer);
                glassOn = false;
            }           
        }
    }


    void FindGlass() {
        var glassObj = GameObject.Find("GLASS");
        glass = glassObj != null ? glassObj.GetComponent<Renderer>() : null;
    }
	
	void Update () {
        if (glassTimer >= 0.0f && glassTimer <= 1.0f) {
            glassTimer += Time.deltaTime * (glassOn ? 1 : -1);

            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(glassTimer));
            Color windowColor = Color.Lerp(glassDarkColor, glassLightColor, amount);
            glass.material.SetColor("_EmissionColor", windowColor);
        }

        if (lightsOn) {
            lightsTimer += Time.deltaTime;
            if (!lightsOnPlayed && lightsTimer > lightsOnDelay) {
                lightsOnPlayed = true;
                leftSounds.PlayOneShot(lightsOnLeft);
                rightSounds.PlayOneShot(lightsOnRight);
            }
            if (!powerUpPlayed && lightsTimer > powerUpDelay) {
                powerUpPlayed = true;
                leftSounds.PlayOneShot(powerUpLeft);
                rightSounds.PlayOneShot(powerUpRight);
                glassTimer = Mathf.Clamp01(glassTimer);
                glassOn = true;
            }
            if (!loopPlayed && lightsTimer > loopDelay) {
                loopPlayed = true;
                leftSounds.Play();
                rightSounds.Play();
            }
            if (!complete && lightsTimer > completeDelay) {
                complete = true;
                if(GreenHouseRoomManager.instance != null) {
                    GreenHouseRoomManager.instance.Complete();
                    GreenHouseRoomManager.instance.GetVOSequencer().AddEvent("greenhousecomplete");
                }
            }
        }

        if (leftSounds.isPlaying) {
            leftSounds.volume -= Time.deltaTime / 8.0f;
            rightSounds.volume -= Time.deltaTime / 8.0f;
            if (leftSounds.volume < 0.05f) {
                leftSounds.Stop();
                rightSounds.Stop();
            }
        }
	}
}
