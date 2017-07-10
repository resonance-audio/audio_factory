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

using System;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class SimpleDraggable : BaseInteractable, IGvrPointerHoverHandler {

    protected const string ANIM_PROP_HOVER = "hover";
    protected const string ANIM_PROP_DOWN = "down";
    protected const string ANIM_PROP_ENABLED = "enabled";

    [SerializeField] protected Animator animator = null; //animator to do animating things with
    [SerializeField] protected int hoverFrames = 3; //frames after hovering in which this remains in the hover state
    [SerializeField] protected float lerpSpeed = 0.1f; //speed at which this lerps to the target location
    [SerializeField] protected Transform draggingPlane = null;
    [SerializeField] protected bool useReticleRaycast = false;

    [SerializeField] protected int _hoverFrame = 0;
    [SerializeField] protected float _grabbedDistance = 0f;
    [SerializeField] protected Vector3 _grabbedOffset = Vector3.zero;

    public bool isDown { get; protected set; }
    public bool isHover { get; protected set; }
    public bool isEnabled { get; protected set; }

    protected virtual void Awake() {
        _hoverFrame = 0;
        isDown = false;
        isHover = false;
        isEnabled = true;
    }

    protected virtual void OnEnable() {
    }

    protected virtual void ClickReleased() {
    }

    protected virtual void ClickPressed() {
    }

    public bool RaycastPlane(Vector3 pointerOrigin, Vector3 pointerDirection, Transform planeNormal, out Vector3 collisionPoint) {
        Vector3 forward = planeNormal.rotation * Vector3.forward;
        Plane leverCollisionPlane = new Plane(forward, planeNormal.position);
        Ray pointerRay = new Ray(pointerOrigin, pointerDirection);

        float enter;
        if(leverCollisionPlane.Raycast(pointerRay, out enter)) {
            //the point along the plane which the pointer has collided with
            collisionPoint = pointerRay.GetPoint(enter);
            return true;
        }else{
            collisionPoint = default(Vector3);
            return false;
        }
    }

    //the normalized direction vector of the pointer; raycasting from the camera position to the reticle position seems to provide the best results
    protected Vector3 PointerDirection {
        get {
            if(useReticleRaycast) {
                return (PointerReticle.instance.Position - Camera.main.transform.position).normalized;
            }else{
                return (GvrController.ArmModel.transform.rotation * GvrController.ArmModel.pointerRotation * Vector3.forward).normalized;
            }
        }
    }

    protected virtual Vector3 GetTargetPosition() {
        Vector3 pointerPos = Camera.main.transform.position;
        if(draggingPlane != null) {
            Vector3 planeCollide;
            if(RaycastPlane(pointerPos, PointerDirection, draggingPlane, out planeCollide)) {
                return planeCollide;
            }else{
                return transform.position;
            }
        }else{
            //no plane; return the scaled pointer direction
            return pointerPos + PointerDirection * _grabbedDistance;
        }
    }

    protected virtual Vector3 LerpPosition(Vector3 startPos, Vector3 targetPos, float lerpFrac) {
        return Vector3.Lerp(startPos, targetPos, lerpFrac);
    }

    protected virtual void Update() {
        bool gvrClicked = false;
        bool gvrDown = false;
        try {
            gvrClicked = GvrController.ClickButtonDown;
            gvrDown = GvrController.ClickButton;
        } catch (NullReferenceException) {
            gvrClicked = false;
            gvrDown = false;
        }

        isHover = (_hoverFrame--) > 0;

        Vector3 pointerPos = Vector3.zero;
        try {
            pointerPos = GvrController.ArmModel.transform.position + GvrController.ArmModel.elbowPosition;
        } catch(NullReferenceException) {
            pointerPos = Vector3.zero;
        }
        
        //allow dragging dragging, even if we're not hovering, so long as the button was pressed when we were previously hovering over the object
        if(isHover && gvrClicked) {
            //if we were previously released, invoke ClickPressed
            if(!isDown) {
                ClickPressed();
            }
            _grabbedDistance = Vector3.Distance(pointerPos, transform.position);
            _grabbedOffset = transform.position - pointerPos - (PointerDirection * _grabbedDistance);
            isDown = true;
        }

        if(!gvrDown) {
            if(isDown) {
                ClickReleased();
            }
            isDown = false;
        }

        if(isDown) {
            var rb = transform.GetComponent<Rigidbody>();
            Vector3 targetPos = GetTargetPosition();
            rb.velocity = (targetPos - transform.position) * lerpSpeed;
        }

        if(animator != null) {
            animator.SetBool(ANIM_PROP_HOVER, isHover);
            animator.SetBool(ANIM_PROP_ENABLED, isEnabled);
            animator.SetBool(ANIM_PROP_DOWN, isDown);
        }
    }

    public void OnGvrPointerHover(PointerEventData eventData) {
        _hoverFrame = hoverFrames;
    }
}