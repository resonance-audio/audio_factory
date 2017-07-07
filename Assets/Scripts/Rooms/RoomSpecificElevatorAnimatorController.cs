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

public abstract class RoomSpecificElevatorAnimatorController : MonoBehaviour {
    public static RoomSpecificElevatorAnimatorController instance { get; protected set; }

    [SerializeField] RuntimeAnimatorController runtimeController = null;
    [NonSerialized] Animator _roomSpecificAnimator = null;
    
    //return the animator on this controller's object
    protected Animator RoomSpecificAnimator {
        get {
            if(_roomSpecificAnimator == null) {
                _roomSpecificAnimator = gameObject.GetOrAddComponent<Animator>();
            }
            return _roomSpecificAnimator;
        }
    }

    //copy values from the prefab component to the runtime component, since we're don't want to create the entire object
    public virtual void Initialize(RoomSpecificElevatorAnimatorController prefab) {
        instance = this;
        this.runtimeController = prefab.runtimeController;
        if(RoomSpecificAnimator != null) {
            RoomSpecificAnimator.runtimeAnimatorController = runtimeController;
            RoomSpecificAnimator.Rebind();
        }
    }
}