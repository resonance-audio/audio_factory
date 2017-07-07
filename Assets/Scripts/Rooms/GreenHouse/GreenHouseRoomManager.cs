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


public class GreenHouseRoomManager : BaseRoomManager {
    
    new public static GreenHouseRoomManager instance {
        get; protected set;
    }

    [NonSerialized] bool _isCompleted = false;
    public override RoomState State {
        get {
            return _isCompleted ? RoomState.Complete : RoomState.Incomplete;
        }
    }

    public GameObject elevatorShaftSounds;

    void OnEnable() {
        _isCompleted = false;
        instance = this;
    }

    protected override void InRoom() {
        base.InRoom();
        VOSequencer.AddEvent("welcomegreenhouse_2");
        VOSequencer.AddEvent("turnhandle");
    }

    public void Complete() {
        elevatorShaftSounds.SetActive(true);
        _isCompleted = true;
    }

    int progress = 0;

    public int Progress {
        get { return progress; }
        set { progress = value; }
    }

    public ElevatorVOSequencer GetVOSequencer() {
        return gameObject.GetComponent<ElevatorVOSequencer>();
    }
}