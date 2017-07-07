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
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class SeedControl : MonoBehaviour {


    List<Transform> orbitPaths = new List<Transform>();
    List<SeedOrbitor> orbitors = new List<SeedOrbitor>();
    Transform orbitorSpeedObj;
    GvrAudioSource orbitorSpeedSound;
    Vector3 orbitorLastPos;
    float currentSpeed;
    float speedFadeOut = -1;


    Transform pipe;

    Transform handle;
    SeedHandle seedHandle;

    float seedHeight = 1.0f;

    SeedFunnel funnel;
    SeedGenerator generator;

    Vector3 smallScale = new Vector3(0.25f, 0.25f, 0.25f);

    public bool movingToFunnel = false;
    public bool movingDownPipe = false;

    float moveProgress = 0.0f;
    float moveDuration = 0.5f;
    Vector3 moveStartPos;
    Vector3 moveEndPos;

    float buildTimer = 0.0f;
    float buildDuration = 3.0f;

    bool hasOrbitors;

    public bool HasOrbitors
    {
        get { return hasOrbitors; }
        set { hasOrbitors = value; }
    }

    bool makingNoise;

    float noiseFadeTimer = 0.0f;
    float noiseDuration = 3.0f;
    GvrAudioSource seedSound;

    GvrAudioSource extraSounds;
    AudioClip hoverSound;
    AudioClip enterSound;
    AudioClip exitSound;

    public bool MakingNoise
    {
        get { return makingNoise; }
        set
        { 
            makingNoise = value; 
            noiseFadeTimer = Mathf.Clamp01(noiseFadeTimer);
        }
    }

    Transform glow;
    Vector3 smallGlow = new Vector3(1.5f, 1.5f, 1.0f);
    Vector3 largeGlow = new Vector3(2.0f, 2.0f, 1.0f);

    bool isHovered = false;
    float hoverTimer = 0.0f;
    float hoverDuration = 0.5f;
    Color hoverOnColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
    Color hoverOffColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);

    float seedTransparency;


    public bool IsHovered
    {
        get { return isHovered; }
        set { 
            isHovered = value;
            hoverTimer = Mathf.Clamp01(hoverTimer);
            if (isHovered) 
                extraSounds.PlayOneShot(hoverSound);
        }
    }

    Renderer seedRenderer;
    Material seedMat;



    public void Init(
        SeedFunnel _funnel, 
        SeedGenerator _generator, 
        AudioClip soundLoop, 
        AudioClip speedSound ) {

        funnel = _funnel;
        generator = _generator;

        AddOrbitPaths();
        CreateHandle();
        CreateAudioSources(soundLoop);
        MakingNoise = true;


        seedRenderer = transform.Find("SEED").GetComponent<Renderer>();
        seedMat = seedRenderer.material;

        glow = transform.Find("Glow");
        glow.GetComponent<Renderer>().enabled = false;

        orbitorSpeedObj = new GameObject("OrbitorSpeedSound").transform;
        orbitorSpeedObj.parent = transform;
        orbitorSpeedObj.localPosition = Vector3.zero;
        orbitorSpeedSound = orbitorSpeedObj.gameObject.AddComponent<GvrAudioSource>();
        orbitorSpeedSound.clip = speedSound;
        orbitorSpeedSound.loop = true;
        orbitorSpeedSound.volume = 0.0f;
        orbitorSpeedSound.Play();
        extraSounds.PlayOneShot(enterSound);
    }

    public void AddExtraSounds(AudioClip seedHover, AudioClip seedEnter, AudioClip seedExit) {
        hoverSound = seedHover;
        enterSound = seedEnter;
        exitSound = seedExit;       
    }


    void Update() {

        Color currentColor = hoverOffColor;
        Vector3 currentPos = orbitPaths[0].position;
        currentSpeed = Mathf.Lerp(currentSpeed, Vector3.Distance(currentPos, orbitorLastPos) * Time.deltaTime, Time.deltaTime * 2);

        if (speedFadeOut >= 0.0f && speedFadeOut <= 1.0f) 
            speedFadeOut += Time.deltaTime / 2.0f;
        
        orbitorSpeedSound.volume = Mathf.InverseLerp(0.0002f, 0.0005f, currentSpeed * Mathf.Clamp01(speedFadeOut));

        orbitorLastPos = currentPos;

        if (hoverTimer >= 0.0f && hoverTimer <= 1.0f)
        {
            hoverTimer += Time.deltaTime / (isHovered ? hoverDuration * 0.25f : -hoverDuration);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(hoverTimer));
            currentColor = Color.Lerp(hoverOffColor, hoverOnColor, amount);
        }


        transform.rotation = Quaternion.LookRotation(transform.position - handle.position, Camera.main.transform.position - transform.position);
        transform.rotation *= Quaternion.AngleAxis(-90, Vector3.right);

        if (buildTimer < 1) {
            buildTimer += Time.deltaTime / buildDuration;
            currentColor.a = Mathf.Lerp(0, hoverOffColor.a, buildTimer);
            seedMat.SetColor("_Color", currentColor);

            if (buildTimer > 1) {
                glow.GetComponent<Renderer>().enabled = true;
                generator.SeedBuilt();
            }
        }

        glow.localScale = Vector3.Lerp(smallGlow, largeGlow, Random.value);

        if (movingToFunnel && !movingDownPipe) {
            moveProgress += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(moveStartPos, moveEndPos, Ease.OutQuart(moveProgress));
            transform.localScale = Vector3.Lerp(Vector3.one, smallScale, Ease.OutQuart(moveProgress));

            if (moveProgress > 1) {
                generator.SeedInFunnel(this);
                glow.GetComponent<Renderer>().enabled = false;
                seedRenderer.enabled = false;
            }
            return;
        }

        if (noiseFadeTimer >= 0.0f && noiseFadeTimer <= 1.0f) {
            noiseFadeTimer += Time.deltaTime / (makingNoise ? noiseDuration : -noiseDuration * 2f);
            float amount = Mathf.SmoothStep(0, 1, Mathf.Clamp01(noiseFadeTimer));
            seedSound.volume = Mathf.Lerp(0, 0.5f, amount);

            if (movingDownPipe && noiseFadeTimer < 0.0f) {
                Destroy(handle.gameObject);
                Destroy(gameObject);
            }
        }
        if (seedSound.volume < 0.1f && seedSound.isPlaying) {
            seedSound.Stop();
        } 
        if (seedSound.volume > 0.1f && !seedSound.isPlaying) {
            seedSound.Play();
        } 

        if (movingDownPipe) {
            transform.position = pipe.position;
        } else {
            Vector3 moveAmount = ((handle.position - (Vector3.up * 0.1f)) - transform.position) * Time.deltaTime * 10;
            transform.Translate(moveAmount, Space.World);

            seedHeight = Mathf.Lerp(seedHeight, Mathf.Clamp(Vector3.Distance(transform.position, handle.position) * 10, 0.9f, 1.1f), Time.deltaTime * 5);
            transform.localScale = new Vector3(1 / seedHeight, seedHeight, 1 / seedHeight);           
        }
    }

    public void ReleaseSeed() {
        generator.ReleaseSeed();
    }

    void CreateHandle() {
        handle = new GameObject("SeedHandle").transform;
        SphereCollider col = handle.gameObject.AddComponent<SphereCollider>();
        col.radius = 0.1f;
        seedHandle = handle.gameObject.AddComponent<SeedHandle>();
        seedHandle.Init(this);
        seedHandle.AddFunnel(funnel);
        handle.position = transform.position + (Vector3.up * 0.1f);
    }

    void CreateAudioSources(AudioClip sound) {
        seedSound = gameObject.AddComponent<GvrAudioSource>();
        seedSound.clip = sound;
        seedSound.loop = true;
        seedSound.volume = 0;
        seedSound.directivityAlpha = 0.12f;
        seedSound.directivitySharpness = 4.5f;
        seedSound.playOnAwake = false;

        extraSounds = gameObject.AddComponent<GvrAudioSource>();
        extraSounds.playOnAwake = false;
    }

    public Vector3 GetOrbitPos(int orbitID) {
        return orbitPaths[orbitID % orbitPaths.Count].position;
    }

    public Quaternion GetOrbitRot(int orbitID) {
        return orbitPaths[orbitID % orbitPaths.Count].rotation;
    }

    public Vector3 GetOrbitScale(int orbitID) {
        return orbitPaths[orbitID % orbitPaths.Count].localScale;
    }

    void AddOrbitPaths() {
        bool noPath = false;
        int pathNumber = 0;
        Transform newPathObj;

        while (!noPath) {
            pathNumber++;
            newPathObj = transform.Find(string.Format("MOONS/Cube__{0}", pathNumber.ToString()));
            if (!newPathObj) {
                noPath = true;
                break;
            }
            newPathObj.GetComponent<Renderer>().enabled = false;
            orbitPaths.Add(newPathObj);
        }
    }

    public void AddOrbitor(Transform orbitor, AudioClip soundClip) {
        SeedOrbitor newOrbitor = orbitor.gameObject.AddComponent<SeedOrbitor>();
        newOrbitor.Init(this, soundClip);
        orbitors.Add(newOrbitor);
    }

    public void SeedPickedUp() {
        speedFadeOut = 0.0f;
        generator.SeedUsed();
    }

    public void StartMoveToFunnel() {
        extraSounds.PlayOneShot(exitSound, 1.0f);

        movingToFunnel = true;

        moveStartPos = transform.position;
        moveEndPos = funnel.transform.position;

        foreach (SeedOrbitor orbitor in orbitors)
            orbitor.Clear();
    }

    public void AddPipeControl(Pipe _pipe) {
        pipe = _pipe.transform;
        movingDownPipe = true;
        MakingNoise = false;
    }

}
