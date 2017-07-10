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

//provides hookups for various triggers for each instrument, to be invoked by the higher animation controller
public class Drum : MonoBehaviour {

    public Action OnClick { get; set; }

    const string ANIM_STATE_IDLE         = "idle";
    const string ANIM_STATE_DOWN         = "down";
    const string ANIM_TRIGGER_DOWN       = "down";
    const string ANIM_TRIGGER_FILL_KICK  = "fill_kick";
    const string ANIM_TRIGGER_FILL_HIHAT = "fill_hihat";
    const string ANIM_TRIGGER_FILL_SNARE = "fill_snare";
    const string ANIM_TRIGGER_FILL_STEAM = "fill_steam";

    [SerializeField] Animator[] targetAnimators = null;
    [SerializeField] GvrAudioSource audioSource = null;
    [SerializeField] float volume_default = 1f;
    [SerializeField] float volume_duringFill = 0f;
    [SerializeField] DrumType fillMask = DrumType.ALL;

    [Flags]
    public enum DrumType {
        None  = 0,
        Kick  = 1,
        Hihat = 2,
        Snare = 4,
        Steam = 8,

        KICK_HIHAT        = Kick  | Hihat,
        KICK_SNARE        = Kick  | Snare,
        HIHAT_SNARE       = Hihat | Snare,
        HIHAT_STEAM       = Hihat | Steam,
        SNARE_STEAM       = Snare | Steam,
        KICK_HIHAT_SNARE  = Kick  | Hihat | Snare,
        KICK_SNARE_STEAM  = Kick  | Snare | Steam,
        HIHAT_SNARE_STEAM = Hihat | Snare | Steam,
        ALL               = Kick  | Hihat | Snare | Steam,
    }

    bool _isFilling = true;
    bool _isAutoplaying = false;

    void _InvokeTrigger(string trigger) {
        if(trigger == ANIM_TRIGGER_DOWN && _isFilling) {
            //do nothing if we're filling and try to hit down
        }else{
            if(targetAnimators != null) {
                foreach(var anim in targetAnimators) {
                    if(anim != null) {
                        anim.SetTrigger(trigger);
                    }
                }
            }
        }
    }

    void Update() {

        if(_isAutoplaying) {
            var state = targetAnimators[0].GetCurrentAnimatorStateInfo(0);
            if(state.IsName(ANIM_STATE_IDLE) || state.IsName(ANIM_STATE_DOWN)) {
                _isFilling = false;
                audioSource.volume = volume_default;
            }else{
                _isFilling = true;
                audioSource.volume = volume_duringFill;
            }
        }else{
            audioSource.volume = 0f;
        }
    }

    public void EngageAutoplay() {
        _isAutoplaying = true;
        //something else?
    }

    public void DisengageAutoplay() {
        _isAutoplaying = true;
        //something else?
    }

    public void Play_Down() {
        _InvokeTrigger(ANIM_TRIGGER_DOWN);
    }

    public void Play_Fill_Kick() {
        if((fillMask & DrumType.Kick) != 0) {
            _InvokeTrigger(ANIM_TRIGGER_FILL_KICK);
        }
    }

    public void Play_Fill_Hihat() {
        if((fillMask & DrumType.Hihat) != 0) {
            _InvokeTrigger(ANIM_TRIGGER_FILL_HIHAT);
        }
    }

    public void Play_Fill_Snare() {
        if((fillMask & DrumType.Snare) != 0) {
            _InvokeTrigger(ANIM_TRIGGER_FILL_SNARE);
        }
    }

    public void Play_Fill_Steam() {
        if((fillMask & DrumType.Steam) != 0) {
            _InvokeTrigger(ANIM_TRIGGER_FILL_STEAM);
        }
    }
}