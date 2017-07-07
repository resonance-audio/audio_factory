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

public class ElevatorSpeaker : MonoBehaviour {

    public static ElevatorSpeaker instance {
        get; protected set;
    }

    public GameObject repeatButtonObj;
    RepeatButton repeatButton;

    void OnEnable() {
        instance = this;

        repeatButton = repeatButtonObj.GetComponentInChildren<RepeatButton>(true);
        repeatButton.Disabled = true;
        source = GetComponent<GvrAudioSource>();
    }

    GvrAudioSource source = null;

    public void SetAudioClip(AudioClip vo) {
        source.clip = vo;
    }

    public void Play(AudioClip vo) {
        if (repeatButton.Disabled)
            repeatButton.Disabled = false;
        
        if(vo == null) return;
        if(source == null) return;
        source.enabled = true;
        source.clip = vo;
        source.Play();
    }

    public void RepeatLast() {
        source.Stop();
        source.Play();
    }

}