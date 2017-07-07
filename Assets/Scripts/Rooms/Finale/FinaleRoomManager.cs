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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace AudioEngineer.Rooms.Finale {
    public class FinaleRoomManager : BaseRoomManager {

        [SerializeField] PlayInstrumentGroupRequirement[] instrumentRequirements = null;
        [SerializeField] UnityEvent onCompletedAllInstrumentRequirements = null;
        [SerializeField] UnityEvent onFinaleSequenceComplete = null;
        [SerializeField] float blackFadeOutTime = 5f;
        [SerializeField] int instrumentFinalPlayalongFadeInFrames = 50;
        [SerializeField] float completeAllInstrumentGroupDelay = 5f;
        [SerializeField] float killAppAfterCompleteDelay = 5f;

        [Serializable]
        public class PlayInstrumentGroupRequirement {
            public Action onCompleted = null;
            [SerializeField] BaseInstrumentGroup group = null;
            [SerializeField] int requiredPlayCount = 1;
            [SerializeField] UnityEvent onCompletedEvent = null;
            [SerializeField] UnityEvent onUniquePlayBeforeActive = null;
            [NonSerialized] bool _hasCompleted = false;
            [NonSerialized] int _lastUniquePlays = 0;

            public void Update() {
                bool completed = IsCompleted;


                if(completed && !_hasCompleted) {
                    if(onCompleted != null) {
                        onCompleted.Invoke();
                    }
                    if(onCompletedEvent != null) {
                        onCompletedEvent.Invoke();
                    }
                    
                    _hasCompleted = true;
                }else if(!completed) {
                    if(_lastUniquePlays != group.UniquePlays) {
                        _lastUniquePlays = group.UniquePlays;
                        if(onUniquePlayBeforeActive != null) {
                            onUniquePlayBeforeActive.Invoke();
                        }
                    }
                }
            }

            public bool IsValid {
                get {
                    return group != null;
                }
            }

            public bool IsCompleted {
                get { return group == null || group.UniquePlays >= requiredPlayCount; }
            }
        }

        [NonSerialized] bool _isPlayingFinalMusic = false;
        [NonSerialized] int _completedInstrumentCount = 0;
        [NonSerialized] float _synchronizedPlaybackStartTime = 0f;
        [NonSerialized] bool _hasCompletedAllInstrumentRequirements = false;

        public float SynchronizedPlaybackTime {
            get { return this._isPlayingFinalMusic ? Time.time - _synchronizedPlaybackStartTime : 0f; }
        }
        public bool IsPlayingFinalMusic {
            get { return _isPlayingFinalMusic; }
        }
        new public static FinaleRoomManager instance { get; protected set; }

        public override BaseRoomManager.RoomState State {
            get { return BaseRoomManager.RoomState.Incomplete; }
        }
        
        protected override void Awake() {
            base.Awake();
            instance = this;
        }

        protected void OnValidate() {
            List<PlayInstrumentGroupRequirement> newReqs = new List<PlayInstrumentGroupRequirement>(instrumentRequirements);
            for(int i=newReqs.Count-1; i>=0; --i) {
                if(!newReqs[i].IsValid) {
                    newReqs.RemoveAt(i);
                    continue;
                }
            }
            instrumentRequirements = newReqs.ToArray();
        }

        protected override void Update() {
            base.Update();

            foreach(var req in instrumentRequirements) {
                req.Update();
            }
            int completedCount = instrumentRequirements.Count(req => req.IsCompleted);
            if(completedCount != _completedInstrumentCount && completedCount < instrumentRequirements.Length) {
                _completedInstrumentCount = completedCount;
            }

            if(_hasCompletedAllInstrumentRequirements) {
            }else{
                //if all instrument groups are complete
                if(!instrumentRequirements.Any(req => !req.IsCompleted)) {
                    StartCoroutine(_FinalSequence());
                }
            }
        }

        protected override void InRoom() {
            base.InRoom();
            VOSequencer.AddEvent("10_TESTROOM_intro_1");
        }

        IEnumerator _FinalSequence() {
            _hasCompletedAllInstrumentRequirements = true;
            VOSequencer.ReplaceEvent("10_TESTROOM_initiatetestseq_1");

            //wait a bit and invoke the "completed-all-requirements" event
            yield return new WaitForSeconds(completeAllInstrumentGroupDelay);
            _isPlayingFinalMusic = true;
            _synchronizedPlaybackStartTime = Time.time;

            //store the volume levels of all audio sources
            Dictionary<GvrAudioSource, float> volumeMap = new Dictionary<GvrAudioSource, float>();
            foreach(var src in GetComponentsInChildren<GvrAudioSource>(true)) {
                volumeMap[src] = src.volume;
                src.volume = 0f;
            }

            if(onCompletedAllInstrumentRequirements != null) {
                onCompletedAllInstrumentRequirements.Invoke();
            }
            VOSequencer.ReplaceEvent("10_TESTROOM_simplebye_1");

            for(int i=0; i<instrumentFinalPlayalongFadeInFrames; ++i) {
                //restore the audio levels of all sources
                foreach(var src in volumeMap) {
                    src.Key.volume = Mathf.Lerp(0, src.Value, (float)(i+1)/(float)instrumentFinalPlayalongFadeInFrames);
                }
                yield return new WaitForEndOfFrame();
            }

            //wait a bit and quit the application
            yield return new WaitForSeconds(killAppAfterCompleteDelay);
            GameManager.instance.IsComplete = true;

            if(onFinaleSequenceComplete != null) {
                onFinaleSequenceComplete.Invoke();
            }
                
        }

        int progress = 0;
        public int Progress {
            get { return progress; }
            set { progress = value; }
        }
            
    }
}