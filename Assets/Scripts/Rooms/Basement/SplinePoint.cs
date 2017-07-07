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

public class SplinePoint : MonoBehaviour {
    
    void OnDrawGizmos() {
        #if UNITY_EDITOR
        SplinePath parent = GetComponentInParent<SplinePath>();
        if(parent != null) {
            Color col = Gizmos.color;
            Color pathCol = parent.editorColor;
            Gizmos.color = (UnityEditor.Selection.gameObjects != null && UnityEditor.Selection.gameObjects.Contains(gameObject)) ? pathCol : new Color(pathCol.r, pathCol.g, pathCol.b, pathCol.a*0.25f);
            Gizmos.DrawCube(transform.position, transform.localScale * 0.05f);
            Gizmos.color = col;
        }
        #endif
    }
    
}