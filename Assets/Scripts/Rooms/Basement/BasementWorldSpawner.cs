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

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasementWorldSpawner : MonoBehaviour {

    AudioEngineer.Rooms.Basement.BasementRoomManager roomManager;
    Transform worldGroup;
    Transform pillarsGroup;

    public GameObject generators;

    public GameObject pillars1;
    public GameObject pillars2;
    public GameObject pillars3;
    public GameObject pillars4;
	
	void Start () {
        worldGroup = transform.Find("World");
        pillarsGroup = worldGroup.Find("Pillars");
        StartCoroutine(ActivateGenerators());
	}
	
    IEnumerator ActivateGenerators () {

        yield return new WaitForSeconds(1.00f);
        Instantiate(generators, worldGroup.position, worldGroup.rotation, worldGroup);

        yield return new WaitForSeconds(1.00f);
        Instantiate(pillars1, pillarsGroup.position, pillarsGroup.rotation, pillarsGroup);

        yield return new WaitForSeconds(1.00f);
        Instantiate(pillars2, pillarsGroup.position, pillarsGroup.rotation, pillarsGroup);

        yield return new WaitForSeconds(1.00f);
        Instantiate(pillars3, pillarsGroup.position, pillarsGroup.rotation, pillarsGroup);

        yield return new WaitForSeconds(1.00f);
        Instantiate(pillars4, pillarsGroup.position, pillarsGroup.rotation, pillarsGroup);

	}
}
