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

//Synchronizes all states on an Animator to align globally with a time-period
[ExecuteInEditMode]
public class AnimatorSynchronizer : MonoBehaviour {
    [SerializeField] public Animator animator = null;
    [SerializeField] public float period = 1f;
    [SerializeField] public int animationLayer = 0;
    [SerializeField, RangeAttribute(0.001f, 1f)] public float lerp = 10f;
    
    void Awake() {
        if(Application.isPlaying) {
            //in play-mode
        }else{
            //in editor
            if(animator == null) {
                animator = GetComponent<Animator>();
            }
        }
    }

    void OnValidate() {
        if(Application.isPlaying) {
            //in play-mode
        }else{
            if(period <= 0f) period = 0.001f;
            if(animator != null) {
                int layerCount = animator.layerCount;
                animationLayer = Mathf.Clamp(animationLayer, 0, layerCount);
            }
        }
    }

    void Update() {

        //in play-mode
        if(Application.isPlaying) {
            if(animator == null || !animator.isActiveAndEnabled) return;
            var stateInfo = animator.GetCurrentAnimatorStateInfo(animationLayer);

            //additional real-time to add to the playback this frame
            float frameAnimTime = Time.deltaTime * stateInfo.speed * stateInfo.speedMultiplier;
            //the real length of the current state
            float stateTime = stateInfo.length / (stateInfo.speed * stateInfo.speedMultiplier);
            //current normalized time
            float normalizedTime = stateInfo.normalizedTime;
            //the current un-normalized time
            float time = (normalizedTime * stateTime) + frameAnimTime;
            
            //the delta which the global timer is from the period (subtract period/2 so we can snap to the middle of the period, instead of clipping around when we're over the modulo value)
            float globalDelta = (Time.realtimeSinceStartup % period) - period/2f;
            //the delta which the current time is from the period
            float currentDelta = (time % period) - period/2f;

            //the delta between the two deltas
            float deltaDelta = globalDelta - currentDelta;
            //the un-normalized time which we'll set on the animator
            float correctTime = time + (deltaDelta * lerp);
            //the normalized time which we'll set on the animator
            float normalizedCorrectTime = correctTime / stateTime;

            //do the animator-setting stuff
            animator.Play(stateInfo.fullPathHash, animationLayer, normalizedCorrectTime);
        }else{
            //in editor
        }
    }
}