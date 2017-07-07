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

public class FinaleRoomSpawner : MonoBehaviour {

    AudioEngineer.Rooms.Finale.FinaleRoomManager roomManager;
    Transform worldGroup;

    public GameObject finalePrefab1;
    public GameObject finalePrefab2;
    public GameObject finalePrefab3;

    void Start () {
        roomManager = GetComponent<AudioEngineer.Rooms.Finale.FinaleRoomManager>();
        worldGroup = transform.Find("World");
        StartCoroutine(ActivateFinaleParts());
    }

    IEnumerator ActivateFinaleParts () {

        yield return new WaitForSeconds(1.00f);
        Instantiate(finalePrefab1, worldGroup.position, worldGroup.rotation, worldGroup);

        yield return new WaitForSeconds(1.00f);
        Instantiate(finalePrefab2, worldGroup.position, worldGroup.rotation, worldGroup);

        yield return new WaitForSeconds(1.00f);
        Instantiate(finalePrefab3, worldGroup.position, worldGroup.rotation, worldGroup);

    }
}
