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
using System;

public class ElevatorLight : MonoBehaviour {

    ElevatorLightControl control;

    [Serializable]
    public struct StereoClip {
        [SerializeField]
        public AudioClip left;
        [SerializeField]
        public AudioClip right;
    }

    bool isOn;
    public bool IsOn
    {
        get { return isOn; }
        set
        {
            isOn = value;
            lightTimer = Mathf.Clamp01(lightTimer);
            if (isOn) {
                if(audioSourceLeft != null && lightToneLeft != null) {
                    audioSourceLeft.PlayOneShot(lightToneLeft, control.Volume);
                }
                if(audioSourceRight != null && lightToneRight != null) {
                    audioSourceRight.PlayOneShot(lightToneRight, control.Volume);
                }
                onTimer = duration;
            }
        }
    }

    Material lightMat;
    float lightTimer = 0.0f;
    float fadeTime = 0.5f;
    Transform audioLeft;
    Transform audioRight;

    float duration = 0.5f;
    public float Duration
    {
        set { duration = value; }
    }

    public float FadeTime
    {
        set { fadeTime = value; }
    }

    float onTimer = 1.0f;

    AudioClip lightToneLeft;
    AudioClip lightToneRight;
    GvrAudioSource audioSourceLeft;
    GvrAudioSource audioSourceRight;

    public void Init (ElevatorLightControl _control, StereoClip clip, int ringId, int lightNumber) {
        control = _control;

        lightMat = gameObject.GetComponent<Renderer>().material;

        //set up left-sound
        lightToneLeft = clip.left;
        audioLeft = transform.Find("audioLeft");
        if(audioLeft != null) {
            audioSourceLeft = audioLeft.gameObject.AddComponent<GvrAudioSource>();
            audioSourceLeft.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSourceLeft.minDistance = 0.5f;
            audioSourceLeft.maxDistance = 3.0f;
            audioSourceLeft.gainDb = 5f;
            audioSourceLeft.listenerDirectivityAlpha = 0.3f;
            audioSourceLeft.listenerDirectivitySharpness = 3f;
        }

        //set up right-sound
        lightToneRight = clip.right;
        audioRight = transform.Find("audioRight");
        if(audioRight != null) {
            audioSourceRight = audioRight.gameObject.AddComponent<GvrAudioSource>();
            audioSourceRight.rolloffMode = AudioRolloffMode.Logarithmic;
            audioSourceRight.minDistance = 0.5f;
            audioSourceRight.maxDistance = 3.0f;
            audioSourceRight.gainDb = 5f;
            audioSourceRight.listenerDirectivityAlpha = 0.3f;
            audioSourceRight.listenerDirectivitySharpness = 3f;
        }
	}
	
	void Update () {
        if (lightTimer >= 0.0f && lightTimer <= 1.0f)
        {
            lightTimer += Time.deltaTime / (isOn ? fadeTime / 2 : -fadeTime);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(lightTimer));
            lightMat.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white, amount));
        }

        if (onTimer > 0)
        {
            onTimer -= Time.deltaTime;
            if (onTimer < 0)
            {
                IsOn = false;
            }
        }
    }
}
