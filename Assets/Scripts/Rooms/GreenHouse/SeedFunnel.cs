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
using UnityEngine.EventSystems;

public class SeedFunnel : MonoBehaviour {

    //[bh 02-20-2017] warning cleanup
    //SeedGenerator generator;

    bool isHovered;
    float hoverTimer = 0.0f;
    float hoverDuration = 1.0f;

    float funnelDistance = 0;


    public bool IsHovered
    {
        get { return isHovered; }
        set { 
            isHovered = value;
            hoverTimer = Mathf.Clamp01(hoverTimer);
        }
    }

    Material funnelMat;

	public void Init(SeedGenerator _generator) {
        //[bh 02-20-2017] warning cleanup
	    //generator = _generator;

        Material[] mats = gameObject.GetComponent<Renderer>().materials;
        funnelMat = mats[1];
  	}
	
	void Update () {
        if (hoverTimer >= 0.0f && hoverTimer <= 1.0f)
        {
            hoverTimer += Time.deltaTime / (isHovered ? hoverDuration * 0.5f : -hoverDuration);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(hoverTimer));
            funnelMat.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white, amount));
        }

        Vector3 reticleDir = LaserSelector.GetReticleRay().direction;
        Vector3 funnelDir = (transform.position - Camera.main.transform.position).normalized;
        funnelDistance = Vector3.Dot(reticleDir, funnelDir);

        if (funnelDistance > 0.95f && !IsHovered)
            IsHovered = true;
        if (funnelDistance < 0.95f && IsHovered)
            IsHovered = false;
	}
}
