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

//Sets an "open" bool parameter on an animator when the elevator object is within the specified distance
public class ElevatorDoor : MonoBehaviour {
    const string ANIM_PROP_OPEN = "open";

    [SerializeField] public float openProximity = 10f;
    [SerializeField] public Animator animator = null;

    [NonSerialized] Elevator elevator = null;
    [NonSerialized] GvrAudioSource audioSource;
    [NonSerialized] bool isOpen;


    void Start() {
        audioSource = GetComponent<GvrAudioSource>();
    }

    void Update() {
        elevator = elevator ?? FindObjectOfType<Elevator>();
        if(elevator != null && animator != null) {
            Vector3 diff = transform.position - elevator.transform.position;
            bool shouldBeOpen = (diff.magnitude <= openProximity);
            if (shouldBeOpen != isOpen) {
                isOpen = shouldBeOpen;

                animator.SetBool(ANIM_PROP_OPEN, isOpen);
                if (audioSource && isOpen) audioSource.Play();
            }
        }
    }
}