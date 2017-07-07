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

public class InputRelay : MonoBehaviour {

    public enum Pointer {Click, Down, Up, Enter, Exit}
    public delegate void InputDelegate(Pointer pointerEvent);
    event InputDelegate OnEvent;


	void Start () {
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry clickEvent = new EventTrigger.Entry( );
        clickEvent.eventID = EventTriggerType.PointerClick;
        clickEvent.callback.AddListener( ( data ) => { PointerEvent(Pointer.Click); } );
        trigger.triggers.Add( clickEvent );

        EventTrigger.Entry pointerDownEvent = new EventTrigger.Entry();
        pointerDownEvent.eventID = EventTriggerType.PointerDown;
        pointerDownEvent.callback.AddListener((data) => { PointerEvent(Pointer.Down); });
        trigger.triggers.Add(pointerDownEvent);

        EventTrigger.Entry pointerUpEvent = new EventTrigger.Entry();
        pointerUpEvent.eventID = EventTriggerType.PointerUp;
        pointerUpEvent.callback.AddListener((data) => { PointerEvent(Pointer.Up); });
        trigger.triggers.Add(pointerUpEvent);

        EventTrigger.Entry enterEvent = new EventTrigger.Entry();
        enterEvent.eventID = EventTriggerType.PointerEnter;
        enterEvent.callback.AddListener((data) => { PointerEvent(Pointer.Enter); });
        trigger.triggers.Add(enterEvent);

        EventTrigger.Entry exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((data) => { PointerEvent(Pointer.Exit); });
        trigger.triggers.Add(exitEvent);

        if (!GetComponent<Collider>()) 
            gameObject.AddComponent<SphereCollider>();
	}

    public void SetInputTarget(InputDelegate callback) {
        OnEvent = callback;
    }

    void PointerEvent(Pointer pointerEvent) {
        if (OnEvent != null)
            OnEvent(pointerEvent);
    }
	

}
