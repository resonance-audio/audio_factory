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
using AudioEngineer.Rooms.Finale;
using UnityEngine;

public class TestFinaleSound : MonoBehaviour {
    [SerializeField] int waitFrames = 2;
    [NonSerialized] GvrAudioSource source;
    [NonSerialized] Animator anim;


    void Awake() {
        anim = GetComponent<Animator>();
        source = GetComponent<GvrAudioSource>();
    }
    
    void OnEnable() {
        StartCoroutine(Routine());
    }

    void OnDisable() {
        StopAllCoroutines();
    }

    IEnumerator Routine() {
        var frame = new WaitForEndOfFrame();
        while(FinaleRoomManager.instance == null) {
            yield return frame;
        }

        if(source != null) {
            source.timeSamples = (int)(FinaleRoomManager.instance.SynchronizedPlaybackTime / source.clip.length * source.clip.samples);
        }

        AnimatorStateInfo animState = default(AnimatorStateInfo);
        if(anim != null) {
            animState = anim.GetCurrentAnimatorStateInfo(0);
            anim.Play(animState.fullPathHash, 0, FinaleRoomManager.instance.SynchronizedPlaybackTime / animState.length);
        }

        for(;;) {
            waitFrames = Mathf.Max(waitFrames, 1);

            //if we've elapsed past the audio length, break
            if(FinaleRoomManager.instance.SynchronizedPlaybackTime > source.clip.length) {
                anim.Play(animState.fullPathHash, 0, 1f);
                source.time = source.clip.length;
                break;
            }
            
            if(source != null) {
                source.timeSamples = (int)(FinaleRoomManager.instance.SynchronizedPlaybackTime / source.clip.length * source.clip.samples);
            }

            if(anim != null) {
                anim.Play(animState.fullPathHash, 0, FinaleRoomManager.instance.SynchronizedPlaybackTime / animState.length);
            }
            for(int f=0; f<waitFrames; ++f) yield return frame;
        }
    }
}