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

public class GreenhouseElevatorAnimatorController : RoomSpecificElevatorAnimatorController {
    new public static GreenhouseElevatorAnimatorController instance { get; protected set; }
    const string ANIM_PROP_PROGRESS = "progress";

    //initialize/copy fields for this component
    public override void Initialize(RoomSpecificElevatorAnimatorController prefab) {
        base.Initialize(prefab);
        instance = this;
    }

    //update the basement progress
    void Update() {
        if(RoomSpecificAnimator == null) return;
        var man = GreenHouseRoomManager.instance;
        if(man == null) return;

        RoomSpecificAnimator.SetInteger(ANIM_PROP_PROGRESS, man.Progress);
    }
}