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
using UnityEngine;

namespace AudioEngineer.Rooms.Basement {

    public class Generator : MonoBehaviour {
        const string ANIM_PROP_IS_READY = "ready";
        const string ANIM_PROP_IS_ON = "on";
        const string ANIM_PROP_IS_DRAGGING = "dragging";
        const string ANIM_PROP_IS_HOVER = "hover";

        [SerializeField] public float plugSnapRadius = 1f;
        [SerializeField] public Transform plugSnapPoint = null;
        [SerializeField] public Animator[] animators = null;
        [SerializeField] public int requiredGeneratorsToBeReady = 0;
        [NonSerialized] Plug[] plugs = null;

        bool _isOn = false;
        bool _isReady = false;

        public bool IsDragging {
            get {
                if(!IsReady) return false;

                plugs = plugs ?? FindObjectsOfType<Plug>();
                for(int p=0; p<plugs.Length; ++p) {
                    if(plugs[p] != null && plugs[p].isDown && !plugs[p].isPlugged) {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsHover {
            get {
                if(!IsReady) return false;

                plugs = plugs ?? FindObjectsOfType<Plug>();
                for(int p=0; p<plugs.Length; ++p) {
                    if(plugs[p] != null && plugs[p].isHover && !plugs[p].isPlugged) {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool IsReady {
            get {
                return _isReady;
            }
            set {
                if(value != _isReady) {
                    _isReady = value;
                    if(!value) {
                        IsOn = false;
                    }

                    foreach(var animator in animators) {
                        if(animator != null) {
                            animator.SetBool(ANIM_PROP_IS_READY, value);
                        }
                    }
                }
            }
        }

        public bool IsOn {
            get {
                return _isOn;
            }
            set {
                //only setting the animator property if this changed means we
                //   should also do this on initialize/enable
                if(value != _isOn) {
                    _isOn = value;
                    if(value) {
                        IsReady = true;
                        var baseman = BasementRoomManager.instance;
                        if(baseman != null) {
                            baseman.GeneratorTurnedOn(this);
                        }
                    }
                    foreach(var animator in animators) {
                        if(animator != null) {
                            animator.SetBool(ANIM_PROP_IS_ON, value);
                        }
                    }
                }
            }
        }

        void OnEnable() {
            foreach(var animator in animators) {
                if(animator != null) {
                    animator.SetBool(ANIM_PROP_IS_ON, IsOn);
                }
            }
        }

        void Update() {
            var baseman = BasementRoomManager.instance;
            if(baseman != null) {
                IsReady = baseman.CompletedGeneratorCount >= requiredGeneratorsToBeReady;
            }
            
            if(animators != null) {
                foreach(var anim in animators) {
                    anim.SetBool(ANIM_PROP_IS_DRAGGING, IsDragging);
                    anim.SetBool(ANIM_PROP_IS_HOVER, IsHover);
                }
            }
        }

        void OnDrawGizmos() {
            Color col = Gizmos.color;
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere((plugSnapPoint ?? transform).position, plugSnapRadius);
            Gizmos.DrawCube((plugSnapPoint ?? transform).position, Vector3.one * 0.05f);
            Gizmos.color = col;
        }
    }
} 