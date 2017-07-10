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

namespace AudioEngineer.Rooms.Finale {

    //Script which allows dragging of the sliding doors in the Finale Rooms
    public class FinaleDoor : MonoBehaviour, IGvrPointerHoverHandler {

        //the starting drag position (only Y-coord matters here)
        [SerializeField] Transform start = null;
        //the ending drag position (only Y-coord matters here)
        [SerializeField] Transform end = null;
        //time-scale at which we lerp between the current position and the drag target
        [SerializeField] float dragSpeed = 10f;

        [SerializeField] Transform planeDebugDisplay = null;

        //The plane which we're dragging along, who's origin is the initial pointer collision point against this object's collider; the plane-normal should always be zero, since we're always dragging these doors up/down
        [NonSerialized] Plane? _dragPlane = null;
        //The initial collision point from the camera/reticle ray against this object's collider
        [NonSerialized] Vector3? _dragStart = null;
        //The position this object was at when w started to drag it
        [NonSerialized] Vector3? _dragStartPosition = null;


        //Whether the pointer is currently hovered over this object's collider
        public bool IsHover { get; protected set; }
        //Whether the pointer was clicked while hovering, and is still being held down
        public bool IsDown { get; protected set; }

        public float OpenPercent {
            get {
                return Mathf.Abs((start.position - transform.position).magnitude / (start.position - end.position).magnitude);
            }
            set {
                transform.position = start.position + (end.position-start.position) * Mathf.Clamp(value, 0f, 1f);
            }
        }

        //Normalized direction vector from the main camera to the reticle
        Vector3 CameraToReticleDirection {
            get {
                AssertValidGvrSetup();
                return (PointerReticle.instance.Position - Camera.main.transform.position).normalized;
            }
        }
        //Normalized direction vector from the main camera to the reticle
        Vector3 ReticleToCameraDirection {
            get {
                AssertValidGvrSetup();
                return (Camera.main.transform.position - PointerReticle.instance.Position).normalized;
            }
        }

        //Normalized direction vector from the main camera to the drag starting point
        Vector3 CameraToDragPointDirection {
            get {
                AssertValidGvrSetup();
                return (_dragStart.Value - Camera.main.transform.position.WithY(_dragStart.Value.y)).normalized;
            }
        }

        //Normalized direction vector from the drag starting point to the main camera
        Vector3 DragPointToCameraDirection {
            get {
                AssertValidGvrSetup();
                return (Camera.main.transform.position.WithY(_dragStart.Value.y) - _dragStart.Value).normalized;
            }
        }

        //Ray originating from the main camera, towards the reticle
        Ray CameraToReticleRay {
            get {
                AssertValidGvrSetup();
                return new Ray(Camera.main.transform.position, CameraToReticleDirection);
            }
        }

        void OnAwake() {
            IsHover = false;
            IsDown = false;

            _dragPlane = null;
            _dragStartPosition = null;
            _dragStart = null;
        }

        //Throws some exceptions if something is not set up correctly with the gvr/camera stuff
        void AssertValidGvrSetup() {
            if(Camera.main == null) throw new NullReferenceException("Null main camera");
            if(PointerReticle.instance == null) throw new NullReferenceException("Null pointer reticle");
        }

        ///http://wiki.unity3d.com/index.php/3d_Math_functions
        //Two non-parallel lines which may or may not touch each other have a point on each line which are closest
        //to each other. This function finds those two points. If the lines are not parallel, the function 
        //outputs true, otherwise false.
        public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){
    
            closestPointLine1 = Vector3.zero;
            closestPointLine2 = Vector3.zero;
    
            float a = Vector3.Dot(lineVec1, lineVec1);
            float b = Vector3.Dot(lineVec1, lineVec2);
            float e = Vector3.Dot(lineVec2, lineVec2);
    
            float d = a*e - b*b;
    
            //lines are not parallel
            if(d != 0.0f){
    
                Vector3 r = linePoint1 - linePoint2;
                float c = Vector3.Dot(lineVec1, r);
                float f = Vector3.Dot(lineVec2, r);
    
                float s = (b*f - c*e) / d;
                float t = (a*f - c*b) / d;
    
                closestPointLine1 = linePoint1 + lineVec1 * s;
                closestPointLine2 = linePoint2 + lineVec2 * t;
    
                return true;
            }
    
            else{
                return false;
            }
        }

