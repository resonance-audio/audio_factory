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

using System.Collections;
using AudioEngineer.Rooms.Finale;
using UnityEngine;

public class TestFinaleDrumkit : BaseInstrumentGroup {
    [SerializeField] Drum[] drums;
    [SerializeField] float hihatFillOffset = 0f;
    [SerializeField] float kickFillOffset = 0f;
    [SerializeField] float snareFillOffset = 0f;
    [SerializeField] float steamFillOffset = 0f;

    [SerializeField] float hihatQuantization = 0f;
    [SerializeField] float kickQuantization = 0f;
    [SerializeField] float snareQuantization = 0f;
    [SerializeField] float steamQuantization = 0f;

    [SerializeField] float hihatFillLength = 1f;
    [SerializeField] float kickFillLength = 1f;
    [SerializeField] float snareFillLength = 1f;
    [SerializeField] float steamFillLength = 1f;

    Coroutine delayedFill = null;

    public override int UniquePlays { get; set; }

    public void EngageAutoplay() {
        foreach(var drum in drums) {
            drum.EngageAutoplay();
        }
    }

    public void DisengageAutoplay() {
        foreach(var drum in drums) {
            drum.DisengageAutoplay();
        }
    }

    void Fill(FillType type) {
        if(delayedFill != null) return;
            
        float fillOffset;
        float quant;
        switch(type) {

            default: case FillType.Hihat:
            fillOffset = hihatFillOffset;
            quant = hihatQuantization;
            break;

            case FillType.Kick:
            fillOffset = kickFillOffset;
            quant = kickQuantization;
            break;

            case FillType.Snare:
            fillOffset = snareFillOffset;
            quant = snareQuantization;
            break;

            case FillType.Steam:
            fillOffset = steamFillOffset;
            quant = steamQuantization;
            break;
        }

        float delay;
        //don't do quantization unless we're playing the finale music
        if(FinaleRoomManager.instance.IsPlayingFinalMusic) {
            delay = quant - fillOffset - (quant <= 0f ? 0f : (FinaleRoomManager.instance.SynchronizedPlaybackTime) % quant);
        }else{
            delay = 0f;
        }

        delayedFill = StartCoroutine(_FillRoutine(type, delay));
    }

    public void Fill_Kick() {
        Fill(FillType.Kick);
    }
    
    public void Fill_Snare() {
        Fill(FillType.Snare);
    }

    public void Fill_Hihat() {
        Fill(FillType.Hihat);
    }

    public void Fill_Steam() {
        Fill(FillType.Steam);
    }

    enum FillType {
        Kick = 0,
        Snare = 1,
        Hihat = 2,
        Steam = 3,
    }

    IEnumerator _FillRoutine(FillType type, float delay) {
        float fillLength;

        if(delay > 0f) {
            yield return new WaitForSeconds(delay);
        }
        
        float playbackOffset = delay < 0f ? (-delay) : 0f;
        switch(type) {

            default: case FillType.Kick: 
            fillLength = kickFillLength;
            foreach(var drum in drums) drum.Play_Fill_Kick();
            break;

            case FillType.Hihat: 
            fillLength = hihatFillLength;
            foreach(var drum in drums) drum.Play_Fill_Hihat();
            break;
            
            case FillType.Snare: 
            fillLength = snareFillLength;
            foreach(var drum in drums) drum.Play_Fill_Snare();
            break;

            case FillType.Steam: 
            fillLength = steamFillLength;
            foreach(var drum in drums) drum.Play_Fill_Steam();
            break;
        }
        ++UniquePlays;

        if(fillLength > 0f) {
            yield return new WaitForSeconds(fillLength);
        }
        delayedFill = null;
    }
}