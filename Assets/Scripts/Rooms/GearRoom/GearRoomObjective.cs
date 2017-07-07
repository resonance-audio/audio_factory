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

namespace AudioEngineer.Rooms.Gears {

    //Component referenced by GearRoomManager to determine which objectives are completed by the user
    public class GearRoomObjective : MonoBehaviour {

        GearRoomManager manager;

        public enum CompletionMode {
            Manual   = 0,
            OnEnable = 1,
        }
        public enum IncompleteMode {
            Manual   = 0,
            OnDisable = 1,
        }
        
        [SerializeField] CompletionMode mode = CompletionMode.Manual;
        [SerializeField] IncompleteMode incompleteMode = IncompleteMode.Manual;


        bool complete;
        public bool Complete { 
            get {return complete; }
            set {
                complete = value; 
                if (manager != null) {
                    manager.ObjectiveCompleted(transform.name);
                }
            }
        }

        void Awake() {
            Complete = false;
        }

        public void SetManager(GearRoomManager _manager) {
            manager = _manager;
        }

        void OnEnable() {
            if(mode == CompletionMode.OnEnable) {
                Complete = true;
            }
        }
        void OnDisable() {
            if(incompleteMode == IncompleteMode.OnDisable) {
                Complete = false;
            }
        }
    }
}