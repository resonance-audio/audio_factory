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
using System;
using System.Collections;
using System.Collections.Generic;

public class RandomAudioClip : MonoBehaviour {
    [SerializeField] public AudioClip[] clips;
    [SerializeField] public Vector2 pitchRange = new Vector2(1f, 1f);
    [SerializeField] public Vector2 gainRange = new Vector2(0f, 0f);
    [NonSerialized] List<AudioClip> randomizedList = new List<AudioClip>();
    

    public void PlayRandom() {
        //if the sound list is empty, re-randomize
        if(randomizedList == null || randomizedList.Count == 0) {
            RandomizeClips();
        }
        //if it's still empty by now, something is wrong; return
        if(randomizedList == null || randomizedList.Count == 0) return;

        GvrAudioSource gvrSource = GetComponent<GvrAudioSource>();

        //return if no audio source
        if(gvrSource == null) return;

        //get the next clip in the list
        AudioClip clip = randomizedList[randomizedList.Count-1];
        randomizedList.RemoveAt(randomizedList.Count-1);

        if(gvrSource != null) {
            gvrSource.clip = clip;
            gvrSource.gainDb = UnityEngine.Random.Range(gainRange.x, gainRange.y);
            gvrSource.pitch = UnityEngine.Random.Range(pitchRange.x, pitchRange.y);
            gvrSource.Play();
        }
    }

    void RandomizeClips() {
        randomizedList = randomizedList ?? new List<AudioClip>();
        randomizedList.Clear();
        for(int i=0; i<clips.Length; ++i) {
            randomizedList.Add(clips[i]);
        }

        Shuffle(randomizedList);
    }
    
    static void Shuffle<T>(IList<T> list) {  
        int n = list.Count;  
        while (n > 1) {  
            n--;  
            int k = UnityEngine.Random.Range(0, (n + 1));  
            T value = list[k];  
            list[k] = list[n];  
            list[n] = value;  
        }  
    }
}