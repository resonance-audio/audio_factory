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

namespace AudioEngineer.Rooms.Basement {
    public class Plug : SimpleDraggable {

        [SerializeField] Rope connectedRope = null;
        [SerializeField, RangeAttribute(0f, 1f)] float snapSpeed = 0.1f;
        [SerializeField] public UnityEvent onClickPressed = null;
        [SerializeField] public UnityEvent onClickReleased = null;
        [NonSerialized] Transform socket = null;

        public bool isPlugged { get; protected set; }

        protected override void OnEnable() {
            base.OnEnable();
            isPlugged = false;
        }

        protected override void ClickPressed() {
            base.ClickPressed();
            
            if(onClickPressed != null) {
                onClickPressed.Invoke();
            }
        }

        protected override void ClickReleased() {
            base.ClickReleased();

            if(onClickReleased != null) {
                onClickReleased.Invoke();
            }
        }

        protected override Vector3 GetTargetPosition() {
            if(socket != null) {
                return socket.transform.position;
            }

            //if this is under a rope and the target position is out of the rope's bounds, constrain it
            Vector3 baseRet = base.GetTargetPosition();
            if(connectedRope != null) {
                float maxLen = connectedRope.RuntimeLength;
                Vector3 start = connectedRope.start.transform.position;
                if(Vector3.Distance(baseRet, start) > maxLen) {
                    baseRet = start + (baseRet - start).normalized * maxLen;
                }
            }

            foreach(var generator in FindObjectsOfType<Generator>()) {
                if(generator != null && Vector3.Distance(baseRet, generator.plugSnapPoint.position) <= generator.plugSnapRadius) {
                    if(!generator.IsOn && generator.IsReady) {
                        generator.IsOn = true;
                        isPlugged = true;
                    }
                    var rb = GetComponent<Rigidbody>();
                    if(rb != null) {
                        rb.useGravity = false;
                    }
                    socket = generator.plugSnapPoint;
                    return socket.transform.position;
                }
            }

            return baseRet;
        }

        protected override void Update() {
            var rb = transform.GetComponent<Rigidbody>();
            if(socket != null) {
                if(rb != null) {
                    rb.velocity = Vector3.zero;
                    transform.position = Vector3.Lerp(transform.position, socket.transform.position, snapSpeed);
                    transform.rotation = Quaternion.Lerp(transform.rotation, socket.transform.rotation, snapSpeed);
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }
            }else{
                base.Update();
                if(isDown) {
                    var rope = GetComponentInParent<Rope>();
                    if(rope != null) {
                        if(rope.end == this || rope.start == this) {
                            rope.ConstrainEndpoints();
                        }
                    }

                    if(rb != null) {
                        rb.angularVelocity = Vector3.zero;
                    }
                }
            }
        }
    }
}