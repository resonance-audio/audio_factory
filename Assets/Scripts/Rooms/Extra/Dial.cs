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


public class Dial : MonoBehaviour {

    //[bh 02-20-2017] warning cleanup
    //ConsoleControl console;

    Material knobMat;

    bool isHovered;
    float hoverTimer;

    bool isPressed;

    public bool IsHovered {
        get { return isHovered; }
        set { 
            isHovered = value;
            hoverTimer = Mathf.Clamp01(hoverTimer);
        }
    }

    public void Init (ConsoleControl _console) {

        gameObject.AddComponent<CapsuleCollider>();

        knobMat = transform.Find("Knob").GetComponent<Renderer>().material;

        //[bh 02-20-2017] warning cleanup
        //console = _console;

        EventTrigger dialTrigger = gameObject.gameObject.AddComponent<EventTrigger>( );

        EventTrigger.Entry dialEnterEvent = new EventTrigger.Entry( );
        dialEnterEvent.eventID = EventTriggerType.PointerEnter;
        dialEnterEvent.callback.AddListener( ( data ) => { DialHover(true); } );
        dialTrigger.triggers.Add( dialEnterEvent );

        EventTrigger.Entry dialExitEvent = new EventTrigger.Entry( );
        dialExitEvent.eventID = EventTriggerType.PointerExit;
        dialExitEvent.callback.AddListener( ( data ) => { DialHover(false); } );
        dialTrigger.triggers.Add( dialExitEvent );

        EventTrigger.Entry dialDownEvent = new EventTrigger.Entry( );
        dialDownEvent.eventID = EventTriggerType.PointerDown;
        dialDownEvent.callback.AddListener( ( data ) => { DialPress(true); } );
        dialTrigger.triggers.Add( dialDownEvent );

        EventTrigger.Entry dialUpEvent = new EventTrigger.Entry( );
        dialUpEvent.eventID = EventTriggerType.PointerUp;
        dialUpEvent.callback.AddListener( ( data ) => { DialPress(false); } );
        dialTrigger.triggers.Add( dialUpEvent );
	}
	
	void Update () {
        if (isPressed)
            transform.Rotate(Vector3.up, -GvrController.Gyro.z * 5, Space.Self);

        if (hoverTimer >= 0.0f && hoverTimer <= 1.0f) {
            hoverTimer += Time.deltaTime * (isHovered ? 5 : -5);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(hoverTimer));

            knobMat.color = Color.Lerp(Color.gray, Color.white, amount);
        }
	}

    void DialHover(bool setting) {
        IsHovered = setting;
    }

    void DialPress(bool setting) {
        isPressed = setting;
    }
}
