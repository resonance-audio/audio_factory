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
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Util.Events;

//Main script for an object which may be rotated via dragging.
//   NOTE: because this handles input through IGvrPointerHoverHandler, this will need to 
//   be placed onto the object with the collider, NOT one of its parents/children.
[Serializable]
public class InteractableLever : BaseInteractable, IGvrPointerHoverHandler {

    const string ANIM_PROP_HOVER = "hover";
    const string ANIM_PROP_DOWN = "down";
    const string ANIM_PROP_ENABLED = "enabled";

    [Serializable]
    public struct Events {
        [SerializeField] public UnityEvent_Float onOutputChanged;
        [SerializeField] public UnityEvent_Float onAngleChanged;
        [SerializeField] public UnityEvent_Float onAngleDelta;
        [SerializeField] public UnityEvent onHoverEnter;
        [SerializeField] public UnityEvent onHoverExit;
    }

    [Serializable]
    public struct ConnectedLever {
        [SerializeField] public InteractableLever lever;
        [SerializeField] public float ratio;
    }

    [Serializable]
    public class LeverAngle {
        [TooltipAttribute("The angle to lock the lever at")]
        [SerializeField] public float angle = 0f;

        [TooltipAttribute("The angle-difference between the current lever orientation and this lever-angle, at which to invoke the unity events")]
        [SerializeField] public float triggerAngleDifference = 5f;

        [TooltipAttribute("The angle-difference between the current lever orientation and this lever-angle, at which to begin snapping")]
        [SerializeField] public float snapAngleDifference = 10f;

        [TooltipAttribute("Whether to require the user releases the click-button to invoke related unity events")]
        [SerializeField] public bool requireReleaseForEvents = false;

        [TooltipAttribute("Invoked when the lever snaps to this angle")]
        [SerializeField] public UnityEvent onLockedToAngle = null;
    }

//------------------------------------------------------------
// Serialized fields for InteractableLever
//------------------------------------------------------------
#region SERIALIZED_FIELDS
    [SerializeField] Events events = default(Events);

    [SerializeField] Animator animator = null;

    [TooltipAttribute("Frames after the pointer hovers over this object that we consider it \"hovered\"")]
    [SerializeField] int hoverFrames = 3;

    [TooltipAttribute("The transform who's rotation will be used as the lever's axis")]
    [SerializeField] Transform leverAxis = null;

    [TooltipAttribute("The transform which this behaviour will rotate on Update")]
    [SerializeField] Transform lever = null;

    [TooltipAttribute("The absolute minimum angle the lever will rotate to")]
    [SerializeField] float minAngle = -80f;

    [TooltipAttribute("The absolute maximum angle the lever will rotate to")]
    [SerializeField] float maxAngle = 80f;

    [TooltipAttribute("Angles which the lever will snap back to when released")]
    //[SerializeField] public float[] snapAngles = new float[] { -75f, 75f };
    [SerializeField] LeverAngle[] snapAngles = new LeverAngle[0];

    [TooltipAttribute("Distance from the lever's orgin in which dragging has no effect")]
    [SerializeField] float minOriginDistance = 0.1f;

    [TooltipAttribute("Speed by which the object will rotate to its target")]
    [RangeAttribute(0f, 1f)]
    [SerializeField] float rotationSpeed = 0.5f;

    [SerializeField] float outputMultiplier = 1f;

    [SerializeField] ConnectedLever[] connectedLevers = null;
    [SerializeField] Transform onWhileMovingPositive = null;
    [SerializeField] Transform onWhileMovingNegative = null;

    [SerializeField] public float positiveMultiplier = 1f;
    [SerializeField] public float negativeMultiplier = 1f;
#endregion SERIALIZED_FIELDS


//------------------------------------------------------------
// Non-Serialized fields for InteractableLever
//------------------------------------------------------------
#region NON_SERIALIZED_FIELDS
    [NonSerialized] protected int _hoverFrame = 0;
    [NonSerialized] protected bool _isDown = false;
    [NonSerialized] protected bool _isHover = false;
    [NonSerialized] protected bool _enabled = true;
    [NonSerialized] protected float _angle = 0f;
    [NonSerialized] protected LeverAngle _currentSnapAngle = null;
    [NonSerialized] protected bool _invokedSnap = false;
    [NonSerialized] protected float _targetAngle = 0f;
    [NonSerialized] protected float _dragAngleOffset = 0f;  
    [NonSerialized] protected float? _lastOutput = null;
    [NonSerialized] protected Vector3 _lastCollisionPoint;
#endregion NON_SERIALIZED_FIELDS

