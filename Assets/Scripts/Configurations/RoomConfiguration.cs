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
using System.Collections.Generic;
using UnityEngine;

//Configuration data class which references some kind of room-layout for the game
//  Unless we employ a non-linear room layout, it's basically just a RoomDefinition[] wrapper
[Serializable]
[CreateAssetMenuAttribute(fileName = "Data", menuName = "AudioEngineer/Config/RoomConfiguration", order = 1)]
public class RoomConfiguration : ScriptableObject {
    [Tooltip("All rooms for the game; index-ZERO being the first-occurring")]
    [SerializeField] public RoomDefinition[] rooms;

    //The very first room defined in this asset
    public RoomDefinition FirstRoom {
        get { return rooms != null && rooms.Length > 0 ? rooms[0] : default(RoomDefinition); }
    }

    //The very last room defined in this asset
    public RoomDefinition LastRoom {
        get { return rooms != null && rooms.Length > 0 ? rooms[rooms.Length-1] : default(RoomDefinition); }
    }

    //Get the previous room in the rooms array. If currentRoom is null, returns the first value
    public RoomDefinition GetPreviousRoom(RoomDefinition currentRoom) {
        if(rooms != null && rooms.Length > 0) {
            if(currentRoom == null) {
                return rooms[0];
            }else{
                for(int i=0; i<rooms.Length; ++i) {
                    if(rooms[i] == currentRoom && i > 0) {
                        return rooms[i-1];
                    }
                }
            }
        }

        return rooms != null && rooms.Length > 0
            ? rooms[rooms.Length-1]
            : default(RoomDefinition);
    }

    //Get the next room in the rooms array. If currentRoom is null, returns the first value
    public RoomDefinition GetNextRoom(RoomDefinition currentRoom) {
        if(rooms != null && rooms.Length > 0) {
            if(currentRoom == null) {
                return rooms[0];
            }else{
                for(int i=0; i<rooms.Length; ++i) {
                    if(rooms[i] == currentRoom && i < (rooms.Length-1)) {
                        return rooms[i+1];
                    }
                }
            }
        }

        return rooms != null && rooms.Length > 0
            ? rooms[0]
            : default(RoomDefinition);
    }
}