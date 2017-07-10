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

public class InvisibleBoundsBox : MonoBehaviour {

    [SerializeField] public Transform[] children = null;
    [SerializeField] public Vector3 size = Vector3.one;
    [SerializeField] public Color editorColor = Color.white;

    public Bounds WorldBounds {
        get {
            return new Bounds() {
                center = this.transform.position,
                size = this.size,
                extents = this.size/2f,
                min = this.transform.position - this.size/2f,
                max = this.transform.position + this.size/2f,
            };
        }
    }

    public Bounds LocalBounds {
        get {
            return new Bounds() {
                center = this.transform.localPosition,
                size = this.size,
                extents = this.size/2f,
                min = this.transform.localPosition - this.size/2f,
                max = this.transform.localPosition + this.size/2f,
            };
        }
    }

    void Update() {
        if(children == null || children.Length == 0) return;

        Bounds bounds = WorldBounds;
        foreach(var child in children) {
            if(child.position.x < bounds.min.x)
                child.position = child.position.WithX(bounds.min.x);
            if(child.position.x > bounds.max.x)
                child.position = child.position.WithX(bounds.max.x);

            if(child.position.y < bounds.min.y)
                child.position = child.position.WithY(bounds.min.y);
            if(child.position.y > bounds.max.y)
                child.position = child.position.WithY(bounds.max.y);

            if(child.position.z < bounds.min.z)
                child.position = child.position.WithZ(bounds.min.z);
            if(child.position.z > bounds.max.z)
                child.position = child.position.WithZ(bounds.max.z);
        }
    }

    #if UNITY_EDITOR
    void OnValidate() {
        transform.localEulerAngles = Vector3.zero;
    }
    void OnDrawGizmos() {
        
        GameObject[] objs = UnityEditor.Selection.gameObjects;
        bool selected = objs.Contains(this.gameObject);
        Color previousColor = Gizmos.color;
        Gizmos.color = selected ? editorColor.WithAlpha(0.25f) : editorColor.WithAlpha(0.1f);
        Gizmos.DrawCube(transform.position, size);
        if(selected) {
            Gizmos.color = editorColor.WithAlpha(0.75f);
            Gizmos.DrawWireCube(transform.position, size);
        }
        Gizmos.color = previousColor;
    }
    #endif
}