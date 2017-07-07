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

﻿using System;
using UnityEngine;

public class FogBlender : MonoBehaviour {
    [SerializeField] float blendTime = 2.0f;
    [SerializeField] Material blendMaterial;

    [NonSerialized] RoomLightingSettings currentSettings;
    [NonSerialized] RoomLightingSettings goalSettings;
    [NonSerialized] Material originalBlendMaterial;

    [NonSerialized] float blendTimer = -1.0f;
	
	void Start () {
        currentSettings = RoomLightingSettings.CreateFromCurrentRoom();
        goalSettings = currentSettings;
        originalBlendMaterial = new Material(blendMaterial);
	}
	
	void Update () {
        if (blendTimer >= 0) {
            blendTimer += Time.deltaTime / blendTime;
            float amount = Mathf.SmoothStep(0, 1, blendTimer);
            RoomLightingSettings lerpedSettings = RoomLightingSettings.Lerp(currentSettings, goalSettings, amount);
            RoomLightingSettings.Set(lerpedSettings, blendMaterial);
            if (blendTimer > 1) {
                blendTimer = -1;
            }
        }
	}

    public void SetSettings(RoomLightingSettings start, RoomLightingSettings end) {
        blendTimer = 0f;
        RoomLightingSettings.Set(start, blendMaterial);
        currentSettings = start;
        goalSettings = end;
    }

    public void SetBlendTexture(Texture newTexture) {
        Texture nextTexture = blendMaterial.GetTexture("_NextTex");
        blendMaterial.SetTexture("_MainTex", nextTexture);
        blendMaterial.SetTexture("_NextTex", newTexture);
        blendMaterial.SetFloat("_TexBlend", 0.0f);
    }

    void OnApplicationQuit() {
        Debug.Log("Resetting Materials");
        blendMaterial.CopyPropertiesFromMaterial(originalBlendMaterial);
    }
}
