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

 //https://gist.github.com/loolo78/cadb84af3150a707b47f7c9c9a2dce6a
 
 using UnityEngine;
 using MaterialDictionary = System.Collections.Generic.Dictionary<UnityEngine.Material, UnityEngine.Material>;
 
 //Component which instantiates shared-materials into a static dictionary, so that accessing Renderer.sharedMaterial properties does not modify the actual project asset at runtime.
 public class SharedMaterialReinstantiater : MonoBehaviour {
 
   private static readonly MaterialDictionary _cache = new MaterialDictionary();
 
   void Awake() {
     var renderer1 = GetComponent<Renderer>();
     Material cachedMat;
     if (!_cache.TryGetValue(renderer1.sharedMaterial, out cachedMat)) {
       cachedMat = new Material(renderer1.sharedMaterial);
       _cache.Add(renderer1.sharedMaterial, cachedMat);
     }
     renderer1.sharedMaterial = cachedMat;
   }
 }