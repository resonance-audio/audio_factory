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

namespace AudioEngineer.Rooms.Finale {
    public class FinaleGvrRoomController : MonoBehaviour {
        [Serializable]
        public struct Door {
            [SerializeField] public FinaleDoor door;
            [SerializeField] public Vector3 roomSize;
            [SerializeField] public float gain;
            [SerializeField] public float brightness;
            [SerializeField] public float time;
            public float OpenPercent {
                get {
                    return door != null ? door.OpenPercent : 0f;
                }
            }
        }

        [SerializeField] GvrAudioRoom room = null;
        [SerializeField] Door[] doors = null;

        void Update() {
            if(room == null || doors == null || doors.Length == 0) { return; }

            float openFraction = 1f;
            Vector3 size = doors[0].roomSize;
            float gain = doors[0].gain;
            float brightness = doors[0].brightness;
            float time = doors[0].time;
            
            for(int d=0; d<doors.Length-1; ++d) {
                openFraction *= doors[d].OpenPercent;

                Vector3 sizeDiff = doors[d+1].roomSize - doors[d].roomSize;
                float gainDiff = doors[d+1].gain - doors[d].gain;
                float brightnessDiff = doors[d+1].brightness - doors[d].brightness;
                float timeDiff = doors[d+1].time - doors[d].time;
                
                size += sizeDiff * openFraction;
                gain += gainDiff * openFraction;
                brightness += brightnessDiff * openFraction;
                time += timeDiff * openFraction;
            }

            room.size = size;
            room.reverbGainDb = gain;
            room.reverbBrightness = brightness;
            room.reverbTime = time;
        }
    }
}