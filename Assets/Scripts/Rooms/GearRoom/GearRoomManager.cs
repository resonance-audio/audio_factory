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

﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace AudioEngineer.Rooms.Gears {
    public class GearRoomManager : BaseRoomManager {

        new public static GearRoomManager instance {
            get; protected set;
        }

		[SerializeField] GearRoomObjective[] objectives = null;
		[SerializeField] GearRoomObjective objective_pulledLever = null;
        [SerializeField] GearRoomObjective objective_crankedCrank = null;
		//invoked on the frame which all objectives are completed
		[SerializeField] public UnityEvent onCompleteObjectives = null;
		//invoked on the frame which we were previously complete, but now we aren't
		[SerializeField] public UnityEvent onReverseCompleteObjectives = null;

		[NonSerialized] RoomState _lastRoomState = RoomState.Incomplete;

		public override RoomState State {
			get {
				if(objectives == null) return RoomState.Complete;
				for(int i=0; i<objectives.Length; ++i) {
						if(objectives[i] != null && !objectives[i].Complete) {
							return RoomState.Incomplete;
						}
				}
				return RoomState.Complete;
			}
		}

		public bool ObjectiveComplete_PulledLever {
			get {
				return objective_pulledLever != null && objective_pulledLever.Complete;
			}
		}

		public bool ObjectiveComplete_CrankedCrank {
			get {
				return objective_crankedCrank != null && objective_crankedCrank.Complete;
			}
		}

		public int CompletedObjectiveCount {
			get {
				int ret = 0;
				for(int i=0; i<objectives.Length; ++i) {
					if(objectives[i] != null && objectives[i].Complete) {
						++ret;
					}
				}
				return ret;
			}
		}

        public int ObjectivesLeft {
            get {
                int remaining = objectives.Length;
                for(int i=0; i<objectives.Length; ++i) {
                    if(objectives[i] != null && objectives[i].Complete) {
                        remaining--;
                    }
                }
                return remaining;
            }
        }

		protected override void Awake() {
			base.Awake();
			instance = this;
            objective_pulledLever.SetManager(this);
            objective_crankedCrank.SetManager(this);
		}

        protected override void InRoom() {
			base.InRoom();
			VOSequencer.AddEvent("08_BOILERROOM_intro_1");
        }

        public void ObjectiveCompleted(string name) {

            if (name.Equals("DoorOpened")) {
				VOSequencer.AddEvent("08_BOILERROOM_2ndcrank_1");
            }
            if (name.Equals("CrankObjective")) {
                VOSequencer.ReplaceEvent("pulllever");
            }
            if (name.Equals("LeverObjective")) {
                VOSequencer.ReplaceEvent("05_lobbyBot_dry_VO1_B__boilerroom_v2");
            }

        }

        protected override void Update() {
            base.Update();
			RoomState curr = State;
			if(curr == RoomState.Complete && _lastRoomState == RoomState.Incomplete) {
				if(onCompleteObjectives != null) {
					onCompleteObjectives.Invoke();
				}
			}else if(curr == RoomState.Incomplete && _lastRoomState == RoomState.Complete) {
				if(onReverseCompleteObjectives != null) {
					onReverseCompleteObjectives.Invoke();
				}
			}
			
			_lastRoomState = curr;
		}
	}
}