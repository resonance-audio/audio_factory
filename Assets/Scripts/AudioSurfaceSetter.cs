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

public class AudioSurfaceSetter : MonoBehaviour {

    GvrAudioRoom room;

    public GvrAudioRoom.SurfaceMaterial[] surfaces;

    int currentSurface = 0;

	void Start () {
        room = GetComponent<GvrAudioRoom>();
        if (!room)
            Debug.Log(gameObject.name + " doesn't have a AudioRoom on it");
	}
	
	void Update () {

        if (Input.GetKeyDown(KeyCode.Space)) {
            currentSurface += 1;
            if (currentSurface >= surfaces.Length)
                currentSurface = 0;
            SetSurface();
        }
	}

    void SetSurface() {
        GvrAudioRoom.SurfaceMaterial surface = surfaces[currentSurface];

        room.leftWall = surface;
        room.rightWall = surface;
        room.frontWall = surface;
        room.backWall = surface;
        room.floor = surface;
        room.ceiling = surface;
    }
}
