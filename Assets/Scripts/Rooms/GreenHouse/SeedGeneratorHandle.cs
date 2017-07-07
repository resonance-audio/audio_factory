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


public class SeedGeneratorHandle : MonoBehaviour {

    bool isOn = false;
    Quaternion turnStartRot;
    Quaternion turnEndRot;
    float turnTimer = 0.0f;
    float turnDuration = 0.5f;

    Color hoverOnColor = new Color(0.8f, 0.8f, 0.8f);
    Color hoverOffColor = new Color(0.0f, 0.0f, 0.0f);

    Color readyOnColor = new Color(1.0f, 1.0f, 1.0f);
    Color readyOffColor = new Color(0.0f, 0.0f, 0.0f);

    AnimationCurve flashCurve;

    public bool Used {
        get; protected set;
    }

    public bool IsOn
    {
        get { return isOn; }
        set {
            if (value == isOn)
                return;

            isOn = value;
            turnTimer = Mathf.Clamp01(turnTimer);
            if (isOn)
            {
                onTimer = onDuration;
                generator.TurnHandle(handleID);
            }
        }
    }

    bool isReady;

    public bool IsReady {
        get { return isReady; }
        set { 
            readyTimer = Mathf.Clamp01(readyTimer);
            isReady = value; 
        }        
    }

    float flashTimer = -1.0f;
    float flashDuration = 0.5f;

    float readyTimer = 0.0f;
    float readyDuration = 0.5f;

    float onTimer = 0.0f;
    float onDuration = 3.0f;

    bool isHovered = false;
    float hoverTimer = 0.0f;
    float hoverDuration = 0.5f;

    AudioClip hoverSound;
    AudioClip handleSound;

    public bool IsHovered
    {
        get { return isHovered; }
        set { 
            isHovered = value;
            hoverTimer = Mathf.Clamp01(hoverTimer);
        }
    }
        
    Material handleMat;
    Material ringMat;

    SeedGenerator generator;
    int handleID;

    GvrAudioSource audioSource;

    public void Init (SeedGenerator _generator, int id, AudioClip _handleSound, AudioClip _hoverSound, AnimationCurve curve) {
        
        generator = _generator;
        handleID = id;

        turnStartRot = transform.localRotation;
        turnEndRot = transform.localRotation * Quaternion.AngleAxis(-90, Vector3.up);

        EventTrigger trigger = gameObject.AddComponent<EventTrigger>( );

        EventTrigger.Entry clickEvent = new EventTrigger.Entry( );
        clickEvent.eventID = EventTriggerType.PointerClick;
        clickEvent.callback.AddListener( ( data ) => { TurnHandle(); } );
        trigger.triggers.Add( clickEvent );

        EventTrigger.Entry enterEvent = new EventTrigger.Entry( );
        enterEvent.eventID = EventTriggerType.PointerEnter;
        enterEvent.callback.AddListener( ( data ) => { Hover(true); } );
        trigger.triggers.Add( enterEvent );

        EventTrigger.Entry exitEvent = new EventTrigger.Entry( );
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener( ( data ) => { Hover(false); } );
        trigger.triggers.Add( exitEvent );

        SphereCollider coll = gameObject.AddComponent<SphereCollider>();
        coll.radius *= 2.0f;

        //we want the 2nd material, if it exists
        var rend = gameObject.GetComponent<Renderer>();
        handleMat = null;
        if(rend != null) {
            var mats = rend.materials;
            if(mats != null && mats.Length >= 2) {
                handleMat = mats[1];
            }else if(mats != null && mats.Length >= 1) {
                handleMat = mats[0];
            }else {
                handleMat = rend.material;
            }
        }

        hoverSound = _hoverSound;
        handleSound = _handleSound;

        flashCurve = curve;

        CreateAudioSource();
	}

    public void AddHoverRing(Material ring) {
        ringMat = ring;
    }

	void Update () {

        if (flashTimer > 0) {
            flashTimer -= Time.deltaTime / flashDuration;
            float flashAmount = flashCurve.Evaluate(Mathf.Clamp01(flashTimer));
            handleMat.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white, flashAmount));  
        }

        if (turnTimer >= 0.0f && turnTimer <= 1.0f)
        {
            turnTimer += Time.deltaTime / (Used ? turnDuration : -turnDuration);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(turnTimer));
            transform.localRotation = Quaternion.Lerp(turnStartRot, turnEndRot, amount);
        }

        if (hoverTimer >= 0.0f && hoverTimer <= 1.0f)
        {
            hoverTimer += Time.deltaTime / (isHovered ? hoverDuration * 0.5f : -hoverDuration);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(hoverTimer));
            ringMat.SetColor("_EmissionColor", Color.Lerp(hoverOffColor, hoverOnColor, amount));
        }

        if (readyTimer >= 0.0f && readyTimer <= 1.0f)
        {
            readyTimer += Time.deltaTime / (isReady ? readyDuration * 0.5f : -readyDuration);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(readyTimer));
            handleMat.SetColor("_EmissionColor", Color.Lerp(readyOffColor, readyOnColor, amount));
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

    public void Flash() {
        if (!IsReady)
            return;
        flashTimer = 1.0f;
    }

    public void TurnHandle()
    {
        Debug.Log("Turn Handle");
        if (!IsReady)
            return;
        if (!generator.ReadyForHandle) {
            return;
        }

        Used = true;
        IsHovered = false;
        IsReady = false;
        IsOn = true;
        audioSource.PlayOneShot(handleSound);
    }

    void Hover(bool setting) {
        if (!IsReady)
            return;
        
        IsHovered = setting;
        if (setting) {
            audioSource.PlayOneShot(hoverSound);
        }
    }

    void CreateAudioSource() {
        audioSource = gameObject.AddComponent<GvrAudioSource>();
        audioSource.volume = 1;
    }


}
