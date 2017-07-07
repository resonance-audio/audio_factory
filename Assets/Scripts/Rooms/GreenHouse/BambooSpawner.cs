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

public class BambooSpawner : MonoBehaviour {

    public GameObject[] bambooPrefabs;

    public Material[] bambooStalkMaterials;
    public Material[] bambooLeafMaterials;

    BambooGroup[] bambooGroups;
    public GameObject[] bambooMarkersPrefabs;
    GameObject[] bambooGroupMarkers;

    public Transform[] showerHeads;

    public AnimationCurve swayCurve;
    public AnimationCurve leafFade;

    List<GameObject> destroyAfterSpawn = new List<GameObject>();

    public Vector2 plantHeightRange = new Vector2(1.00f, 1.65f);

    public Vector2 stalkFrequencyRange = new Vector2(0.05f, 0.10f);
    public Vector2 stalkAmplitudeRange = new Vector2(0.05f, 0.10f);

    public Vector2 stemFrequencyRange = new Vector2(1.0f, 3.00f);
    public Vector2 stemAmplitudeRange = new Vector2(4.0f, 6.0f);

    bool oddFrame;
    bool bambooReady = false;

    class BambooInfo {
        public Transform obj;
        public float animFrequency;
        public float animAmplitude;
        public Quaternion startRot;
        public BambooInfo(Transform _obj, float _animFrequency, float _animAmplitude, Quaternion _startRot) {
            obj = _obj;
            animFrequency = _animFrequency;
            animAmplitude = _animAmplitude;            
            startRot = _startRot;            
        }
    }

    BambooInfo[] allBamboo;
    List<List<BambooInfo>> bambooStands = new List<List<BambooInfo>>();
    List<List<BambooInfo>> stemsGroups = new List<List<BambooInfo>>();

    GreenhouseControl control;

    public void Init (GreenhouseControl _control) {
        control = _control;
        for (int i = 0; i < 3; i++) {
            List<BambooInfo> newStand = new List<BambooInfo>();
            bambooStands.Add(newStand);

            List<BambooInfo> newGroup = new List<BambooInfo>();
            stemsGroups.Add(newGroup);
        }

        bambooGroups = new BambooGroup[4];
        StartCoroutine(CreateBamboo());

	}
	
	void Update () {

        if (!bambooReady)
            return;
        
        oddFrame = !oddFrame;
        int startFrame = oddFrame ? 1 : 0;
        for (int i = startFrame; i < allBamboo.Length; i += 2) {
            BambooInfo bambooInfo = allBamboo[i];
            float amount = swayCurve.Evaluate(Time.time * bambooInfo.animFrequency) * bambooInfo.animAmplitude;
            Quaternion rotOffset = Quaternion.AngleAxis(amount, Vector3.right);
            bambooInfo.obj.localRotation = bambooInfo.startRot * rotOffset;
        }

        for (int i = 0; i < showerHeads.Length; i++) {
            ShowerHead showerHead = showerHeads[i].GetComponent<ShowerHead>();
            if (showerHead.ShowerOn) {
                foreach(BambooInfo bambooInfo in stemsGroups[i]) {
                    float amount = swayCurve.Evaluate(Time.time * bambooInfo.animFrequency) * bambooInfo.animAmplitude;
                    Quaternion rotOffset = Quaternion.AngleAxis(amount, bambooInfo.obj.right);
                    bambooInfo.obj.localRotation = bambooInfo.startRot * rotOffset;
                }
            }
        }
	}


