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

// using UnityEngine;

// public class TestFinaleManager : MonoBehaviour {

//     static TestFinaleManager _inst = null;
//     public static TestFinaleManager instance {
//         get {
//             if(_inst == null) {
//                 _inst = FindObjectOfType(typeof(TestFinaleManager)) as TestFinaleManager;
//             }
//             return _inst;
//         }
//     }

//     float enableTime = 0f;

//     void OnEnable() {
//         enableTime = Time.realtimeSinceStartup;
//     }

//     public float SyncTime {
//         get {
//             return Time.realtimeSinceStartup - enableTime;
//         }
//     }
// }