    //Get or set the current angle of the lever manually
    public float Angle {
        get {
            return _angle;
        }
        set {
            _angle = value;
            _targetAngle = value;
            _invokedSnap = false;
            _currentSnapAngle = GetClosestLeverAngle(_angle);
            lever.rotation = leverAxis.rotation * Quaternion.AngleAxis(_angle, Vector3.forward);
        }
    }

    Vector3 LastCollisionPoint {
        get { return _lastCollisionPoint; }
    }

    public float Output {
        get {
            float max = maxAngle;
            float min = minAngle;
            float cur = _angle;
            return outputMultiplier * (1f - (Mathf.DeltaAngle(cur, max) / Mathf.DeltaAngle(min, max)));
        }
    }

    public bool IsDragging {
        get {
            return _isDown;
        }
    }

    //Release the lever from the dragging state, if it is being dragged
    public void Release() {
        _isDown = false;
    }

    //------------------------------------------------------------
    // Methods for InteractableLever
    //------------------------------------------------------------
#region METHODS
    void Awake() {
        _hoverFrame = 0;
        _isDown = false;
        _isHover = false;
        _enabled = true;
        _angle = 0f;
        _targetAngle = 0f;
        _dragAngleOffset = 0f;
        _lastOutput = null;

        if(snapAngles != null && snapAngles.Length > 0) {
            //the current up-vector for the lever
            Vector3 up = lever.rotation * Vector3.up;
            //the origin up-vector, according to the rotation of the base
            Vector3 axisUp = leverAxis.rotation * Vector3.up;
            //the current real angle the lever is at
            float startingAngle = SignedDegreesBetween(up, axisUp, leverAxis.rotation * Vector3.forward);
            _currentSnapAngle = GetClosestLeverAngle(startingAngle);
        }else{
            _currentSnapAngle = null;
        }
    }

    void OnEnable() {
        if(animator != null) {
            animator.SetBool(ANIM_PROP_ENABLED, true);
        }
    }
    void OnDisable() {
        if(GlobalHoverTarget == this) GlobalHoverTarget = null;
        if(animator != null) {
            animator.SetBool(ANIM_PROP_ENABLED, false);
        }
    }

    void OnDestroy() {
        if(GlobalHoverTarget == this) GlobalHoverTarget = null;
        if(animator != null) {
            animator.SetBool(ANIM_PROP_ENABLED, false);
        }
    }

    public bool RaycastLeverPlane(Vector3 pointerOrigin, Vector3 pointerDirection, out Vector3 collisionPoint) {
        Vector3 forward = leverAxis.rotation * Vector3.forward;
        Plane leverCollisionPlane = new Plane(forward, leverAxis.position);
        Ray pointerRay = new Ray(pointerOrigin, pointerDirection);

        float enter;
        if(leverCollisionPlane.Raycast(pointerRay, out enter)) {
            //the point along the plane which the pointer has collided with
            collisionPoint = pointerRay.GetPoint(enter);
            _lastCollisionPoint = collisionPoint;
            return true;
        }else{
            collisionPoint = default(Vector3);
            _lastCollisionPoint = collisionPoint;
            return false;
        }
    }


    //Returns the closest snap-angle to the specified angle
    LeverAngle GetClosestLeverAngle(float angle) {
        if(snapAngles != null) {
            float min = float.MaxValue;
            int ind = -1;
            for(int i=0; i<snapAngles.Length; ++i) {
                if(snapAngles[i] != null){ 
                    float delta = Mathf.Abs(Mathf.DeltaAngle(angle, snapAngles[i].angle));
                    if(delta < min) {
                        ind = i;
                        min = delta;
                    }
                }
            }

            if(ind >= 0) {
                return snapAngles[ind];
            }
        }

        return null;
    }

