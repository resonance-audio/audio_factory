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
using UnityEngine;

namespace AudioEngineer.Rooms.Finale {
    //Represents a group of instruments which 
    [ExecuteInEditMode]
    public class InstrumentGroup : BaseInstrumentGroup {

    //---------------------------------------------------------------------------
    #region SERIALIZED_FIELDS
        //instruments which belong to this group; values generated according to {Instrument.parentGroup}
        [SerializeField, ReadOnly] Instrument[] instruments = null;
        [SerializeField] bool allowAutoPlay = false;
        [SerializeField] public bool syncInstrumentsAutoPlay = true;
        [SerializeField] public float idleTimeUntilAutoPlay = 3f;
        [SerializeField] public bool loop = false;
    #endregion SERIALIZED_FIELDS

    //---------------------------------------------------------------------------
    #region NON_SERIALIZED_FIELDS
        [NonSerialized] bool _autoPlay = false;
        [NonSerialized] System.DateTime? _lastUserInput = null;
        [NonSerialized] public int uniquePlays = 0;
    #endregion NON_SERIALIZED_FIELDS

    //---------------------------------------------------------------------------
    #region PROPERTIES

        public bool Loop {
            get { return loop; }
            set { loop = value; }
        }

        public override int UniquePlays {
            get { return uniquePlays; }
            set { uniquePlays = value; }
        }

        public bool AllowAutoPlay {
            get { return allowAutoPlay; }
            set { 
                if(AutoPlay && !value) {
                    AutoPlay = false;
                }
                allowAutoPlay = value;
            }
        }

        //Whether this instrument group is auto-playing or not
        public bool AutoPlay {
            get {
                return _autoPlay && AllowAutoPlay;
            }
            set {
                bool changed = _autoPlay != value;
                _autoPlay = value;
                if(value) {
                    _lastUserInput = null;
                }
                if(changed) {
                    foreach(var child in instruments) {
                        if(child != null) {
                            child.RefreshPlayStatus();
                        }
                    }
                }
            }
        }

        public float PlaybackTime {
            get {
                var man = FinaleRoomManager.instance;
                return man != null ? man.SynchronizedPlaybackTime : Time.realtimeSinceStartup;
            }
        }
    #endregion PROPERTIES

    //---------------------------------------------------------------------------
    #region METHODS

        protected void Update() {
            instruments = instruments ?? ArrayUtil.Empty<Instrument>();

            //if in editor, sync the serialized instruments array
            if(!Application.isPlaying) {
                _RefreshInstrumentArray();
            }

            RefreshAutoPlayStatus();
        }

        public void SyncChildren() {
            foreach(var inst in instruments) {
                inst.SyncWithInstrumentGroup();
            }
        }


        public void RefreshAutoPlayStatus() {
            //update lastUserInput to the most recent value among all child instruments
            _lastUserInput = null;
            for(int i=0; i<instruments.Length; ++i) {
                if(instruments[i] == null) continue;
                if(!_lastUserInput.HasValue || (instruments[i].LastUserInput.HasValue && instruments[i].LastUserInput.Value > _lastUserInput)) {
                    _lastUserInput = instruments[i].LastUserInput;
                }
            }

            //If we've never interacted with the instrument group, or the time since we've last interacted with it has surpassed the idleTimeUntilAutoPlay value, set AutoPlay to true
            AutoPlay = !_lastUserInput.HasValue || (float)(System.DateTime.UtcNow - _lastUserInput.Value).TotalSeconds >= idleTimeUntilAutoPlay;
        }

        //Add/remove instruments which reference this group as their parent
        //A little more work is involved so that instruments aren't required to be
        //  a child of this group
        void _RefreshInstrumentArray() {
            List<Instrument> newInstruments = new List<Instrument>(instruments);
            foreach(var instrument in GetComponentsInChildren<Instrument>()) {
                if(instrument != null) {
                    if(instrument.parentGroup == this && !newInstruments.Contains(instrument)) {
                        newInstruments.Add(instrument);
                    }
                }
            }
            
            for(int i=newInstruments.Count-1; i>=0; --i) {
                if(newInstruments[i] == null || newInstruments[i].parentGroup != this) {
                    newInstruments.RemoveAt(i);
                    continue;
                }
            }

            instruments = newInstruments.ToArray();
        }
    #endregion METHODS
    }
}