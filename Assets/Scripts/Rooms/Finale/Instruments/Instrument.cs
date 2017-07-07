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
using UnityEngine.EventSystems;

namespace AudioEngineer.Rooms.Finale {
    //Base class for all instruments
    [ExecuteInEditMode]
    public abstract class Instrument : MonoBehaviour {

        //the group which this instrument belongs to
        [SerializeField] public InstrumentGroup parentGroup = null;
        [SerializeField] protected GvrAudioSource source = null;
        [SerializeField] protected AudioClip autoPlayClip = null;
        [SerializeField] protected AudioClip manualPlayClip = null;
        [SerializeField] protected int synchronizeFrameFrequency = 60;

        [NonSerialized] protected System.DateTime? _lastUserInput = null;
        [NonSerialized] protected bool _previouslyAutoPlaying = false;
        [NonSerialized] protected int _synchronizeCounter = 60;

        //Set LastUserInput to now; may be invoked via UnityEvents
        public virtual void RecordLastPlayed() {
            LastUserInput = System.DateTime.UtcNow;
            if(parentGroup != null) {
                ++parentGroup.UniquePlays;
                parentGroup.RefreshAutoPlayStatus();
            }
        }

        //Time of the last user-input
        public System.DateTime? LastUserInput {
            get {
                return _lastUserInput;
            }
            protected set {
                _lastUserInput = value;
            }
        }

        public bool AutoPlay {
            get {
                if(parentGroup != null) {
                    if(!parentGroup.AllowAutoPlay) {
                        return false;
                    }else if(parentGroup.syncInstrumentsAutoPlay) {
                        return parentGroup.AutoPlay;
                    }else if(_lastUserInput.HasValue) {
                        return (System.DateTime.UtcNow - _lastUserInput.Value).TotalSeconds
                            >= parentGroup.idleTimeUntilAutoPlay;
                    }else{
                        return true;
                    }
                }else{
                    return false;
                }
            }
        }

        //do some setup on awake
        protected virtual void Awake() {
            //editor awake setup
            #if UNITY_EDITOR
            if(!Application.isPlaying) {
                if(parentGroup == null) {
                    parentGroup = GetComponentInParent<InstrumentGroup>();
                }
            }
            #endif
        }

        void OnEnable() {
            if(AutoPlay) {
                EngageAutoPlay();
            }else{
                DisengageAutoPlay();
            }
        }

        public void RefreshPlayStatus() {
            bool shouldAutoPlay = AutoPlay;
            if(!_previouslyAutoPlaying && shouldAutoPlay) {
                EngageAutoPlay();
            }else if(_previouslyAutoPlaying && !shouldAutoPlay) {
                DisengageAutoPlay();
            }
        }

        public void SyncWithInstrumentGroup() {                    
            _synchronizeCounter = synchronizeFrameFrequency;
            if(source.clip != null) {
                if(parentGroup.loop) {
                    source.time = parentGroup.PlaybackTime % source.clip.length;
                    source.Play();
                }else{
                    if(parentGroup.PlaybackTime < source.clip.length) {
                        source.time = Mathf.Clamp(parentGroup.PlaybackTime, 0f, source.clip.length);
                        source.Play();
                    }else{
                        source.Stop();
                    }
                }
            }
        }

        protected virtual void Update() {
            if(Application.isPlaying) {
                RefreshPlayStatus();
            }

            if(AutoPlay && parentGroup != null) {
                --_synchronizeCounter;
                if(_synchronizeCounter <= 0) {
                    SyncWithInstrumentGroup();
                }
            }
        }

        protected virtual void EngageAutoPlay() {
            _previouslyAutoPlaying = true;
            source.loop = parentGroup.loop;
            source.clip = autoPlayClip;
            if(source.clip != null) {
                if(parentGroup.loop) {
                    source.time = parentGroup.PlaybackTime % autoPlayClip.length;
                    source.Play();
                }else{
                    if(parentGroup.PlaybackTime < autoPlayClip.length) {
                        source.time = Mathf.Clamp(parentGroup.PlaybackTime, 0f, autoPlayClip.length);
                        source.Play();
                    }else{
                        source.Stop();
                    }
                }
            }else{
                source.Stop();
            }
        }

        protected virtual void DisengageAutoPlay() {
            _previouslyAutoPlaying = false;
            source.loop = false;
            source.clip = manualPlayClip;
            if(source.clip != null) {
                source.time = 0f;
            }else{
                source.Stop();
            }
        }
    }
}