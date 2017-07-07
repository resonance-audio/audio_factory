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

public class Rotate : MonoBehaviour {
    
    [SerializeField] public Mode mode = Mode.Update;
    [SerializeField] public Operation operation = Operation.Additive;
    [SerializeField] public Vector3 speed = Vector3.zero;
    [SerializeField] public Vector3 multiplier = Vector3.one;
    [SerializeField] public Vector3 offset = Vector3.zero;
    [SerializeField] public bool local = false;

    [Serializable]
    public enum Operation {
        Additive = 0,
        Absolute = 1,
        AdditiveSin = 2,
        AbsoluteSin = 3,
    }

    [Serializable]
    public enum Mode {
        Update,
        LateUpdate,
    }

    void Update() {
        if(mode == Mode.Update) {
            DoUpdate();
        }
    }

    void LateUpdate() {
        if(mode == Mode.LateUpdate) {
            DoUpdate();
        }
    }

    void DoUpdate() {
        Vector3 eulers = transform.eulerAngles;
        switch(operation) {

            //additive
            default: case Operation.Additive:
            if(speed.x != 0f) {
                var rot = Quaternion.AngleAxis(multiplier.x * speed.x * Time.deltaTime, Vector3.right);
                if(local) transform.localRotation *= rot;
                else transform.rotation *= rot;
            }
            if(speed.y != 0f) {
                var rot = Quaternion.AngleAxis(multiplier.y * speed.y * Time.deltaTime, Vector3.up);
                if(local) transform.localRotation *= rot;
                else transform.rotation *= rot;
            }
            if(speed.z != 0f) {
                var rot = Quaternion.AngleAxis(multiplier.z * speed.z * Time.deltaTime, Vector3.forward);
                if(local) transform.localRotation *= rot;
                else transform.rotation *= rot;
            }
            break;

            //absolute
            case Operation.Absolute: {
            var rot = (Vector3.Scale(multiplier, speed)) * Time.timeSinceLevelLoad + offset;
            if(local) transform.localEulerAngles = rot;
            else transform.eulerAngles = rot;
            }
            break;
            
            //additive sin
            case Operation.AdditiveSin:
            if(speed.x != 0f) {
                var rot = Quaternion.AngleAxis(multiplier.x * Mathf.Sin(speed.x * Mathf.PI * Time.timeSinceLevelLoad), Vector3.right);
                if(local) transform.localRotation *= rot;
                else transform.rotation *= rot;
            }
            if(speed.y != 0f) {
                var rot = Quaternion.AngleAxis(multiplier.y * Mathf.Sin(speed.y * Mathf.PI * Time.timeSinceLevelLoad), Vector3.up);
                if(local) transform.localRotation *= rot;
                else transform.rotation *= rot;
            }
            if(speed.z != 0f) {
                var rot = Quaternion.AngleAxis(multiplier.z * Mathf.Sin(speed.z * Mathf.PI * Time.timeSinceLevelLoad), Vector3.forward);
                if(local) transform.localRotation *= rot;
                else transform.rotation *= rot;
            }
            break;

            //absolute sin
            case Operation.AbsoluteSin: {
                var rot = Vector3.Scale(multiplier, VectorUtil.Sin(speed*Mathf.PI*Time.timeSinceLevelLoad)) + offset;
                if(local) transform.localEulerAngles = rot;
                else transform.eulerAngles = rot;
            }
            break;
        }
    }
}