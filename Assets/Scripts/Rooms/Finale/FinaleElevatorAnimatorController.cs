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

namespace AudioEngineer.Rooms.Finale {
    public class FinaleElevatorAnimatorController : RoomSpecificElevatorAnimatorController {
        new public static FinaleElevatorAnimatorController instance { get; protected set; }
        const string ANIM_PROP_COMPLETE = "progress";

        public override void Initialize(RoomSpecificElevatorAnimatorController prefab) {
            base.Initialize(prefab);
            instance = this;
        }

        //update the finale progress
        void Update() {
            if(RoomSpecificAnimator == null) return;
            var man = FinaleRoomManager.instance;
            if(man == null) return;

            RoomSpecificAnimator.SetInteger(ANIM_PROP_COMPLETE, man.Progress);
        }

    }
}