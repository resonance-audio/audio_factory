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
using System.Collections.Generic;

public class ElevatorControl : MonoBehaviour {

    public GameObject elevatorPrefab;
    public RuntimeAnimatorController elevatorAnims;
    public Material pedestalMat;

    public AudioClip gates_L;
    public AudioClip gates_R;
    public AudioClip sides_L;
    public AudioClip sides_R;
    public AudioClip risingLoop;
    public AudioClip gearSound;
    public AudioClip stopSound;

    public float gearSound_multiplier = 1f;
    public float elevatorFadeTime = 4.0f;

    public AnimationCurve volumeCurve_loopSound;

    GvrAudioSource elevatorLoop;
    GvrAudioSource elevatorGears;
    GvrAudioSource elevatorStop;

    GameObject elevator;

    float volumeTimer;

    bool moving;

    List<GvrAudioSource> sources = new List<GvrAudioSource>() ;

	void Start () {
        Transform displayGroup = transform.Find("Parent/Display");
        elevator = Instantiate(elevatorPrefab, transform.position, Quaternion.identity, displayGroup) as GameObject;	
        Animator anims = elevator.GetComponent<Animator>();
        anims.runtimeAnimatorController = elevatorAnims;
        GetComponent<Elevator>().SetElevatorAnims(anims);
        elevator.AddComponent<ElevatorRelay>().Init(this);

        Renderer[] renderers = elevator.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers) {
            renderer.material = pedestalMat;
        }

        AddAudioSources();

	}

    void AddAudioSources() {
        Transform door1 = elevator.transform.Find("ELEVATOR/DOOR");
        GvrAudioSource door1Source = door1.gameObject.AddComponent<GvrAudioSource>();
        door1Source.clip = gates_L;
        door1Source.playOnAwake = false;
        sources.Add(door1Source);

        Transform door2 = elevator.transform.Find("ELEVATOR/DOOR_2");
        GvrAudioSource door2Source = door2.gameObject.AddComponent<GvrAudioSource>();
        door2Source.clip = gates_R;
        door2Source.playOnAwake = false;
        sources.Add(door2Source);

        Transform l1 = elevator.transform.Find("ELEVATOR/L1");
        GvrAudioSource l1Source = l1.gameObject.AddComponent<GvrAudioSource>();
        l1Source.clip = sides_L;
        l1Source.playOnAwake = false;
        sources.Add(l1Source);

        Transform r1 = elevator.transform.Find("ELEVATOR/R1");
        GvrAudioSource r1Source = r1.gameObject.AddComponent<GvrAudioSource>();
        r1Source.clip = sides_R;
        r1Source.playOnAwake = false;
        sources.Add(r1Source);

        GameObject stopObj = new GameObject("StopSound");
        stopObj.transform.SetParent(elevator.transform.Find("ELEVATOR"));
        stopObj.transform.localPosition = new Vector3(0.0f, -0.25f, 0.0f);
        elevatorStop = stopObj.AddComponent<GvrAudioSource>();
        elevatorStop.clip = stopSound;
        elevatorStop.playOnAwake = false;

        GameObject gearObj = new GameObject("GearSound");
        gearObj.transform.SetParent(elevator.transform.Find("ELEVATOR"));
        gearObj.transform.localPosition = new Vector3(1.25f, 0.66f, 0.0f);
        elevatorGears = gearObj.AddComponent<GvrAudioSource>();
        elevatorGears.playOnAwake = false;
        elevatorGears.loop = true;
        elevatorGears.clip = gearSound;

        GameObject loopObj = new GameObject("LoopSound");
        loopObj.transform.SetParent(elevator.transform.Find("ELEVATOR"));
        loopObj.transform.localPosition = new Vector3(0.0f, -0.4f, 0.0f);
        elevatorLoop = loopObj.AddComponent<GvrAudioSource>();
        elevatorLoop.playOnAwake = false;
        elevatorLoop.loop = true;
        elevatorLoop.clip = risingLoop;

    }

    void Update() {
        if (volumeTimer >= 0.0f && volumeTimer <= 1.0f) {
            volumeTimer += Time.deltaTime / (moving ? elevatorFadeTime : -elevatorFadeTime * 0.25f);
            float amount = volumeCurve_loopSound.Evaluate(volumeTimer);
            elevatorLoop.volume = amount;
            elevatorGears.volume = amount * gearSound_multiplier;
        }
    }

    void OnValidate() {
        if(volumeCurve_loopSound == null) volumeCurve_loopSound = new AnimationCurve();
        if(volumeCurve_loopSound.keys == null || volumeCurve_loopSound.keys.Length < 2) {
            Keyframe[] keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(1f, 1f);
            volumeCurve_loopSound.keys = keys;
        }
        volumeCurve_loopSound.preWrapMode = WrapMode.Clamp;
        volumeCurve_loopSound.postWrapMode = WrapMode.Clamp;
    }


    public void PlaySounds(string type) {

        if (type.Equals("Open")) {
            foreach(GvrAudioSource source in sources) {
                source.Play();
            }
            moving = false;
            volumeTimer = Mathf.Clamp01(volumeTimer);
            elevatorStop.Play();
        }

        if (type.Equals("Close")) {
            foreach(GvrAudioSource source in sources) {
                source.Play();
            }
            elevatorLoop.volume = 0.0f;
            elevatorLoop.Play();

            elevatorGears.volume = 0.0f;
            elevatorGears.Play();
            moving = true;
            volumeTimer = Mathf.Clamp01(volumeTimer);

        }
    }
}
