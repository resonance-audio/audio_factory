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

using UnityEngine;

[ExecuteInEditMode]
public class PositionLerper : MonoBehaviour {
    [SerializeField] UpdateMode mode = UpdateMode.Update;
    [SerializeField] Transform start;
    [SerializeField] Transform end;
    [SerializeField] float value;
    [SerializeField] bool includeRotation = false;

    public enum UpdateMode {
        None = 0,
        Update = 1,
        LateUpdate = 2,
        Update_And_LateUpdate = Update | LateUpdate,
    }

    public float Value {
        get { return this.value; }
        set { this.value = value; DoUpdate(); }
    }

    void Update() {
        if((mode & UpdateMode.Update) != 0) {
            DoUpdate();
        }
    }

    void LateUpdate() {
        if((mode & UpdateMode.LateUpdate) != 0) {
            DoUpdate();
        }
    }

    void DoUpdate() {
        if(start != null && end != null) {
            transform.position = Vector3.Lerp(start.position, end.position, this.Value);
            if(includeRotation) {
                transform.rotation = Quaternion.Lerp(start.rotation, end.rotation, this.Value);
            }
        }
    }
}