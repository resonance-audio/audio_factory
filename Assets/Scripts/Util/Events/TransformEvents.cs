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

//Simple transform-related functions which may be invoked via UnityEvents
public class TransformEvents : MonoBehaviour {

    [SerializeField] public float floatMultiplier = 1f;
    [SerializeField] public int intMultiplier = 1;

    public void RotateAboutAxis_X(float angle) {
        transform.rotation *= Quaternion.AngleAxis(angle, Vector3.right);
    }
    public void RotateAboutAxis_Y(float angle) {
        transform.rotation *= Quaternion.AngleAxis(angle, Vector3.up);
    }
    public void RotateAboutAxis_Z(float angle) {
        transform.rotation *= Quaternion.AngleAxis(angle, Vector3.forward);
    }
    public void SetRotateAboutAxis_X(float angle) {
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.right);
    }
    public void SetRotateAboutAxis_Y(float angle) {
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
    }
    public void SetRotateAboutAxis_Z(float angle) {
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}