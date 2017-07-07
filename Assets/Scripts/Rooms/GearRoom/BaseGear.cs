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
using UnityEngine.Serialization;

namespace AudioEngineer.Rooms.Gears {

    //Base-class for gears which must engage dynamically in gear-room
    public class BaseGear : MonoBehaviour {
        [SerializeField] public float engageDistance = 1f;
        [SerializeField] public float collisionDistance = 0.9f;
        [FormerlySerializedAsAttribute("radiusCoefficient")]
        [SerializeField] public int teethCount = 16;
        [SerializeField] public float teethOffset = 0f;
        [SerializeField] public bool isInternalGear = false;

        protected virtual void OnValidate() {
            if(isInternalGear && collisionDistance < engageDistance) {
                collisionDistance = engageDistance;
            }else if(!isInternalGear && collisionDistance > engageDistance) {
                collisionDistance = engageDistance;
            }
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos() {
            Color col = Gizmos.color;

            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            GizmosUtil.DrawCircle(transform.position, transform.rotation, engageDistance, teethCount);
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            GizmosUtil.DrawCircle(transform.position, transform.rotation, collisionDistance, teethCount);

            Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
            float teethOffsetRadians = teethOffset * Mathf.PI / 180f;
            float rotationOffset = transform.eulerAngles.z * Mathf.PI / 180f;
            for(int i=0; i<teethCount; ++i) {
                float percent = (float)i / (float)teethCount;
                Gizmos.DrawLine(
                    transform.position + transform.rotation * new Vector3(Mathf.Cos(teethOffsetRadians + Mathf.PI * 2f * percent + rotationOffset) * collisionDistance, 
                                Mathf.Sin(teethOffsetRadians + Mathf.PI * 2f * percent + rotationOffset) * collisionDistance, 
                                0f), 
                    transform.position + transform.rotation * new Vector3(Mathf.Cos(teethOffsetRadians + Mathf.PI * 2f * percent + rotationOffset) * engageDistance,
                                Mathf.Sin(teethOffsetRadians + Mathf.PI * 2f * percent + rotationOffset) * engageDistance,
                                0f));



            }

            Gizmos.color = col;
        }
#endif

    }
}