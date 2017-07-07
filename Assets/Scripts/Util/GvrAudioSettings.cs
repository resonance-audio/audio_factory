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

﻿using UnityEngine;
using System.Collections;


[System.Serializable]
public class GvrAudioSettings {

    public AudioClip clip;
    public bool playOnAwake = false;
    public bool loop = false;
    public float volume = 1.0f;
    public float gain = 0.0f;
    public AudioRolloffMode volumeRolloff = AudioRolloffMode.Linear;
    public float minDistance = 0.0f;
    public float maxDistance = 500.0f;

    public GvrAudioSettings(AudioClip _clip) {
        clip = _clip;
    }


    public static GvrAudioSource ConfigSource(GvrAudioSource source, GvrAudioSettings settings) {
        source.clip = settings.clip;
        source.playOnAwake = settings.playOnAwake;
        source.loop = settings.loop;
        source.volume = settings.volume;
        source.gainDb = settings.gain;
        source.rolloffMode = settings.volumeRolloff;
        source.minDistance = settings.minDistance;
        source.maxDistance = settings.maxDistance;
        return source;
    }
}
