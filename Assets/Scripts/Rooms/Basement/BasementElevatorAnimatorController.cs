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


namespace AudioEngineer.Rooms.Basement {
    public class BasementElevatorAnimatorController : RoomSpecificElevatorAnimatorController {
        new public static BasementElevatorAnimatorController instance { get; protected set; }
        const string ANIM_PROP_PROGRESS = "progress";

        //This is a bit unconventional for initializing "prefabs". The point of this class is to control an animator which we create at runtime on a specific transform. Since animated values in Unity are referenced by object path/names in the scene hierarchy, so we can't just do a GameObject.Instantiate. Instead, we need to create a new component and copy the values from the prefab. Reflection or ScriptableObjects would also be viable solutions for this.
        public override void Initialize(RoomSpecificElevatorAnimatorController prefab) {
            base.Initialize(prefab);
            instance = this;
        }

        //update the basement progress
        void Update() {
            if(RoomSpecificAnimator == null) return;
            var man = BasementRoomManager.instance;
            if(man == null) return;

            RoomSpecificAnimator.SetInteger(ANIM_PROP_PROGRESS, man.CompletedGeneratorCount);
        }
    }
}