        //Perform the raycast from the main camera to the pointer reticle, onto the dragging plane
        RaycastHit? DoRaycast() {
            //if there is no start point to drag from, check the actual object collision point
            if(_dragStart == null) {
                var collider = GetComponent<Collider>();
                if(collider != null) {
                    RaycastHit hit;
                    if(collider.Raycast(CameraToReticleRay, out hit, float.MaxValue)) {
                        return hit;
                    }
                }
            //if we've already done a raycast onto the object before and got the collision point,
            //  raycast to a plane facing the camera instead
            }else{

                //get the closest point between the (end-start) line and the pointer ray
                //if those two are parallel, just use the starting distance 
                Vector3 closestPointerRayPoint, closestTrackPoint;
                Vector3 planeOrigin;
                if(ClosestPointsOnTwoLines(out closestPointerRayPoint, out closestTrackPoint, Camera.main.transform.position, CameraToReticleDirection, start.position, (end.position-start.position).normalized)) {
                    planeOrigin = closestTrackPoint;
                }else{
                    float planeDist = (_dragStart.Value - Camera.main.transform.position).magnitude;
                    planeOrigin = Camera.main.transform.position + planeDist * CameraToReticleDirection;
                }
                
                //_dragPlane = new Plane(ReticleToCameraDirection, planeDistTowardsReticle);
                Vector3 up = Vector3.Cross((start.position - end.position).normalized, ReticleToCameraDirection).normalized;
                _dragPlane = new Plane(Vector3.Cross((start.position-end.position).normalized, up), planeOrigin);
                if(planeDebugDisplay != null) {
                    planeDebugDisplay.position = planeOrigin;
                    planeDebugDisplay.eulerAngles = _dragPlane.Value.normal;
                }

                float enter;
                Ray ray = CameraToReticleRay;
                if(_dragPlane.Value.Raycast(ray, out enter)) {
                    RaycastHit hit = new RaycastHit() {
                        point = ray.GetPoint(enter),
                    };
                    return hit;
                }
            }

            return default(RaycastHit?);
        }
        
        Vector3 ProjectPointToSegment(Vector3 linePointA, Vector3 linePointB, Vector3 point) {
            Vector3 line = (linePointB - linePointA);
            float dot = Vector3.Dot(line.normalized, point - linePointA);
            return linePointA + line.normalized * Mathf.Clamp(dot, 0f, line.magnitude);
        }

        void LateUpdate() {

            //These accessors can throw sometimes if something is not set up correctly with the gvr objects in the scene; so wrapping this in a try-cath block
            bool gvrClicked = false;
            bool gvrDown = false;
            try {
                gvrClicked = GvrController.ClickButtonDown;
                gvrDown = GvrController.ClickButton;
            } catch (NullReferenceException) {
                gvrClicked = false;
                gvrDown = false;
            }

            //Only allow setting IsDown to true while hovering
            if(IsHover) {
                IsDown = gvrDown;
            //But allow IsDown afterwards, even if we're not hovering
            }else{
                if(!gvrDown) {
                    IsDown = false;
                }
            }

            //If we're currently holding the pointer down on the object
            if(IsDown) {
                var hit = DoRaycast();
                if(gvrClicked) {
                    if(hit != null) {
                        _dragStart = hit.Value.point;
                    }
                }

                if(_dragStartPosition == null) {
                    _dragStartPosition = transform.position;
                }

                //if everything is good with our draggy variables
                if(_dragStart != null && _dragStartPosition != null && _dragPlane != null) {

                    //difference between the raycast point and the start position
                    Vector3 offset = _dragStart.Value - _dragStartPosition.Value;
                    //Vector3 targetPosition = transform.position.WithY(hit.Value.point.y - offset.y);
                    Vector3 targetPosition = hit.Value.point - offset;

                    //the fraction between 0-1 which we'll lerp to the target position
                    float dragFraction = Mathf.Clamp(Time.deltaTime * dragSpeed, 0f, 1f);
                    transform.position = Vector3.Lerp(transform.position, targetPosition, dragFraction);
                }
            }else{
                _dragStart = null;
                _dragStartPosition = null;
                _dragPlane = null;
            }

            transform.position = ProjectPointToSegment(start.position, end.position, transform.position);

            IsHover = false;
        }

        //Set IsHover to true every frame which we're hovering, then set to false at the end of the frame
        public void OnGvrPointerHover(PointerEventData eventData) {
            IsHover = true;
        }
    }
}