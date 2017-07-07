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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExitUIControl : MonoBehaviour {

    public Renderer finalFader;

    bool isOpen = false;
    float openTimer = 0.0f;

    [SerializeField] float blackFadeOutTime = 2f;

    CanvasGroup canvasGroup;


	void Start () {
        canvasGroup = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
        canvasGroup.alpha = 0;

        if (!finalFader)
            OpenMenu();
	}
	
	void Update () {

        if (isOpen && openTimer < 1.0f) {
            openTimer += Time.deltaTime;
            float amount = Mathf.SmoothStep(0, 1, openTimer);
            canvasGroup.alpha = amount;
            Vector3 canvasPos = transform.localPosition;
            canvasPos.y = Mathf.Lerp(1.25f, 1.5f, amount);
            transform.localPosition = canvasPos;
        }

        if (finalFader && finalFader.enabled) {
            Color fadeColor = finalFader.material.GetColor("_TintColor");
            fadeColor = Color.Lerp(fadeColor, Color.black, Time.deltaTime * 1f);
            finalFader.material.SetColor("_TintColor", fadeColor);
        }
	}

    public void OpenMenu() {
        if (isOpen)
            return;
        gameObject.SetActive(true);
        isOpen = true;
    }


    IEnumerator _FinalSequence() {
        yield return new WaitForSeconds(blackFadeOutTime);
        Debug.Log("Starting final sequence");

        GvrDaydreamApi.LaunchVrHome();
        yield return new WaitForSeconds(0.5f);
        Application.Quit();
    }
}
