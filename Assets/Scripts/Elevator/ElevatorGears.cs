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

﻿using UnityEngine;
using AudioEngineer.Rooms.Gears;

public class ElevatorGears : MonoBehaviour {

    GearRoomManager manager;

    bool gearsOn = false;

	void Start () {
        manager = GearRoomManager.instance;
	}
	
	void Update () {
        if (!gearsOn && manager.State == BaseRoomManager.RoomState.Complete) {
            gearsOn = true;
            GvrAudioSource[] sources = transform.GetComponentsInChildren<GvrAudioSource>();
            foreach(GvrAudioSource source in sources)
                source.enabled = true;
        }
	}
}
