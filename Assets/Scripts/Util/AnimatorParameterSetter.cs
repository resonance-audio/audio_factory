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
using UnityEngine;
using UnityEngine.Serialization;

//Sets a parameter on an animator. Useful for hooking up to other animations / unityEvents.
//
//See AnimatorParameterSetterEditor.cs for the editor implementation
//
//As of Unity_5_6_x, animation of integer values is not allowed, so both float/integer animator
//   parameters are set via a float value internally - which is Mathf.Floor()'d for integer params
[RequireComponent(typeof(Animator))]
public class AnimatorParameterSetter : MonoBehaviour {

    //which behaviour-message to update the parameter on
    [Flags]
    public enum UpdateMode {
        ManualOnly  = 0, //Updates the parameter only when we call DoUpdate(...) directly
        Awake       = 1,
        OnEnable    = 2,
        Start       = 4,
        Update      = 8, //although this isn't the default enum value, I'm making it the value which unity defaults the component to, since it seems like the most desired
        LateUpdate  = 16,
        OnDisable   = 32, //Setting parameters on disabled animators might spit out warnings

        //Some esoteric combinations if want this invoking on more than one behaviour message
        AWAKE_AND_ONENABLE       = Awake    | OnEnable,
        AWAKE_AND_START          = Awake    | Start,
        AWAKE_ONENABLE_AND_START = Awake    | OnEnable | Start,
        ONENABLE_AND_START       = OnEnable | Start,
        ONENABLE_AND_ONDISABLE   = OnEnable | OnDisable,
        Update_AND_LATEUPDATE    = Update   | LateUpdate,
    }

//-----------------------------------------------------------------------------------------
#region SERIALIZED_FIELDS
    [SerializeField] public string parameter = "";
    [SerializeField] public UpdateMode mode = UpdateMode.Update;
    [SerializeField] public bool value_bool = false;
    [SerializeField, FormerlySerializedAs("value_float")] public float value_number = 0f;
#endregion SERIALIZED_FIELDS

#region NON_SERIALIZED_FIELDS
    [NonSerialized] Dictionary<int, AnimatorParametersCache> _paramsCache = new Dictionary<int, AnimatorParametersCache>();
#endregion NON_SERIALIZED_FIELDS

    class AnimatorParametersCache {

        Animator animator;
        Dictionary<string,int> _paramHashes;
        Dictionary<string,AnimatorControllerParameterType> _paramTypes;

        public AnimatorParametersCache(Animator animator) {
            this.animator = animator;
            _paramHashes = new Dictionary<string,int>();
            _paramTypes = new Dictionary<string,AnimatorControllerParameterType>();
        }

        public int GetParamHash(string param) {
            if(string.IsNullOrEmpty(param)) {
                return 0;
            }else{
                int ret;
                if(!_paramHashes.TryGetValue(param, out ret)) {
                    ret = Animator.StringToHash(param);
                    _paramHashes[param] = ret;
                }
                return ret;
            }
        }

        public AnimatorControllerParameterType GetParamType(string param) {
            if(string.IsNullOrEmpty(param)) {
                return AnimatorControllerParameterType.Int;
            }else{
                AnimatorControllerParameterType ret;
                if(!_paramTypes.TryGetValue(param, out ret)) {
                    int paramCount = animator.parameterCount;
                    for(int i=0; i<paramCount; ++i) {
                        var p = animator.GetParameter(i);
                        if(p.name == param) {
                            ret = p.type;
                            _paramTypes[param] = ret;
                            break;
                        }
                    }
                }

                return ret;
            }
        }
    }

//-----------------------------------------------------------------------------------------
#region PROPERTIES
    //value used for boolean parameters
    public bool BooleanValue {
        get { return value_bool; }
        set { value_bool = value; }
    }
    //float value used for both float and integer parameters
    public float NumberValue {
        get { return value_number; }
        set { value_number = value; }
    }
#endregion PROPERTIES

//-----------------------------------------------------------------------------------------
#region MESSAGES
    protected void Awake() {
        if((mode & UpdateMode.Awake) != 0) {
            DoUpdate();
        }
    }

    protected void OnEnable() {
        if((mode & UpdateMode.OnEnable) != 0) {
            DoUpdate();
        }
    }

    protected void Start() {
        if((mode & UpdateMode.Start) != 0) {
            DoUpdate();
        }
    }

    protected void Update() {
        if((mode & UpdateMode.Update) != 0) {
            DoUpdate();
        }
    }

    protected void LateUpdate() {
        if((mode & UpdateMode.LateUpdate) != 0) {
            DoUpdate();
        }
    }
    
    protected void OnDisable() {
        if((mode & UpdateMode.OnDisable) != 0) {
            DoUpdate();
        }
    }
#endregion MESSAGES
    
//-----------------------------------------------------------------------------------------
#region ACTUAL_UPDATING
    //update a boolean parameter
    public void DoUpdate(bool booleanValue) {
        bool prev = value_bool;
        value_bool = booleanValue;
        DoUpdate();
        value_bool = prev;
    }
    //update an integer value
    public void DoUpdate(int integerValue) {
        float prev = value_number;
        value_number = (float)integerValue;
        DoUpdate();
        value_number = prev;
    }
    //update a float value
    public void DoUpdate(float floatValue) {
        float prev = value_number;
        value_number = floatValue;
        DoUpdate();
        value_number = prev;
    }
    //the actual parameter-setting function
    public void DoUpdate() {
        if(parameter == null || parameter.Length == 0) return;

        var animator = GetComponentInParent<Animator>();
        int animatorId = animator.GetInstanceID();

        AnimatorParametersCache cache;
        if(!_paramsCache.TryGetValue(animatorId, out cache)) {
            cache = new AnimatorParametersCache(animator);
            _paramsCache[animatorId] = cache;
        }

        int hash = cache.GetParamHash(parameter);
        var paramType = cache.GetParamType(parameter);

        switch(paramType) {
            case AnimatorControllerParameterType.Bool:
            animator.SetBool(hash, value_bool);
            break;
            case AnimatorControllerParameterType.Float:
            animator.SetFloat(hash, value_number);
            break;
            case AnimatorControllerParameterType.Int:
            animator.SetInteger(hash, (int)Mathf.Floor(value_number));
            break;
            case AnimatorControllerParameterType.Trigger:
            animator.SetTrigger(hash);
            break;
        }
    }
#endregion ACTUAL_UPDATING
}