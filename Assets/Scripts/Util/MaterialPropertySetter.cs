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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//Component which exposes shared/instance material properties so they may be modified by animations/unity-events
[ExecuteInEditMode]
public class MaterialPropertySetter : MonoBehaviour {

    
//-----------------------------------------------------------------------------------------
#region SERIALIZED_FIELDS
    [SerializeField] public OptimizationLevel optimizationLevel;
    [SerializeField] public bool _updateInEditor;
    [SerializeField] public bool _useSharedMaterial;
    [SerializeField] public Renderer rend;
    [SerializeField] public int materialIndex = 0;
    [SerializeField] public string propertyName;
    [SerializeField] public PropertyType _propertyType = PropertyType.Color;
    [SerializeField] public Color value_color;
    [SerializeField] public float value_float;
    [SerializeField] public Texture value_tex;
    [SerializeField] public Vector4 value_vec4;
#endregion SERIALIZED_FIELDS

    MaterialCache _materialCache;
    static Dictionary<string, int> _propertyIds = new Dictionary<string, int>();
    static Dictionary<int, Dictionary<int,bool>> _cachedValidProperties = new Dictionary<int, Dictionary<int,bool>>();

    [Serializable]
    public enum OptimizationLevel {
        Default        = 0,
        CacheMaterials = 1,
    }

    struct MaterialCache {
        public bool shared;
        public Material[] mats;
    }

//-----------------------------------------------------------------------------------------
#region METHODS
    //changing this enum order will break previous serializations
    public enum PropertyType {
        Color   = 0,
        Float   = 1,
        Range   = 2,
        Texture = 3,
        Vector  = 4,
    }

    void Awake() {
        if(rend == null) {
            rend = GetComponent<Renderer>();
        }
    }

    static bool IsParent(GameObject parent, GameObject child) {
        if(parent == null || child == null) return false;

        Transform curr = child.transform.parent;
        while(curr != null) {
            if(curr.gameObject == parent) return true;
            curr = curr.parent;
        }

        return false;
    }

    static bool AreAnyOfTheseParents(IEnumerable<GameObject> potentialParents, GameObject child) {
        if(potentialParents == null || child == null) return false;
        for(var itor = potentialParents.GetEnumerator(); itor.MoveNext();) {
            if(IsParent(itor.Current, child)) return true;
        }
        return false;
    }

    void Update() {

        //by default, only update in play mode
        bool update = Application.isPlaying;

        #if UNITY_EDITOR

        //if _updateInEditor is true...
        if(!update && this._updateInEditor) {

            //if we're previewing an animation, update the setters which are childed under the current selection
            if(UnityEditor.AnimationMode.InAnimationMode()) {
                if(AreAnyOfTheseParents(UnityEditor.Selection.gameObjects, gameObject)) {
                    update = true;
                }

            //otherwise, only update the setters which are selected
            }else{
                if(UnityEditor.Selection.gameObjects != null && UnityEditor.Selection.gameObjects.Any(obj => obj == this.gameObject)) {
                    update = true;
                }
            }
        }
        #endif

        if(update) {
            DoUpdate();
        }
    }

    public void ClearCache() {
        _materialCache = default(MaterialCache);
    }

    public void ResetMaterialPropertyBlock(string id) {
        if(rend != null) {
            rend.SetPropertyBlock(GlobalMaterialPropertyBlocks.Get(id));
        }
    }

    public bool IsValidProperty(Material mat, int propertyId) {
        if(mat == null) return false;
        int matId = mat.GetInstanceID();
        Dictionary<int,bool> dict;
        if(!_cachedValidProperties.TryGetValue(matId, out dict)) {
            dict = new Dictionary<int,bool>();
            _cachedValidProperties[matId] = dict;
        }

        bool isPropValid;
        if(!dict.TryGetValue(propertyId, out isPropValid)) {
            isPropValid = mat.HasProperty(propertyId);
            dict[propertyId] = isPropValid;
        }

        return isPropValid;
    }

    Material GetTargetMaterial() {
        if(materialIndex < 0) return null;

        if((optimizationLevel & OptimizationLevel.CacheMaterials) != 0) {
            if(_materialCache.mats == null) {
                _materialCache = new MaterialCache() {
                    shared = _useSharedMaterial,
                    mats = Application.isPlaying && !_useSharedMaterial
                        ? rend.materials
                        : rend.sharedMaterials,
                };
            }

            if(_materialCache.mats != null && materialIndex < _materialCache.mats.Length) {
                return _materialCache.mats[materialIndex];
            }else{
                return null;
            }
        }else{
            Material[] materials = Application.isPlaying && !_useSharedMaterial
                ? rend.materials
                : rend.sharedMaterials;
                if(materials != null && materialIndex < materials.Length) {
                    return materials[materialIndex];
                }else{
                    return null;
                }
        }
    }

    public void DoUpdate() {
        if(rend == null) return;
        bool isPlaying = Application.isPlaying;

        int id;
        if(!_propertyIds.TryGetValue(propertyName, out id)) {
            id = Shader.PropertyToID(propertyName);
            _propertyIds[propertyName] = id;
        }

        if(!IsValidProperty(rend.sharedMaterial, id)) {
            return;
        }

        Material material = GetTargetMaterial();
        if(material != null) {
            switch(_propertyType) {
                default: case PropertyType.Color: material.SetColor(id, value_color);
                break;
                case PropertyType.Float: material.SetFloat(id, value_float);
                break;
                case PropertyType.Range: material.SetFloat(id, value_float);
                break;
                case PropertyType.Texture: material.SetTexture(id, value_tex);
                break;
                case PropertyType.Vector: material.SetVector(id, value_vec4);
                break;
            }
        }
    }
#endregion METHODS
}
