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

using UnityEngine;
using System;

//Rotates the component's Transform according to which direction it is displaced between Updates
public class FaceDirection : MonoBehaviour {

//-----------------------------------------------------------------------------------------
#region SERIALIZED_FIELDS
    //The up-vector
    [SerializeField] public Vector3 up = Vector3.up;
    //What fraction to lerp to the target rotation every Update()
    [SerializeField, RangeAttribute(0f, 1f)] public float lerp = 1f;
    //The minimum effective velocity
    [SerializeField] public float minVelocity = 0f;
    //The maximum effective velocity
    [SerializeField] public float maxVelocity = 0f;
#endregion SERIALIZED_FIELDS

    [NonSerialized] Vector3 _previousPosition = Vector3.zero;

//-----------------------------------------------------------------------------------------
#region METHODS
    void OnEnable() {
        _previousPosition = transform.position;
    }

    void OnValidate() {
        if(minVelocity < 0f) minVelocity = 0f;
        if(maxVelocity < 0f) maxVelocity = 0f;
        
        if(maxVelocity < minVelocity) {
            maxVelocity = minVelocity;
        }
    }

    void Update() {
        float lerpMultiplier = 1f;
        float velocityMagnitude = (transform.position - _previousPosition).magnitude;
        if(velocityMagnitude < minVelocity) {
            return;
        }else if(velocityMagnitude >= maxVelocity) {
            lerpMultiplier = 1f;
        }else{
            if(minVelocity == maxVelocity) {
                lerpMultiplier = 1f;
            }else{
                lerpMultiplier = (velocityMagnitude - minVelocity) / (maxVelocity - minVelocity);
            }
        }

        if(transform.position != _previousPosition) {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.position - _previousPosition, up), lerp * lerpMultiplier);
            _previousPosition = transform.position;
        }
    }
#endregion METHODS
}