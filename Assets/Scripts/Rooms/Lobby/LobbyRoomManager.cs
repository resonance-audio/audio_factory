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
using System.Collections;
using UnityEngine;

namespace AudioEngineer.Rooms.Lobby {

    public class LobbyRoomManager : BaseRoomManager {

        //TODO: in a hurry to finish this for the JAN deadline, most of this should be torn out into its own behaviour

        new public static LobbyRoomManager instance {
            get; protected set;
        }


        const string ANIM_PROP_PLAYING = "playing";
        const string ANIM_PROP_PROCEED = "proceed";

        [SerializeField] InteractableButton screenButton = null;
        [SerializeField] GvrAudioSource screenAudio = null;
        [SerializeField] Animator screenAnimator = null;

        [NonSerialized] bool _completed = false;

        protected override void Awake() {
            
            base.Awake();
            instance = this;

        }

        void OnEnable() {
            screenButton.OnPressed = Pressed;
            screenAnimator.SetBool(ANIM_PROP_PLAYING, false);
        }

        void Pressed() {
            screenAnimator.SetBool(ANIM_PROP_PLAYING, true);
            screenAudio.Play();
            screenButton.OnPressed = null;

            StartCoroutine(_Delayed_Complete());
        }

        IEnumerator _Delayed_Complete() {
            var frame = new WaitForEndOfFrame();

            //for some reason, changing focus on the app will cause audio.isPlaying to return false, even if it's still playing. I'm fudging a grace-period of 5 frames to compensate
            bool donePlaying = false;
            while(!donePlaying) {
                while(screenAudio.isPlaying) yield return frame;

                donePlaying = true;
                for(int i=0; i<5; ++i) {
                    if(screenAudio.isPlaying) {
                        donePlaying = false;
                        break;
                    }
                    yield return frame;
                }
            }
            
            screenAnimator.SetBool(ANIM_PROP_PLAYING, false);
            
			VOSequencer.AddEvent("01_LOBBY_controlalt_1");
            _completed = true;

        }
        
        protected override void Update() {
            base.Update();
            if(screenAnimator != null) {
                screenAnimator.SetBool(ANIM_PROP_PROCEED, elevator.IsMoving);
            }
        }

        public override RoomState State {
            get {
                return _completed ? RoomState.Complete : RoomState.Incomplete;
            }
        }
    }
}