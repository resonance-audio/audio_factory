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

public class MagicField : MonoBehaviour {


    Material magicFieldMat;
    Renderer magicRenderer;
    Vector3 mainOffset = Vector3.zero;
    Vector3 maskOffset = Vector3.zero;

    float spinSpeed = 0.2f;
    float scrollSpeed = 0.5f;

    Vector3 onScale = new Vector3(1.0f, 1.0f, 1.0f);
    Vector3 offScale = new Vector3(2.0f, 2.0f, 2.0f);

    bool isOn;
    float onTimer = 0.0f;
    float onDuration = 2.0f;

    public bool IsOn
    {
        get { return isOn; }
        set { 
            isOn = value; 
            onTimer = Mathf.Clamp01(onTimer);
        }
    }

	void Start () {
        magicRenderer = GetComponent<Renderer>();
        magicFieldMat = magicRenderer.material;

	}
	
	void Update () {
        if (magicFieldMat && onTimer > 0)
        {
            mainOffset.x -= Time.deltaTime * spinSpeed;
            mainOffset.y -= Time.deltaTime * scrollSpeed;
            maskOffset.x += Time.deltaTime * spinSpeed * 1.5f;
            magicFieldMat.SetTextureOffset("_MainTex", mainOffset);
            magicFieldMat.SetTextureOffset("_MaskTex", maskOffset);
        }

        if (onTimer >= 0.0f && onTimer <= 1.0f)
        {
            onTimer += Time.deltaTime / (isOn ? onDuration : -onDuration);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(onTimer));
            magicFieldMat.SetColor("_Color", Color.Lerp(Color.black, Color.white, amount));
            if (isOn)
                transform.localScale = Vector3.Lerp(offScale, onScale, amount);
        }

        if (onTimer < 0.0f && magicRenderer.enabled) {
            magicRenderer.enabled = false;
        }

        if (onTimer > 0.0f && !magicRenderer.enabled) {
            magicRenderer.enabled = true;
        }            
	}   
}
