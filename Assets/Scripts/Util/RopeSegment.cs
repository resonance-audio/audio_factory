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

[Serializable]
public class RopeSegment : MonoBehaviour {

    public void Initialize(float radius, float segmentLength) {
        var capsule = GetComponent<CapsuleCollider>() ?? gameObject.AddComponent<CapsuleCollider>();
        capsule.radius = radius;
        capsule.height = segmentLength;
    }

    public void SafeDestroy() {

        //due to some finicky issues destroying joints/colliders in editor mode, I'm separating these out with try-catch blocks

        bool editor = !Application.isPlaying;

        //destroy joints
        try {
            foreach(var joint in GetComponentsInChildren<Joint>()) {
                if(editor) Component.DestroyImmediate(joint);
                else Component.Destroy(joint);
            }
        }catch(Exception excep) {
            UnityEngine.Debug.LogError(excep.Message);
        }

        //destroy colliders
        try {
            foreach(var coll in GetComponentsInChildren<Collider>()) {
                if(editor) Component.DestroyImmediate(coll);
                else Component.Destroy(coll);
            }
        } catch(Exception excep) {
            UnityEngine.Debug.LogError(excep.Message);
        }

        //destroy game object
        try {
            if(editor) {
                GameObject.DestroyImmediate(gameObject);
            }else{
                GameObject.Destroy(gameObject);
            }
        } catch(Exception excep) {
            UnityEngine.Debug.LogError(excep.Message);
        }
    }
}