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
using UnityEngine.Events;

public class SeedGenerator : MonoBehaviour {

    public GameObject[] seedPrefabs;
    public GameObject orbitorPrefab;
    public RuntimeAnimatorController[] seedAnimators;

    Animator animator;

    Transform basin;

    SeedControl currentSeed;

    SeedGeneratorHandle[] handles;
    SeedFunnel funnel;

    GreenhouseControl greenhouseControl;

    public AudioClip[] seedSounds;
    public AudioClip[] handleSounds;
    public AudioClip[] handleHoverSounds;
    public AudioClip seedHover;
    public AudioClip seedEnter;
    public AudioClip seedExit;
    public AudioClip seedSpeedSound;

    FeederPipe[] feederPipes;

    public AnimationCurve handleFlash;

    bool handleTurned;
    bool seedDraged;
    bool dragWarningPlayed;
    bool twoLeft;
    bool oneLeft;

    public bool ReadyForHandle {
        get { return readyForHandle; } 
    }

    enum GeneratorState {Ready, BuildingSeed, WaitingForOrbitors, AddingOrbitors, SeedComplete}
    GeneratorState currentState = GeneratorState.Ready;
    bool readyForHandle = true;

    ElevatorVOSequencer VOSequencer;

	void Start () {
        basin = transform.Find("GENERATOR/BASIN/BASIN_2");
        SetUpFunnel();
        SetUpHandles();
        SetUpFeederPipes();

        greenhouseControl = GameObject.Find("GreenhouseControl").GetComponent<GreenhouseControl>();
        animator = GetComponent<Animator>();
        VOSequencer = GreenHouseRoomManager.instance.GetVOSequencer();

	}

	void Update () {
    
	}

    public void GenerateSeed(int seedId)
    {
        SetGeneratorState(GeneratorState.BuildingSeed);

        GameObject newSeed = Instantiate(seedPrefabs[seedId], basin.position + (Vector3.up * 0.1f), Quaternion.identity) as GameObject;
        newSeed.GetComponent<Animator>().runtimeAnimatorController = seedAnimators[seedId];
        currentSeed = newSeed.AddComponent<SeedControl>();
        currentSeed.Init(funnel, this, seedSounds[seedId], seedSpeedSound);
        currentSeed.AddExtraSounds(seedHover, seedEnter, seedExit);
    }

    void SetUpHandles()
    {
        handles = new SeedGeneratorHandle[3];
        handles[0] = transform.Find("GENERATOR/SOURCE_1/HANDLE_1").gameObject.AddComponent<SeedGeneratorHandle>();
        handles[0].Init(this, 0, handleSounds[0], handleHoverSounds[0], handleFlash);
        handles[0].AddHoverRing(transform.Find("GENERATOR/SOURCE_1/RING_1").GetComponent<Renderer>().material);

        handles[1] = transform.Find("GENERATOR/SOURCE_2/HANDLE_2").gameObject.AddComponent<SeedGeneratorHandle>();
        handles[1].Init(this, 1, handleSounds[1], handleHoverSounds[1], handleFlash);
        handles[1].AddHoverRing(transform.Find("GENERATOR/SOURCE_2/RING_2").GetComponent<Renderer>().material);

        handles[2] = transform.Find("GENERATOR/SOURCE_3/HANDLE_3").gameObject.AddComponent<SeedGeneratorHandle>();
        handles[2].Init(this, 2, handleSounds[2], handleHoverSounds[2], handleFlash);
        handles[2].AddHoverRing(transform.Find("GENERATOR/SOURCE_3/RING_3").GetComponent<Renderer>().material);

    }

    void SetUpFeederPipes() {
        feederPipes = new FeederPipe[3];
        Transform newFeederPipeAObj;
        Transform newFeederPipeBObj;

        GameObject newFeederPipe;
        for(int i = 0; i < feederPipes.Length; i++) {
            newFeederPipeAObj = transform.Find(string.Format("GENERATOR/SOURCE_{0}/SPIGOT_PIPES_A_{0}", (i + 1)));
            newFeederPipeBObj = transform.Find(string.Format("GENERATOR/SOURCE_{0}/SPIGOT_PIPES_B_{0}", (i + 1)));

            newFeederPipe = GameObject.Find(string.Format("FeederPipe{0}", (i + 1).ToString()));
            feederPipes[i] = newFeederPipe.GetComponent<FeederPipe>();
            feederPipes[i].AddPipeObj(newFeederPipeAObj, newFeederPipeBObj);
            feederPipes[i].AddHandle(handles[i]);
        }
    }

