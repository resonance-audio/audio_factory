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
using UnityEditor;
using UnityEngine;

//Editor implementation for MaterialPropertySetter.cs
[CustomEditor(typeof(MaterialPropertySetter))]
public class MaterialPropertySetterEditor : Editor {

    public override void OnInspectorGUI() {
        MaterialPropertySetter obj = target as MaterialPropertySetter;
        if(obj == null) return;
        var serialized = new SerializedObject(target);
        if(serialized == null) return;

        var serializedRenderer = serialized.FindProperty("rend");
        obj._updateInEditor = EditorGUILayout.Toggle("Update in edit-mode", obj._updateInEditor);
        obj._useSharedMaterial = EditorGUILayout.Toggle("Use shared-materials", obj._useSharedMaterial);
        EditorGUILayout.PropertyField(serializedRenderer, new GUIContent("Renderer: "));
        Renderer rend = serializedRenderer.objectReferenceValue as Renderer;
        Material selectedMaterial = null;
        if(rend != null) {
            int selectedMaterialIndex = 0;
            if(rend.sharedMaterials != null && rend.sharedMaterials.Length > 1) {
                int materialCount = rend.sharedMaterials.Length;
                string[] materialSelection = new string[materialCount];
                for(int m=0; m<rend.sharedMaterials.Length; ++m) {
                    materialSelection[m] = rend.sharedMaterials[m] != null ? rend.sharedMaterials[m].name : "NULL_MATERIAL";
                }
                selectedMaterialIndex = Mathf.Max(0, EditorGUILayout.Popup("Material: ", obj.materialIndex, materialSelection));
                selectedMaterial = rend.sharedMaterials[obj.materialIndex];
            }else if(rend.sharedMaterial != null){
                selectedMaterial = rend.sharedMaterial;
            }

            serializedObject.FindProperty("materialIndex").intValue = selectedMaterialIndex;
            obj.materialIndex = selectedMaterialIndex;

            var serializedPropertyName = serializedObject.FindProperty("propertyName");

            if(selectedMaterial != null && serializedRenderer.objectReferenceValue != null) {
                Shader shader = selectedMaterial.shader;
                string selectedShaderProp = serializedPropertyName.stringValue;
                int shaderPropCount = ShaderUtil.GetPropertyCount(shader);
                List<string> shaderPropertiesList = new List<string>();
                for(int sp=0; sp<shaderPropCount; ++sp) {
                    shaderPropertiesList.Add(ShaderUtil.GetPropertyName(shader, sp));
                }
                bool missingSelectedShaderProp = false;
                if(!shaderPropertiesList.Contains(selectedShaderProp)) {
                    missingSelectedShaderProp = true;
                    shaderPropertiesList.Add(selectedShaderProp);
                }
                GUIContent[] shaderProperties = new GUIContent[shaderPropertiesList.Count];
                for(int i=0; i<shaderProperties.Length; ++i) {
                    string contentPrefix = missingSelectedShaderProp && shaderPropertiesList[i] == selectedShaderProp
                        ? "(MISSING) "
                        : "";
                        
                    shaderProperties[i] = new GUIContent(contentPrefix + shaderPropertiesList[i]);
                }
                int shaderPropIndex = -1;
                for(int i=0; i<shaderPropertiesList.Count; ++i) {
                    if(shaderPropertiesList[i] == selectedShaderProp) {
                        shaderPropIndex = i;
                    }
                }
                shaderPropIndex = Mathf.Max(0, EditorGUILayout.Popup(new GUIContent("Shader property: "), shaderPropIndex, shaderProperties));
                if(shaderPropIndex >= 0 && shaderPropIndex < shaderProperties.Length) {
                    obj.propertyName = shaderPropertiesList[shaderPropIndex];
                    serializedPropertyName.stringValue = shaderPropertiesList[shaderPropIndex];
                }

                serialized.ApplyModifiedProperties();
                if(shaderPropIndex < shaderPropCount) {
                    ShaderUtil.ShaderPropertyType shaderPropType = ShaderUtil.GetPropertyType(shader, shaderPropIndex);
                    var serializedPropertyType = serializedObject.FindProperty("_propertyType");
                    switch(shaderPropType) {
                        
                        default: case ShaderUtil.ShaderPropertyType.Color: {
                            var serializedColorField = serialized.FindProperty("value_color");
                            EditorGUILayout.PropertyField(serializedColorField, new GUIContent("Value: "));
                            obj._propertyType = MaterialPropertySetter.PropertyType.Color;
                        break; }

                        case ShaderUtil.ShaderPropertyType.Range:
                        case ShaderUtil.ShaderPropertyType.Float: {
                            var serializedFloatField = serialized.FindProperty("value_float");
                            EditorGUILayout.PropertyField(serializedFloatField, new GUIContent("Value: "));
                            obj._propertyType = MaterialPropertySetter.PropertyType.Float;
                        break; }

                        case ShaderUtil.ShaderPropertyType.TexEnv: {
                            var serializedTextureField = serialized.FindProperty("value_tex");
                            EditorGUILayout.PropertyField(serializedTextureField, new GUIContent("Value: "));
                            obj._propertyType = MaterialPropertySetter.PropertyType.Texture;
                        break; }

                        case ShaderUtil.ShaderPropertyType.Vector: {
                            var serializedVectorField = serialized.FindProperty("value_vec4");
                            serializedVectorField.vector4Value = EditorGUILayout.Vector4Field(new GUIContent("Value: "), serializedVectorField.vector4Value);
                            obj._propertyType = MaterialPropertySetter.PropertyType.Vector;
                        break; }
                    }
                }
            }
        }

        serialized.ApplyModifiedProperties();

        if(obj._updateInEditor) {
            obj.DoUpdate();
        }
    }
}