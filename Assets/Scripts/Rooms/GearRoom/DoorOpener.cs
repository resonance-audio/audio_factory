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
using AudioEngineer.Rooms.Gears;

public class DoorOpener : MonoBehaviour {

    GearRoomManager roomManager;

    public Transform handle;
    Renderer handleRenderer;

    Color handleColorHover = Color.white;
    Color handleColorIdle = Color.black;

    InputRelay inputRelay;

    bool isHovering = false;
    public bool IsHovering {
        get { return isHovering; }
        set { 
            isHovering = value; 
            if (isHovering && !hoverSound.isPlaying)
                hoverSound.Play();
        }
    }

    bool isDragging = false;
    public bool IsDragging {
        get { return isDragging; }
        set { isDragging = value; }
    }

    public Transform crankAxis;
    Plane crankPlane;

    int rotations = 0;
    float lastAngle = 0;
    float totalGoalAngle = 0;
    float totalActualAngle = 0;

    float minRotation = 270.0f;
    float maxRotation = 1170.0f;

    public AnimationCurve wrongWayAnim;
    float wrongWayTimer = -1.0f;
    float wrongWayAmount = 1.0f;

    public Light hoverLight;
    public float lightHover = 0.5f;
    public float lightDrag = 1.0f;

    public GvrAudioSource hoverSound;

    public float Progress {
        get { return Mathf.InverseLerp(minRotation, maxRotation, totalGoalAngle); }
    }

	void Start () {

        inputRelay = handle.gameObject.AddComponent<InputRelay>();
        inputRelay.SetInputTarget(PointerEvent);
        crankPlane = new Plane(crankAxis.forward, crankAxis.position);

        totalActualAngle = totalGoalAngle = lastAngle = minRotation;

        handleRenderer = handle.GetComponent<Renderer>();
        handleColorIdle = handleRenderer.material.color;

        roomManager = GearRoomManager.instance;
	}
	
	void Update () {

        if (wrongWayTimer > 0) {
            wrongWayTimer -= Time.deltaTime;
            int direction = totalActualAngle < (minRotation + maxRotation) / 2.0f ? 1 : -1;
            totalActualAngle += wrongWayAnim.Evaluate(1 - wrongWayTimer) * direction * wrongWayAmount;
        }

        if (isDragging) {
            Vector3 reticleDirection = (GetPointOnPlane() - crankAxis.position).normalized;

            float reticleAngle = Vector3.Angle(reticleDirection, Vector3.up);
            reticleAngle *= Mathf.Sign(Vector3.Dot(reticleDirection, -crankAxis.right));
            if (reticleAngle < 0 )
                reticleAngle += 360;
            
            float deltaFromLastFrame = reticleAngle - lastAngle;
            lastAngle = reticleAngle;


            if (deltaFromLastFrame > 180) {
                rotations--;
                deltaFromLastFrame -= 360;
            }
            if (deltaFromLastFrame < -180) {
                rotations++;
                deltaFromLastFrame += 360;
            }

            totalGoalAngle += deltaFromLastFrame;

            if (totalActualAngle < minRotation - 10) {
                wrongWayTimer = 1;
                totalGoalAngle = minRotation;
                lastAngle = minRotation % 360;
                PointerEvent(InputRelay.Pointer.Up);
            }
            if (totalActualAngle > maxRotation + 10) {
                wrongWayTimer = 1;
                totalGoalAngle = maxRotation;
                lastAngle = maxRotation % 360;
                PointerEvent(InputRelay.Pointer.Up);
            }
        } else {
            if (roomManager.ObjectivesLeft == 0 && totalGoalAngle > minRotation) {
                totalGoalAngle -= Time.deltaTime * 60;
            }            
        }

        totalActualAngle = Mathf.Lerp(totalActualAngle, totalGoalAngle, Time.deltaTime * 5);
        Quaternion targetRot = Quaternion.AngleAxis(totalActualAngle + 90, Vector3.forward);
        transform.localRotation = targetRot;

        float lightGoal = 0;
        if (isHovering)
            lightGoal = lightHover;
        if (isDragging)
            lightGoal = lightDrag;

        hoverLight.intensity = Mathf.Lerp(hoverLight.intensity, lightGoal, Time.deltaTime * 5);

        handleRenderer.material.SetColor("_EmissionColor", isHovering || isDragging ? handleColorHover : handleColorIdle);
	}

    Vector3 GetPointOnPlane() {
        Ray reticle = LaserSelector.GetReticleRay();
        float rayDistance;
        if (crankPlane.Raycast(reticle, out rayDistance)) {
            return reticle.GetPoint(rayDistance);
        }
        Debug.Log("No point on plane");
        return Vector3.zero;
    }

    void PointerEvent(InputRelay.Pointer pointerEvent) {

        switch(pointerEvent) {
            case InputRelay.Pointer.Click:
                break;
            case InputRelay.Pointer.Down:
                Vector3 reticleDirection = (GetPointOnPlane() - crankAxis.position).normalized;

                float reticleAngle = Vector3.Angle(reticleDirection, Vector3.up);
                reticleAngle *= Mathf.Sign(Vector3.Dot(reticleDirection, -crankAxis.right));
                if (reticleAngle < 0 )
                    reticleAngle += 360;

                lastAngle = reticleAngle;
                IsDragging = true;
                break;            
            case InputRelay.Pointer.Up:
                IsDragging = false;
                break;            
            case InputRelay.Pointer.Enter:
                IsHovering = true;
                break;            
            case InputRelay.Pointer.Exit:
                IsHovering = false;
                break;            
            default:
                break;
        }
    }
}
