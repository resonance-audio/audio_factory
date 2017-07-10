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

//Definition data-class for floors/rooms
[CreateAssetMenuAttribute(fileName = "Data", menuName = "AudioEngineer/Data/Room", order = 1)]
[Serializable]
public class RoomDefinition : ScriptableObject {
    [SerializeField] public string scenePath = "";
    [SerializeField] public RoomSpecificElevatorAnimatorController roomSpecificElevatorAnimController = null;
    [SerializeField] public float maxLaserDistance = 5f;
    [SerializeField] public float maxReticleDistance = 5f;
}