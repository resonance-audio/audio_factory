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
using System.Collections;

public class Switch : MonoBehaviour {

    ConsoleControl console;

    Transform coverObj;
    Transform switchObj;

    bool coverOpen;
    float coverTimer;
    float coverClosedAngle = 0;
    float coverOpenAngle = -120;

    public bool CoverOpen {
        get { return coverOpen; }
        set { 
            coverOpen = value;
            coverTimer = Mathf.Clamp01(coverTimer);
        }
    }

    bool switchOn;
    float switchTimer;
    float switchClosedAngle = 50;
    float switchOpenAngle = -50;

    public bool SwitchOn {
        get { return switchOn; }
        set { 
            switchOn = value;
            switchTimer = Mathf.Clamp01(switchTimer);
            console.SwitchSet();
        }
    }

    public void Init (ConsoleControl _console) {
        console = _console;
        coverObj = transform.GetChild(0);
        switchObj = transform.GetChild(1);
        BoxCollider coverCol = coverObj.gameObject.AddComponent<BoxCollider>();
        coverCol.size *= 1.5f;
        switchObj.gameObject.AddComponent<BoxCollider>();

        EventTrigger coverTrigger = coverObj.gameObject.AddComponent<EventTrigger>( );

        EventTrigger.Entry coverEnterEvent = new EventTrigger.Entry( );
        coverEnterEvent.eventID = EventTriggerType.PointerEnter;
        coverEnterEvent.callback.AddListener( ( data ) => { CoverHover(true); } );
        coverTrigger.triggers.Add( coverEnterEvent );

        EventTrigger.Entry coverExitEvent = new EventTrigger.Entry( );
        coverExitEvent.eventID = EventTriggerType.PointerExit;
        coverExitEvent.callback.AddListener( ( data ) => { CoverHover(false); } );
        coverTrigger.triggers.Add( coverExitEvent );

        EventTrigger.Entry coverClickEvent = new EventTrigger.Entry( );
        coverClickEvent.eventID = EventTriggerType.PointerClick;
        coverClickEvent.callback.AddListener( ( data ) => { CoverClick(); } );
        coverTrigger.triggers.Add( coverClickEvent );

        EventTrigger switchTrigger = switchObj.gameObject.AddComponent<EventTrigger>( );

        EventTrigger.Entry switchEnterEvent = new EventTrigger.Entry( );
        switchEnterEvent.eventID = EventTriggerType.PointerEnter;
        switchEnterEvent.callback.AddListener( ( data ) => { SwitchHover(true); } );
        switchTrigger.triggers.Add( switchEnterEvent );

        EventTrigger.Entry switchExitEvent = new EventTrigger.Entry( );
        switchExitEvent.eventID = EventTriggerType.PointerExit;
        switchExitEvent.callback.AddListener( ( data ) => { SwitchHover(false); } );
        switchTrigger.triggers.Add( switchExitEvent );

        EventTrigger.Entry switchClickEvent = new EventTrigger.Entry( );
        switchClickEvent.eventID = EventTriggerType.PointerClick;
        switchClickEvent.callback.AddListener( ( data ) => { SwitchClick(); } );
        switchTrigger.triggers.Add( switchClickEvent );


	}
	
	void Update () {
        if (coverTimer >= 0.0f && coverTimer <= 1.0f) {
            coverTimer += Time.deltaTime * (coverOpen ? 5 : -5);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(coverTimer));
            Quaternion startAngle = Quaternion.AngleAxis(coverClosedAngle, Vector3.right);
            Quaternion endAngle = Quaternion.AngleAxis(coverOpenAngle, Vector3.right);

            coverObj.localRotation = Quaternion.Lerp(startAngle, endAngle, amount);
        }

        if (switchTimer >= 0.0f && switchTimer <= 1.0f) {
            switchTimer += Time.deltaTime * (switchOn ? 5 : -5);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(switchTimer));
            Quaternion startAngle = Quaternion.AngleAxis(switchClosedAngle, Vector3.right);
            Quaternion endAngle = Quaternion.AngleAxis(switchOpenAngle, Vector3.right);
            switchObj.localRotation = Quaternion.Lerp(startAngle, endAngle, amount);
        }

	}


    void CoverHover(bool setting) {
        
    }

    void CoverClick() {
        CoverOpen = !CoverOpen;
    }

    void SwitchHover(bool setting) {

    }

    void SwitchClick() {
        SwitchOn = !SwitchOn;
    }
}
