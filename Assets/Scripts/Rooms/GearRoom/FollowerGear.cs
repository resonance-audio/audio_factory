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

namespace AudioEngineer.Rooms.Gears {

    [ExecuteInEditMode]
    public class FollowerGear : BaseGear {
        [SerializeField] BaseGear masterGear = null;
        [SerializeField] TeethLocking teethLocking = TeethLocking.Lock;
        [SerializeField] float toothLockSpeed = 10f;
        [SerializeField] float reverseMultiplier = 1f;

        public enum TeethLocking {
            Lock = 0, //locked teeth forward direction
            Free = 1, //unlocked teeth forward direction
        }

        [SerializeField, HideInInspector] bool engagedLastFrame = false;
        [SerializeField, HideInInspector] float lastMasterAngle = 0f;

        //TODO: make this work on different axes
        //Hardmode: make this work at any rotation

        protected void Update() {
            if(masterGear != null && !(masterGear.isInternalGear && this.isInternalGear)) {

                Vector2 dist = (masterGear.transform.position.XY() - transform.position.XY());

                bool reverse = masterGear.isInternalGear || this.isInternalGear;
                //whether the gear teeth are touching
                bool engaged = (masterGear.isInternalGear || this.isInternalGear)
                    ? (dist.magnitude + this.engageDistance >= masterGear.engageDistance)
                    : (dist.magnitude <= (engageDistance + masterGear.engageDistance));

                if(engaged) {
                    //the ratio between the master gear count and this gear count
                    float gearRatio = (float)masterGear.teethCount / (float)this.teethCount;
                    //constant which is generally multiplied by the angle-difference
                    float gearConstant = 1f + (1f/gearRatio);

                    //the angle-difference between the centerpoints of each gear
                    float angleDifference = Mathf.Atan2(masterGear.transform.position.y - transform.position.y, masterGear.transform.position.x - transform.position.x) * 180f / Mathf.PI;

                    //the master angle this frame
                    float masterAngle = Mathf.MoveTowardsAngle(masterGear.transform.eulerAngles.z, masterGear.transform.eulerAngles.z - angleDifference * gearConstant, float.MaxValue);

                    if(!engagedLastFrame) {
                        lastMasterAngle = masterAngle;
                    }

                    //the difference between this frame's master angle and the last frame's
                    float masterAngleDiff = Mathf.DeltaAngle(lastMasterAngle, masterAngle);
                    
                    //We generally want to add the master angle difference each frame, NOT just set the rotation to the current master angle. This way, the gears may rotate independently if they are not engaged, and then rotate appropriately when re-engaged at a different starting angle.
                    transform.localEulerAngles += (reverse ? Vector3.forward : Vector3.back) * gearRatio * masterAngleDiff * (reverse?reverseMultiplier:1f);
                    lastMasterAngle = masterAngle;

                    //If we're in standard-forward mode, find the smallest delta between the two gears' teeth, and add a fraction of the delta each frame, so that the gears lock together properly
                    if(teethLocking == TeethLocking.Lock) {
                        float finalToothLockingDelta = 0f;
                        if(reverse) {
                            float teethAngle = 360f/(float)teethCount;
                            //the absolute angle difference between the two gears
                            float gearDelta = -transform.eulerAngles.z + angleDifference*gearConstant*gearRatio + masterGear.transform.eulerAngles.z*gearRatio - teethOffset - masterGear.teethOffset*gearRatio;
                            //take the modulo of this according to the tooth angle
                            finalToothLockingDelta = gearDelta % teethAngle;
                            if(finalToothLockingDelta > teethAngle/2f) finalToothLockingDelta = finalToothLockingDelta - teethAngle;

                            float lerpFactor = Application.isPlaying
                                ? Mathf.Clamp(Time.deltaTime * toothLockSpeed, 0f, 1f)
                                : 1f;
                            
                            finalToothLockingDelta *= lerpFactor;
                        }else{
                            float teethAngle = 360f/(float)teethCount;
                            //the absolute angle difference between the two gears
                            float gearDelta = transform.eulerAngles.z - angleDifference*gearConstant*gearRatio + masterGear.transform.eulerAngles.z*gearRatio + teethOffset + masterGear.teethOffset*gearRatio;
                            //take the modulo of this according to the tooth angle
                            finalToothLockingDelta = gearDelta % teethAngle;
                            if(finalToothLockingDelta > teethAngle/2f) finalToothLockingDelta = finalToothLockingDelta - teethAngle;

                            float lerpFactor = Application.isPlaying
                                ? Mathf.Clamp(Time.deltaTime * toothLockSpeed, 0f, 1f)
                                : 1f;
                            
                            finalToothLockingDelta *= lerpFactor;
                        }

                        //do the additional rotation to our transform
                        transform.localEulerAngles += Vector3.back * finalToothLockingDelta;
                    }

                    engagedLastFrame = true;
                }else{
                    engagedLastFrame = false;
                }
            }
        }
    }
}