    IEnumerator CreateBamboo() {

        // spawn markers
        yield return new WaitForEndOfFrame();
        GameObject bambooMarker1 = Instantiate(bambooMarkersPrefabs[0], transform.position, transform.rotation) as GameObject;

        yield return new WaitForEndOfFrame();
        GameObject bambooMarker2 = Instantiate(bambooMarkersPrefabs[1], transform.position, transform.rotation) as GameObject;

        yield return new WaitForEndOfFrame();
        GameObject bambooMarker3 = Instantiate(bambooMarkersPrefabs[2], transform.position, transform.rotation) as GameObject;

        bambooGroupMarkers = new GameObject[3];
        bambooGroupMarkers[0] = bambooMarker1.transform.Find("BAMBOO_01").gameObject;
        bambooGroupMarkers[1] = bambooMarker2.transform.Find("BAMBOO_02").gameObject;
        bambooGroupMarkers[2] = bambooMarker3.transform.Find("BAMBOO_03").gameObject;

        List<BambooInfo> bambooList = new List<BambooInfo>();
        for (int groupIndex = 0; groupIndex < bambooGroupMarkers.Length; groupIndex++) {
            Transform groupObj = transform.Find("Group" + (groupIndex + 1));
            BambooGroup bambooGroup = groupObj.GetComponent<BambooGroup>();
            bambooGroups[groupIndex] = bambooGroup;
            bambooGroups[groupIndex].AddShowerHead(showerHeads[groupIndex].GetComponent<ShowerHead>());
            bambooGroups[groupIndex].AddPlants(control.GetPlantGroup(groupIndex));
            bambooGroups[groupIndex].AddLeafFadeCurve(leafFade);
            GameObject groupMarker = bambooGroupMarkers[groupIndex];

            destroyAfterSpawn.Add(groupMarker.transform.parent.gameObject);

            foreach(Transform stalk in groupMarker.transform) {
                yield return new WaitForSeconds(0.05f);

                int selector = groupIndex;
                GameObject prefab = bambooPrefabs[selector];
                GameObject bambooStalk = Instantiate(prefab, stalk.position, stalk.rotation) as GameObject;

                float height = Random.Range(plantHeightRange.x, plantHeightRange.y);
                bambooStalk.transform.localScale = new Vector3(1, height, 1);
                bambooStalk.transform.SetParent(groupObj);
                BambooInfo info = new BambooInfo(
                    bambooStalk.transform, 
                    Random.Range(stalkFrequencyRange.x, stalkFrequencyRange.y), 
                    Random.Range(stalkAmplitudeRange.x, stalkAmplitudeRange.y), 
                    bambooStalk.transform.localRotation
                );
                bambooList.Add(info);
                bambooStands[groupIndex].Add(info);
                foreach(Transform stem in bambooStalk.transform) {

                    for (int i = 0; i < showerHeads.Length; i++) {
                        Vector2 stemPos = new Vector2(stem.position.x, stem.position.z);
                        Vector2 showerPos = new Vector2(showerHeads[i].position.x, showerHeads[i].position.z);
                        float distance = Vector2.Distance(stemPos, showerPos);
                        float maxDistance = 1.0f;
                        if ( distance < maxDistance && stem.position.y < (showerHeads[i].position.y - 0.25f)) {
                            float scale = 1 - (distance / maxDistance);
                            float invert = Random.value > 0.5f ? -1 : 1;
                            BambooInfo stemInfo = new BambooInfo(
                                stem, 
                                Random.Range(stemFrequencyRange.x, stemFrequencyRange.y) * scale * invert, 
                                Random.Range(stemAmplitudeRange.x, stemAmplitudeRange.y) * scale * invert, 
                                stem.localRotation
                            ); 
                            stemsGroups[i].Add(stemInfo);
                        }
                    }
                }
            }
        }

        allBamboo = bambooList.ToArray();

        foreach(GameObject killme in destroyAfterSpawn) {
            Destroy(killme);
        }

        bambooReady = true;
        control.Init();
    }

    void TestGroups() {
        foreach (List<BambooInfo> stemGroup in stemsGroups) {
            foreach(BambooInfo stem in stemGroup) {
                stem.obj.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    public void LightUpStand(int index) {
        Debug.Log("Lighting up " + index);
        bambooGroups[index].LightUp();
    }

    public BambooGroup GetGroup(int index) {
        return bambooGroups[index];
    }


}
