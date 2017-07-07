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
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace AudioEngineer.Rooms.Basement {

    public class BasementRoomManager : BaseRoomManager {

        [SerializeField] public UnityEvent onCompleted = null;

        [NonSerialized] HashSet<Generator> completedGenerators = new HashSet<Generator>();
        [NonSerialized] int requiredGeneratorsToComplete = 0;

        new public static BasementRoomManager instance {
            get; protected set;
        }

        public int CompletedGeneratorCount {
            get {
                return completedGenerators != null
                    ? completedGenerators.Count
                    : 0;
            }
        }

        bool noGeneratorsStarted = true;

        protected override void Awake() {
            base.Awake();
            instance = this;
        }

        protected override void InRoom() {
            base.InRoom();
            VOSequencer.AddEvent("02_lobbyBot_dry_VO1_A_generator_v2");
        }

        void OnEnable() {
            requiredGeneratorsToComplete = 3;
        }

        public void GeneratorTurnedOn(Generator generator) {
            if(!completedGenerators.Contains(generator)) {
                completedGenerators.Add(generator);

                if(completedGenerators.Count >= requiredGeneratorsToComplete) {
                    if(onCompleted != null) {
                        onCompleted.Invoke();
                        VOSequencer.ReplaceEvent("02_lobbyBot_dry_VO1_C__generator_v2");
                    }
                }

                int generatorsLeft = requiredGeneratorsToComplete - completedGenerators.Count;

                if(generatorsLeft == 2) {
                    VOSequencer.AddEvent("twomore_plugs");
                }
                if(generatorsLeft == 1) {
                    VOSequencer.ReplaceEvent("onemore_plug");
                }
            }
            if (noGeneratorsStarted) {
                noGeneratorsStarted = false;
                VOSequencer.AddEvent("02_lobbyBot_dry_VO1_B__generator_v2");
            }
        }

        public override RoomState State {
            get {
                return completedGenerators.Count >= requiredGeneratorsToComplete
                    ? RoomState.Complete
                    : RoomState.Incomplete;
            }
        }
    }
}