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
using AudioEngineer.Rooms.Finale;
using UnityEngine;
using UnityEngine.EventSystems;

public class ElevatorLever : MonoBehaviour {

    const string ANIM_PARAM_ON = "on";
    const string ANIM_PARAM_HOVER = "hover";
    const string ANIM_PARAM_DOWN = "down";

    public Action OnPressed { get; set; }

    //our elevator lever is actually an invisible sliding door; with a lever points to it
    [SerializeField] FinaleDoor lever;
    [SerializeField] Vector3 axis;
    [SerializeField] float start;
    [SerializeField] float end;
    [SerializeField] float lerp;
    [SerializeField] bool onByDefault;
    [SerializeField] float activateThreshold = 0.99f;
    [SerializeField] Animator animator = null;
    [SerializeField] float timeStuckAtEnd = 1f;

    [NonSerialized] float? _percent = null;
    [NonSerialized] float _timePressed = 0f;

    public bool On {
        get {
            return lever.isActiveAndEnabled;
        }
        set {
            lever.gameObject.SetActive(value);
        }
    }

    public bool Hover {
        get {
            return lever.IsHover;
        }
    }
    public bool Down {
        get {
            return lever.IsDown;
        }
    }

    void OnEnable() {
        On = onByDefault;
    }

    void Update() {

        if(animator != null) {
            animator.SetBool(ANIM_PARAM_ON, On);
            animator.SetBool(ANIM_PARAM_DOWN, Down);
            animator.SetBool(ANIM_PARAM_HOVER, Hover);
        }

        if(!_percent.HasValue) {
            _percent = lever.OpenPercent;
        }else{
            _percent = Mathf.Lerp(_percent.Value, lever.OpenPercent, Mathf.Clamp(Time.deltaTime * lerp, 0f, 1f));
        }
        transform.localEulerAngles = axis.normalized * CosLerp(start, end, _percent.Value);

        if(On && _percent >= activateThreshold) {
            if(OnPressed != null) {
                OnPressed.Invoke();
            }
            _timePressed = Time.time;
            lever.OpenPercent = 1f;
            On = false;
        }

        //if we've released, set the position to zero
        if((!On || !Down) && (Time.time - _timePressed >= timeStuckAtEnd)) {
            lever.OpenPercent = 0f;
        }
    }

    float CosLerp(float start, float end, float t) {
        return start + (end-start) * (1f + Mathf.Cos((1f + Mathf.Clamp(t, 0f, 1f))*Mathf.PI)) / 2f;
    }
}