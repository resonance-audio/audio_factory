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

public class SeedOrbitor : MonoBehaviour {

    SeedControl control;
    int attactStrength = 1;

    Transform glow;
    Vector3 smallGlow = new Vector3(0.25f, 0.25f, 1.0f);
    Vector3 largeGlow = new Vector3(3.0f, 3.0f, 1.0f);

    int orbitorID;

    bool dying;

    GvrAudioSource orbitorSound;

    bool makingNoise;
    public bool MakingNoise
    {
        get { return makingNoise; }
        set { 
            makingNoise = value; 
            noiseFadeTimer = Mathf.Clamp01(noiseFadeTimer);
        }
    }
    float noiseFadeTimer = 0.0f;
    float noiseDuration = 3.0f;

    public void Init (SeedControl _control, AudioClip clip) {
        attactStrength = Random.Range(2, 8);
        orbitorID = int.Parse(transform.name.Substring(transform.name.Length - 1)) - 1;
        glow = transform.Find("Glow");
        control = _control;
        if (orbitorID == 0) 
            CreateAudioSource(clip);
	}
	
	void Update () {
        if (dying) {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 3);
            transform.Translate(0.0f, Time.deltaTime * 0.5f, 0.0f);
            if  (transform.localScale.x < 0.05f) {
                Destroy(gameObject);
            }
            return;
        }

        if (!control)
            return;
        
        transform.position = Vector3.Lerp(transform.position, control.GetOrbitPos(orbitorID), Time.deltaTime * attactStrength);
        transform.rotation = Quaternion.Lerp(transform.rotation, control.GetOrbitRot(orbitorID), Time.deltaTime * 5);
        transform.localScale = control.GetOrbitScale(orbitorID);

        glow.LookAt(Camera.main.transform);
        glow.localScale = Vector3.Lerp(smallGlow, largeGlow, Random.value);

        if (orbitorSound) {
            if (noiseFadeTimer >= 0.0f && noiseFadeTimer <= 1.0f) {
                noiseFadeTimer += Time.deltaTime / (makingNoise ? noiseDuration : -noiseDuration * 0.5f);
                float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(noiseFadeTimer));
                orbitorSound.volume = Mathf.Lerp(0, 0.5f, amount);
            } 

            if (orbitorSound.volume < 0.1f && orbitorSound.isPlaying) {
                orbitorSound.Stop();
            } 

            if (orbitorSound.volume > 0.1f && !orbitorSound.isPlaying) {
                orbitorSound.Play();
            } 
        }
    }

    public void Clear() {
        dying = true;
    }

    void CreateAudioSource(AudioClip sound) {
        orbitorSound = gameObject.AddComponent<GvrAudioSource>();
        orbitorSound.clip = sound;
        orbitorSound.loop = true;
        orbitorSound.volume = 0;
        orbitorSound.directivityAlpha = 0.12f;
        orbitorSound.directivitySharpness = 4.5f;
        MakingNoise = true;
    }
}