    void ActivateHandles() {
        for (int i = 0; i < handles.Length; i++) {
            if (feederPipes[i].pipeOn) 
                handles[i].IsReady = true;
        }
    }

    void DisableHandles() {
        foreach (SeedGeneratorHandle generatorHandle in handles)
            generatorHandle.IsReady = false;
    }

    void ActivateFeederPipes() {
        foreach (FeederPipe pipe in feederPipes)
            pipe.TurnOnWater();
    }

    void DisableFeederPipes() {
        foreach (FeederPipe pipe in feederPipes)
            pipe.TurnOffWater();
    }
        
    void SetUpFunnel()
    {
        funnel = transform.Find("FUNNEL/FUNNEL_2").gameObject.AddComponent<SeedFunnel>();
        funnel.Init(this);
    }

    public IEnumerator AddOrbitors(int soundId) {
        SetGeneratorState(GeneratorState.AddingOrbitors);

        for (int orbitorID = 1; orbitorID < 7; orbitorID++)
        {
            GameObject newOrbitor = Instantiate(orbitorPrefab, basin.position, Quaternion.identity) as GameObject;
            newOrbitor.name = "orbitor_" + orbitorID;
            currentSeed.AddOrbitor(newOrbitor.transform, seedSounds[soundId]);
            yield return new WaitForSeconds(0.5f); 
        }
        currentSeed.HasOrbitors = true;
        SetGeneratorState(GeneratorState.SeedComplete);
    }

    public void TurnHandle(int id)
    {
        if (!handleTurned) {
            handleTurned = true;
            VOSequencer.ReplaceEvent("06_GREENHOUSE_draganddropfunnel_1");
        }
        feederPipes[id].UsePipe();
        currentState = GeneratorState.Ready;
        GenerateSeed(id);
        StartCoroutine(AddOrbitors(id));
    }

    public void ReleaseSeed() {
        if (!twoLeft) {
            float dirToBasin = Vector3.Dot(Camera.main.transform.forward, (basin.position - Camera.main.transform.position).normalized);
            if (!dragWarningPlayed && dirToBasin > 0.0f) {
                UnityEvent doneEvent = new UnityEvent ();
                doneEvent.AddListener (DragWarningPlayed);
                VOSequencer.ReplaceEvent("06_GREENHOUSE_draganddropfunnel_1", doneEvent);
            }
        }
    }

    public void DragWarningPlayed() {
        dragWarningPlayed = true;
    }



    public void SeedInFunnel(SeedControl seed)
    {
        if (twoLeft) {
            VOSequencer.StopAllEvents();
        }
        if (twoLeft && !oneLeft) {
            oneLeft = true;
            VOSequencer.ReplaceEvent("green_oneomore_left");
        }
        if (!twoLeft) {
            twoLeft = true;
            VOSequencer.ReplaceEvent("green_twomore_left");
        }

        seed.AddPipeControl(greenhouseControl.GetCurrentPipe());
        greenhouseControl.NextPipeOn();
        ActivateHandles();
    }

    public void SeedBuilt() {
        SetGeneratorState(GeneratorState.WaitingForOrbitors);
    }

    public void SeedUsed() {
        if (!seedDraged) {
            seedDraged = true;
        }
        VOSequencer.StopEvent("06_GREENHOUSE_draganddropfunnel_1");
        SetGeneratorState(GeneratorState.Ready);
    }

    void SetGeneratorState(GeneratorState newState) {
        switch (newState) {
            case GeneratorState.Ready :
                if (seedDraged)
                    animator.SetInteger("AnimState", 0);
                readyForHandle = true;
                break;
            case GeneratorState.BuildingSeed :
                animator.SetInteger("AnimState", 1);
                DisableHandles();
                readyForHandle = false;
                break;
            case GeneratorState.WaitingForOrbitors :
                readyForHandle = true;
                break;
            case GeneratorState.AddingOrbitors :
                animator.SetInteger("AnimState", 2);
                readyForHandle = true;
                break;
            case GeneratorState.SeedComplete :
                if (currentState == GeneratorState.Ready)
                    return;
                readyForHandle = true;
                break;
            default :
                readyForHandle = true;
                break;
        }
        currentState = newState;
    }
}
