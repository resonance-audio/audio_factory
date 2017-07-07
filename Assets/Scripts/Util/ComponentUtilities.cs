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

public static class ComponentUtilities {

    //Returns an existing T component, or instantiates a new one
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component {
        if(obj == null) throw new ArgumentNullException("obj");
        var ret = obj.GetComponent<T>();
        if(ret == null) ret = obj.AddComponent<T>();
        return ret;
    }

    //Returns an existing T component, or instantiates a new one
    public static T GetOrAddComponent<T>(this Component comp) where T : Component {
        if(comp == null) throw new ArgumentNullException("comp");
        var ret = comp.gameObject.GetComponent<T>();
        if(ret == null) ret = comp.gameObject.AddComponent<T>();
        return ret;
    }
}