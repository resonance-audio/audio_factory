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

public class ConsoleControl : MonoBehaviour {

    Switch[] switches;

    Dial dial;

    Animator animator;
	
	void Start () {
        animator = GetComponent<Animator>();
        FindSwitches();
        FindDial();
	}

    void FindSwitches() {
        Transform switchObj;
        switches = new Switch[4];
        for (int i = 0; i < switches.Length; i++) {
            switchObj = transform.Find("ConsolePanel/SwitchBase" + (i + 1));
            switches[i] = switchObj.gameObject.AddComponent<Switch>();
            switches[i].Init(this);
        }
    }

    void FindDial() {
        Transform dialObj = transform.Find("ConsolePanel/Dial");
        dial = dialObj.gameObject.GetOrAddComponent<Dial>();
        dial.Init(this);
    }
	
	
	void Update () {
	
	}

    public void SwitchSet() {
        bool allOn = true;
        foreach(Switch switchObj in switches) {
            if (!switchObj.SwitchOn) allOn = false;
        }

        animator.SetBool("ShowDial", allOn);
    }
}
