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
using UnityEngine.EventSystems;

public class PedestalButton : MonoBehaviour {


    public AudioClip hoverSound;
    public AudioClip clickSound;
    GvrAudioSource buttonSounds;

    public bool Disabled {
        get { return isDisabled; }
        set { 
            if (isDisabled != value) {
                disabledTimer = Mathf.Clamp01(disabledTimer);
                hoverTimer = 0.0f;
            }
            isDisabled = value;
        }
    }

    protected SphereCollider collision;

    protected ElevatorSpeaker speaker; 
    protected Material buttonMat;

    protected bool isHovered = false;
    protected float hoverTimer = 0.0f;
    protected float hoverDuration = 0.5f;    

    public Color hoverColor = new Color(1.0f, 0.0f, 0.0f);
    public Color idleColor = new Color(0.0f, 1.0f, 0.0f);
    public Color disabledColor = new Color(0.1f, 0.1f, 0.1f);
    public Color pressedColor = new Color(1.0f, 1.0f, 1.0f);


    protected bool isDisabled = false;
    protected float disabledTimer = 0.0f;
    protected float disabledDuration = 0.5f;    

    public AnimationCurve idlePulseAnim;
    public float pulseDarkenAmout = 1.0f;

    public bool IsHovered {
        get { return isHovered; }
        set { 
            isHovered = value;
            hoverTimer = Mathf.Clamp01(hoverTimer);
            if (isHovered && !isDisabled)
                buttonSounds.PlayOneShot(hoverSound);
        }
    }

    protected bool buttonDown;
    Vector3 upPos;
    Vector3 downPos;
    public float buttonDepth;

    protected virtual void Start() {
        speaker = ElevatorSpeaker.instance;
        buttonMat = gameObject.GetComponent<Renderer>().material;
            
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>( );

        EventTrigger.Entry clickEvent = new EventTrigger.Entry( );
        clickEvent.eventID = EventTriggerType.PointerClick;
        clickEvent.callback.AddListener( ( data ) => { Clicked(); } );
        trigger.triggers.Add( clickEvent );

        EventTrigger.Entry downEvent = new EventTrigger.Entry( );
        downEvent.eventID = EventTriggerType.PointerDown;
        downEvent.callback.AddListener( ( data ) => { Pressed(true); } );
        trigger.triggers.Add( downEvent );

        EventTrigger.Entry upEvent = new EventTrigger.Entry( );
        upEvent.eventID = EventTriggerType.PointerUp;
        upEvent.callback.AddListener( ( data ) => { Pressed(false); } );
        trigger.triggers.Add( upEvent );

        EventTrigger.Entry enterEvent = new EventTrigger.Entry( );
        enterEvent.eventID = EventTriggerType.PointerEnter;
        enterEvent.callback.AddListener( ( data ) => { Hover(true); } );
        trigger.triggers.Add( enterEvent );

        EventTrigger.Entry exitEvent = new EventTrigger.Entry( );
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener( ( data ) => { Hover(false); } );
        trigger.triggers.Add( exitEvent );

        collision = gameObject.AddComponent<SphereCollider>();	

        IsHovered = false;
        upPos = transform.localPosition;
        downPos = transform.localPosition - (Vector3.up * buttonDepth);

        buttonSounds = gameObject.AddComponent<GvrAudioSource>();


    }
	
    protected virtual void Update() {

                
        if (!buttonDown && !isDisabled) {
            if (hoverTimer >= 0.0f && hoverTimer <= 1.0f) {
                hoverTimer += Time.deltaTime / (isHovered ? hoverDuration * 0.5f : -hoverDuration);
                float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(hoverTimer));
                buttonMat.SetColor("_EmissionColor", Color.Lerp(idleColor, hoverColor, amount));
            }
        }

        if (disabledTimer >= 0.0f && disabledTimer <= 1.0f) {
            disabledTimer += Time.deltaTime / (isDisabled ? disabledDuration : -disabledDuration);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(disabledTimer));
            buttonMat.SetColor("_EmissionColor", Color.Lerp(idleColor, disabledColor, amount));
        }

        transform.localPosition = Vector3.Lerp(transform.localPosition, upPos, Time.deltaTime * 1);

        if (!isHovered && !isDisabled) {
            float amount = idlePulseAnim.Evaluate(Time.time);
            buttonMat.SetColor("_EmissionColor", Color.Lerp(idleColor * pulseDarkenAmout, idleColor, amount));
        }
    }

    protected virtual void Clicked() {
        if (isDisabled)
            return;
    }


    void Pressed(bool setting) {

        if (isDisabled)
            return;

        if (setting)
            buttonSounds.PlayOneShot(clickSound);
        
        buttonDown = setting;
        if (buttonDown) {
            transform.localPosition = downPos;
            buttonMat.SetColor("_EmissionColor", pressedColor);
        }
    }

    void Hover(bool setting) {
        IsHovered = setting;
    }
}

