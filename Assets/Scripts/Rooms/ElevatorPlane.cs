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

using System.Linq;
using UnityEngine;

public class ElevatorPlane : MonoBehaviour {

    [SerializeField] public Color editorColor = Color.magenta;

    void OnDrawGizmos() {
        #if UNITY_EDITOR
        if(UnityEditor.Selection.gameObjects != null && UnityEditor.Selection.gameObjects.Contains(gameObject)) {
            Color col = Gizmos.color;
            Gizmos.color = new Color(editorColor.r, editorColor.g, editorColor.b, 0.25f);
            Gizmos.DrawCube(transform.position, new Vector3(100f, 0f, 100f));
            Gizmos.color = editorColor;
            Gizmos.DrawWireCube(transform.position, new Vector3(100f, 0f, 100f));
            Gizmos.color = col;
        }
        #endif
    }
}