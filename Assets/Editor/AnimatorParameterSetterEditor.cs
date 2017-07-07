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
using System;
using UnityEditor;

//Editor implementation for AnimatorParameterSetter.cs
[CustomEditor(typeof(AnimatorParameterSetter))]
public class AnimatorParameterSetterEditor : Editor {
    public override void OnInspectorGUI() {
        var obj = target as AnimatorParameterSetter;
        if(obj == null) return;

        Animator animator = obj.GetComponentInParent<Animator>() as Animator;

        //as of 5_6_f3, putting an empty avatar onto the animator seems to fix the "animator is not playing an AnimatorController" warning
        if(animator.runtimeAnimatorController != null && animator.avatar == null) {
            if(GUILayout.Button("Generate Avatar")) {
                string path = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
                path = path.Split(new string[1]{".controller"}, StringSplitOptions.RemoveEmptyEntries)[0] + "_avatar.avatar";
                var avatar = AvatarBuilder.BuildGenericAvatar(animator.transform.root.gameObject, "");
                AssetDatabase.CreateAsset(avatar, path);
                animator.avatar = avatar;
            }
        }

        if(animator != null && animator.runtimeAnimatorController != null) {

            var serializedUpdateMode = serializedObject.FindProperty("mode");
            EditorGUILayout.PropertyField(serializedUpdateMode, new GUIContent("Mode: "));
            if(animator.parameterCount == 0) return;
            
            AnimatorControllerParameter[] parameters = new AnimatorControllerParameter[animator.parameterCount];
            string[] parameterNames = new string[animator.parameterCount];
            for(int i=0; i<animator.parameterCount; ++i) {
                parameters[i] = animator.GetParameter(i);
                parameterNames[i] = parameters[i].name;
            }

            SerializedProperty serializedParameter = serializedObject.FindProperty("parameter");
            int parameterIndex = 0;
            for(int i=0; i<parameterNames.Length; ++i) {
                if(string.Equals(parameterNames[i], serializedParameter.stringValue, StringComparison.Ordinal)) {
                    parameterIndex = i;
                    break;
                }
            }
            parameterIndex = EditorGUILayout.Popup("Parameter: ", parameterIndex, parameterNames);
            if(parameterIndex >= 0 && parameterIndex < parameterNames.Length) {
                serializedParameter.stringValue = parameterNames[parameterIndex];
                AnimatorControllerParameter selectedParameter = parameters[parameterIndex];
                if(selectedParameter != null) {
                    switch(selectedParameter.type) {
                        case AnimatorControllerParameterType.Bool:
                        SerializedProperty boolProp = serializedObject.FindProperty("value_bool");
                        EditorGUILayout.PropertyField(boolProp, new GUIContent("Value: "));
                        break;
                        case AnimatorControllerParameterType.Float:
                        SerializedProperty floatProp = serializedObject.FindProperty("value_number");
                        EditorGUILayout.PropertyField(floatProp, new GUIContent("Value: "));
                        break;
                        case AnimatorControllerParameterType.Int:
                        SerializedProperty intProp = serializedObject.FindProperty("value_number");
                        EditorGUILayout.PropertyField(intProp, new GUIContent("Value: "));
                        break;
                        case AnimatorControllerParameterType.Trigger:
                        break;
                    }
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }
}
