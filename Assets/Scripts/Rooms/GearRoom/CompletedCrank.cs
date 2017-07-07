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

namespace AudioEngineer.Rooms.Gears {
    public class CompletedCrank : MonoBehaviour {

        [SerializeField] Animator targetAnimator = null;
        [SerializeField] float targetSpeed = 1f;
        [SerializeField] float targetSpeedLerp = 10f;
        [SerializeField] string animStateName = "main";

        [SerializeField] public float startSpeed = 1f;
        [SerializeField] float currentSpeed = 1f;
        [SerializeField] float playbackTime = 0f;

        void OnEnable() {
            currentSpeed = startSpeed;
            playbackTime = targetAnimator.playbackTime;
        }

        void Update() {
            if(targetAnimator != null) {
                float percent = Mathf.Clamp(Time.deltaTime * targetSpeedLerp, 0f, 1f);
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, percent);
                var info = targetAnimator.GetCurrentAnimatorStateInfo(0);
                playbackTime += currentSpeed * Time.deltaTime;
                targetAnimator.Play(animStateName, 0, info.normalizedTime + currentSpeed*Time.deltaTime);
            }
        }
    }
}