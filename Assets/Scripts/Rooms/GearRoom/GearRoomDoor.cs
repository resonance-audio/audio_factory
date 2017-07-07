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
    public class GearRoomDoor : MonoBehaviour {
        [SerializeField] GearRoomManager manager = null;

        [SerializeField] DoorOpener doorOpener;
        [SerializeField] SplineMover mover;
        [SerializeField] SplinePath path;

        [SerializeField] float slamSoundMultiplier = 0f;
        [SerializeField] float slamSoundMax = 1f;
        [SerializeField] GvrAudioSource[] topSlamSound = null;
        [SerializeField] GvrAudioSource[] bottomSlamSound = null;


        [NonSerialized] bool hasBeenOpened = false;

        [NonSerialized] float lastProgress = 0;
        [NonSerialized] float endStrength = 0;

        void Update() {
            if(doorOpener == null || mover == null || path == null) return;

            if (!hasBeenOpened) {
                if (mover._time > 0.75f) {
                    hasBeenOpened = true;
                    manager.ObjectiveCompleted("DoorOpened");
                }
            }

            float currentProgress = doorOpener.Progress;

            float diff = Mathf.DeltaAngle(lastProgress, currentProgress);
            mover._time = currentProgress;

            //starting sound
            if ((lastProgress <= 0 && currentProgress > 0) || (lastProgress >= 1 && currentProgress < 1)) {
                foreach(var sound in bottomSlamSound) sound.PlayOneShot(sound.clip);
            }
            foreach(var sound in bottomSlamSound) sound.volume = Mathf.Min(slamSoundMax, Mathf.Abs(diff * slamSoundMultiplier));


            if ((currentProgress <= 0 && lastProgress > 0) || (currentProgress >= 1 && lastProgress < 1)) {
                foreach(var sound in topSlamSound) sound.PlayOneShot(sound.clip);
                endStrength = diff;
            }
            foreach(var sound in topSlamSound) sound.volume = Mathf.Min(slamSoundMax, Mathf.Abs(endStrength * slamSoundMultiplier));


            lastProgress = currentProgress;
        }
    }
}