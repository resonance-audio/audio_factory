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

//Terrible, terrible methods used to access Animator methods via single-parameter UnityEvents
//[bh 04-18-2017] - marking this obsolete; AnimatorParameterSetter has an inspector for setting
//   animator parameters and triggers
[Serializable, RequireComponent(typeof(Animator))]
[ObsoleteAttribute("Use AnimatorParameterSetter")]
public class AnimatorEvents : MonoBehaviour {

    //Set a boolean parameter
    public void SetBoolean(string args) {
        if(args == null || args.Length == 0) return;
        string[] split = args.Trim().Split(' ');
        if(split == null || split.Length < 2) return;

        string name = split[0];
        bool val = bool.Parse(split[1]);
        GetComponent<Animator>().SetBool(name, val);
    }
    
    //Set an integer parameter
    public void SetInt(string args) {
        if(args == null || args.Length == 0) return;
        string[] split = args.Trim().Split(' ');
        if(split == null || split.Length < 2) return;

        string name = split[0];
        int val = int.Parse(split[1]);
        GetComponent<Animator>().SetInteger(name, val);
    }
    
    //Set an integer parameter
    public void SetFloat(string args) {
        if(args == null || args.Length == 0) return;
        string[] split = args.Trim().Split(' ');
        if(split == null || split.Length < 2) return;

        string name = split[0];
        float val = float.Parse(split[1]);
        GetComponent<Animator>().SetFloat(name, val);
    }
}