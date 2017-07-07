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
using UnityEngine;
using UnityEngine.EventSystems;

public class SeedHandle : MonoBehaviour {


    bool isDragging;
    bool seedGrabbed;

    public bool IsDragging
    {
        get { return isDragging; }
        set { isDragging = value;  }
    }

    float minDistanceFromController = 0.5f;
    float maxDistanceFromController = 1.25f;

    SeedControl seedControl;
    SeedFunnel seedFunnel;

    Vector3 seedDrift = Vector2.zero;
    Vector3 lastPosition = Vector2.zero;
    float homeHeight;


    public void Init(SeedControl _seedControl) {
        seedControl = _seedControl;

        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDownEvent = new EventTrigger.Entry();
        pointerDownEvent.eventID = EventTriggerType.PointerDown;
        pointerDownEvent.callback.AddListener((data) => { PointerDown(); });
        trigger.triggers.Add(pointerDownEvent);

        EventTrigger.Entry pointerUpEvent = new EventTrigger.Entry();
        pointerUpEvent.eventID = EventTriggerType.PointerUp;
        pointerUpEvent.callback.AddListener((data) => { PointerUp(); });
        trigger.triggers.Add(pointerUpEvent);

        EventTrigger.Entry enterEvent = new EventTrigger.Entry();
        enterEvent.eventID = EventTriggerType.PointerEnter;
        enterEvent.callback.AddListener((data) => { Hover(true); });
        trigger.triggers.Add(enterEvent);

        EventTrigger.Entry exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((data) => { Hover(false); });
        trigger.triggers.Add(exitEvent);


	}
	
	void Update () {

        Vector3 directionToHandle = (transform.position - (Camera.main.transform.position - (Vector3.up * 0.5f))).normalized;
        float offsetFromUp = Vector3.Dot(directionToHandle, Vector3.up);

        float distanceFromController = Mathf.Lerp(maxDistanceFromController, minDistanceFromController, offsetFromUp);


        if (isDragging) {
            if (seedFunnel.IsHovered)
            {
                Vector3 funnelPos = seedFunnel.transform.position + new Vector3(0.0f, 0.25f, 0.0f);
                transform.position = Vector3.Lerp(transform.position, funnelPos, Time.deltaTime * 4);
            }
            else
            {
                Vector3 dragPoint = LaserSelector.GetReticleRay().GetPoint(distanceFromController);
                transform.position = Vector3.Lerp(transform.position, dragPoint, Time.deltaTime * 3);
            }
            seedDrift = transform.position - lastPosition;

            lastPosition = transform.position;
        } else {
            if (seedGrabbed) {
                Vector3 correctPos = transform.position;
                correctPos.y = homeHeight;

                transform.position = Vector3.Lerp(transform.position, correctPos, Time.deltaTime * 0.25f);

                transform.Translate(seedDrift);

                seedDrift *= 0.965f;
            }
        }
	}

    void PointerDown()
    {
//        if (!seedControl.HasOrbitors) {
//            return;
//        }

        if (!seedGrabbed) {
            seedGrabbed = true;
            seedControl.SeedPickedUp();
            homeHeight = transform.position.y ;
            Debug.Log("Home height " + homeHeight);
        }

        IsDragging = true;

    }

    void PointerUp()
    {
        if (!seedControl.movingToFunnel && seedFunnel.IsHovered)
            seedControl.StartMoveToFunnel();
        
        IsDragging = false;

        seedControl.ReleaseSeed();
    }

    void Hover(bool setting)
    {
        if (!IsDragging)
            seedControl.IsHovered = setting;
    }

    public void AddFunnel(SeedFunnel _funnel) {
        seedFunnel = _funnel;
    }
}