    //Returns the first snap-angle found in which the specified angle is within its snapAngleDifference
    LeverAngle GetLeverAngleToSnapTo(float angle) {
        if(snapAngles != null) {
            for(int i=0; i<snapAngles.Length; ++i) {
                float delta = Mathf.Abs(Mathf.DeltaAngle(angle, snapAngles[i].angle));
                if(delta <= snapAngles[i].snapAngleDifference) {
                    return snapAngles[i];
                }
            }
        }
        
        return null;
    }

    //Raycasts from the GvrPointer onto the plane defined by the lever's axis/origin, and set's the lever's angle according to where the ray hit occurs.
    public void UpdateLeverAngle(Vector3 pointerOrigin, Vector3 pointerDirection) {

        Vector3 collision;
        if(RaycastLeverPlane(PointerPosition, PointerEulers, out collision)) {

            if((collision - leverAxis.position).magnitude < minOriginDistance) {
                //should we do anything if we're trying to drag too close to the base of the lever?
                return;
            }

            //the collision position that we got was in world-space, we need local to do anything useful
            collision = collision - leverAxis.position;

            //up-vector according to the static axis
            Vector3 axisUp = leverAxis.rotation * Vector3.up;
            //up-vector according to the rotation of the lever when we started dragging
            Vector3 currentUp = lever.rotation * Vector3.up;
            //forward-vector for the lever axis
            Vector3 forward = leverAxis.rotation * Vector3.forward;

            //the angle before we started dragging
            float currentAngle = SignedDegreesBetween(currentUp, axisUp, forward);
            //the delta-angle between where we started dragging and where we're currently dragging
            float angleDelta = SignedDegreesBetween(collision, currentUp, forward);
            angleDelta = Mathf.MoveTowardsAngle(angleDelta, angleDelta - _dragAngleOffset, float.MaxValue);
            if(angleDelta > 180f) angleDelta -= 360f;

            //find the angle between the "up"-vector and the raycast hit vector
            _targetAngle = currentAngle + angleDelta;
            
            //clamp angle within our specified bounds
            _targetAngle = Mathf.Clamp(_targetAngle, minAngle, maxAngle);

            float prevAngle = _angle;
            _angle = Mathf.LerpAngle(_angle, _targetAngle, rotationSpeed * (angleDelta>=0f ? positiveMultiplier : negativeMultiplier));

            if(angleDelta >= 0f) {
                if(onWhileMovingPositive != null) {
                    onWhileMovingPositive.gameObject.SetActive(true);
                }
                if(onWhileMovingNegative != null) {
                    onWhileMovingNegative.gameObject.SetActive(false);
                }
            }else{
                if(onWhileMovingPositive != null) {
                    onWhileMovingPositive.gameObject.SetActive(false);
                }
                if(onWhileMovingNegative != null) {
                    onWhileMovingNegative.gameObject.SetActive(true);
                }
            }
            
            if(events.onAngleDelta != null) {
                events.onAngleDelta.Invoke(_angle - prevAngle);
            }

            if(events.onAngleChanged != null && _angle != prevAngle) {
                events.onAngleChanged.Invoke(_angle);
            }

            //set the rotation on the lever to point at the hit vector
            lever.rotation = leverAxis.rotation * Quaternion.AngleAxis(_angle, Vector3.forward);
        }
    }

    //Returns the signed angle difference between two vectors.
    float SignedDegreesBetween(Vector3 a, Vector3 b, Vector3 planeNormal) {
        float diff = Vector3.Dot(a.normalized, b.normalized);
        bool positive = Vector3.Dot(Vector3.Cross(a.normalized, b.normalized).normalized, planeNormal.normalized) < 0f;
        float ret = Mathf.Acos(diff) * (positive ? 1f : -1f) * 180f / Mathf.PI;
        if(float.IsInfinity(ret) || float.IsNaN(ret)) {
            return 0f;
        }else{
            return ret;
        }
    }

    Vector3 PointerPosition {
        get {
            //return GvrController.ArmModel.transform.position + GvrController.ArmModel.pointerPosition;
            return PointerReticle.instance.RootPosition;
        }
    }

    Vector3 PointerEulers {
        get {
            // Vector3 rp = GvrController.ArmModel.GetComponentInChildren<GvrLaserPointer>().reticle.transform.position;
            // Vector3 pp = PointerPosition;
            // Quaternion dir = Quaternion.Euler((rp-pp).x, (rp-pp).y, (rp-pp).z);
            // return (dir * Vector3.forward).normalized;
            return PointerReticle.instance.Rotation * Vector3.forward;
        }
    }

