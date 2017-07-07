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
using UnityEngine.EventSystems;

namespace AudioEngineer.Rooms.Finale {

    public class FinaleTogglable : MonoBehaviour, IGvrPointerHoverHandler {

        [SerializeField] public Animator animator = null;
        [SerializeField] public bool onByDefault = false;

        const string ANIM_PROP_HOVER = "hover";
        const string ANIM_PROP_DOWN = "down";
        const string ANIM_PROP_ON = "on";

        //Whether the pointer is currently hovered over this object's collider
        public bool IsHover { get; protected set; }
        //Whether the pointer was clicked while hovering, and is still being held down
        public bool IsDown { get; protected set; }
        public bool IsOn { get; protected set; }

        //Set IsHover to true every frame which we're hovering, then set to false at the end of the frame
        public void OnGvrPointerHover(PointerEventData eventData) {
            IsHover = true;
        }

        void OnEnable() {
            IsOn = onByDefault;
        }

        void OnClicked() {
            IsOn = !IsOn;
        }

        void LateUpdate() {

            //These accessors can throw sometimes if something is not set up correctly with the gvr objects in the scene; so wrapping this in a try-cath block
            bool gvrClicked = false;
            bool gvrDown = false;
            try {
                gvrClicked = GvrController.ClickButtonDown;
                gvrDown = GvrController.ClickButton;
            } catch (NullReferenceException) {
                gvrClicked = false;
                gvrDown = false;
            }

            //Only allow setting IsDown to true while hovering
            if(IsHover) {
                IsDown = gvrDown;
                if(IsDown && gvrClicked) {
                    OnClicked();
                }
            //But allow IsDown afterwards, even if we're not hovering
            }else{
                if(!gvrDown) {
                    IsDown = false;
                }
            }

            if(animator != null) {
                animator.SetBool(ANIM_PROP_HOVER, IsHover);
                animator.SetBool(ANIM_PROP_DOWN, IsDown);
                animator.SetBool(ANIM_PROP_ON, IsOn);
            }

            IsHover = false;
        }
    }

}