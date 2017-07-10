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

//UnityEvent-wrapper with some facilities for invoking when two objects are close enough together
public class ProximityInvoker : MonoBehaviour {
    
//-----------------------------------------------------------------------------------------
#region INNER_DEFINITIONS
    [Serializable]
    public enum Mode {
        InvokeEachTime   = 0,
        InvokeEveryFrame = 1,
        InvokeOnlyOnce   = 2,
    }

    [Serializable]
    public enum ResetOn {
        Never    = 0,
        OnEnable = 1,
        Awake    = 2,
        Start    = 4,
    }
#endregion INNER_DEFINITIONS

//-----------------------------------------------------------------------------------------
#region SERIALIZED_FIELDS
    [SerializeField] public Transform target = null;
    [SerializeField] public float requiredDistance = 0f;
    [SerializeField] public Mode mode = Mode.InvokeEachTime;
    [SerializeField] public UnityEvent onEnter = null;
    [SerializeField] public UnityEvent onExit = null;
    [SerializeField] public ResetOn resetOn = ResetOn.OnEnable;
#endregion SERIALIZED_FIELDS

//-----------------------------------------------------------------------------------------
#region NON_SERIALIZED_FIELDS
    [NonSerialized] int timesInvokedEnter = 0;
    [NonSerialized] int timesInvokedExit = 0;
    [NonSerialized] bool insideLastFrame = false;
#endregion NON_SERIALIZED_FIELDS


//-----------------------------------------------------------------------------------------
#region METHODS
    void OnEnable() {
        if((resetOn & ResetOn.OnEnable) != 0) {
            Reset();
        }
    }

    void Awake() {
        if((resetOn & ResetOn.Awake) != 0) {
            Reset();
        }
    }

    void Start() {
        if((resetOn & ResetOn.Start) != 0) {
            Reset();
        }
    }

    void Reset() {
        timesInvokedEnter = 0;
        timesInvokedExit = 0;
        insideLastFrame = false;
    }

    protected virtual void Update() {
        if(target == null) return;
        float dist = (target.position - transform.position).magnitude;
        bool inside = dist <= requiredDistance;

        if(inside) {
            switch(mode) {

                //if we only want to invoke once upon entering
                default: case Mode.InvokeEachTime: 
                if(!insideLastFrame) {
                    InvokeEnter();
                }
                break;

                case Mode.InvokeEveryFrame:
                InvokeEnter();
                break;

                case Mode.InvokeOnlyOnce:
                if(timesInvokedEnter == 0) {
                    InvokeEnter();
                }
                break;
            }
        }else{
            switch(mode) {
                //if we only want to invoke once upon entering
                default: case Mode.InvokeEachTime: 
                if(insideLastFrame) {
                    InvokeExit();
                }
                break;

                case Mode.InvokeEveryFrame:
                InvokeExit();
                break;

                case Mode.InvokeOnlyOnce:
                if(timesInvokedExit == 0) {
                    InvokeExit();
                }
                break;
            }
        }

        insideLastFrame = inside;
    }

    void InvokeExit() {
        ++timesInvokedExit;

        if(onExit != null) {
            onExit.Invoke();
        }
    }

    void InvokeEnter() {
        ++timesInvokedEnter;

        if(onEnter != null) {
            onEnter.Invoke();
        }
    }
#endregion METHODS
}