    void ClickReleased() {
    }

    void ClickPressed() {
        Vector3 dragStart;
        if(RaycastLeverPlane(PointerPosition, PointerEulers, out dragStart)) {

            //the up-vector currently for the lever
            Vector3 currentUpVector = lever.rotation * Vector3.up;

            //get the angle between the initial pointer-ray-hit and the current up-vector, so we can add this to the angle for the entire drag motion
            _dragAngleOffset = SignedDegreesBetween((dragStart - leverAxis.position).normalized, currentUpVector.normalized, leverAxis.rotation * Vector3.forward);
        }
    }

    protected virtual void Update() {
        bool gvrClicked = GvrController.ClickButtonDown;
        bool gvrDown = GvrController.ClickButton;

        //if we stopped hovering, invoke the proper event(s) and do global stuff
        {
            bool previouslyHovering = _isHover;
            _isHover = (_hoverFrame--) > 0;
            if(!_isHover && GlobalHoverTarget == this) {
                GlobalHoverTarget = null;
            }

            if(previouslyHovering && !_isHover && !_isDown && events.onHoverExit != null) {
                events.onHoverExit.Invoke();
            }
        }

        //allow dragging dragging, even if we're not hovering, so long as the button was pressed when we were previously hovering over the object
        if(_isHover && gvrClicked) {
            //if we were previously released, invoke ClickPressed
            if(!_isDown) {
                ClickPressed();
            }
            _isDown = true;
        }

        if(!gvrDown) {
            if(_isDown) {
                ClickReleased();
            }
            _isDown = false;
        }

        var prevSnap = _currentSnapAngle;
        _currentSnapAngle = GetLeverAngleToSnapTo(_targetAngle) ?? _currentSnapAngle;

        //if we switched angle-snaps, invoke the appropriate events
        if(prevSnap != _currentSnapAngle) {
            _invokedSnap = false;
        }
            
        if(!_invokedSnap
        && _currentSnapAngle != null 
        && _currentSnapAngle.onLockedToAngle != null 
        && !(_isDown & _currentSnapAngle.requireReleaseForEvents)
        && Mathf.Abs(Mathf.DeltaAngle(_angle, _currentSnapAngle.angle)) <= _currentSnapAngle.triggerAngleDifference) {
            _invokedSnap = true;
            _currentSnapAngle.onLockedToAngle.Invoke();
        }

        if(_isDown) {
            UpdateLeverAngle(PointerPosition, PointerEulers);

            if(connectedLevers != null) {
                for(int c=0; c<connectedLevers.Length; ++c) {
                    if(connectedLevers[c].lever != null) {
                        connectedLevers[c].lever.Angle = this.Angle * connectedLevers[c].ratio;
                    }
                }
            }
        }else{
            if(_currentSnapAngle != null) {
                _targetAngle = _currentSnapAngle.angle;
                _angle = Mathf.LerpAngle(_angle, _targetAngle, rotationSpeed);
                //set the rotation on the lever to point at the hit vector
                lever.rotation = leverAxis.rotation * Quaternion.AngleAxis(_angle, Vector3.forward);
            }
        }

        float output = Output;
        if(_lastOutput != null) {
            if(events.onOutputChanged != null) {
                events.onOutputChanged.Invoke(output);
            }
        }

        if(animator != null) {
            animator.SetBool(ANIM_PROP_HOVER, _isHover);
            animator.SetBool(ANIM_PROP_DOWN, _isDown);
        }
    }

    public void OnGvrPointerHover(PointerEventData eventData) {
        
        //if we were not previously hovering, invoke the hover-enter event
        if(!_isHover && events.onHoverEnter != null) {
            events.onHoverEnter.Invoke();
        }
        
        _hoverFrame = hoverFrames;
        GlobalHoverTarget = this;
    }
    
#endregion METHODS

    void OnDrawGizmos() {
        if(Application.isPlaying) {
            Color col = Gizmos.color;
            Gizmos.color = Color.white;
            Gizmos.DrawLine(PointerPosition, PointerPosition + (PointerEulers) * 10f);
            Gizmos.color = col;
        }
    }
}