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

[Serializable]
public class InteractableButton : BaseInteractable, IGvrPointerHoverHandler {

    public Action OnPressed { get; set; }

    const string ANIM_PROP_HOVER = "hover";
    const string ANIM_PROP_DOWN = "down";

    [SerializeField] public int hoverFrames = 3;
    [SerializeField] public Animator _animator = null;

    [NonSerialized] protected int _hoverFrame = 0;
    [NonSerialized] protected bool _isDown = false;
    [NonSerialized] protected bool _isHover = false;
    [NonSerialized] protected bool _enabled = true;

    void Awake() {
        _animator = _animator ?? GetComponent<Animator>();
        _hoverFrame = 0;
        _isDown = false;
        _isHover = false;
    }

    void OnDisable() {
        if(GlobalHoverTarget == this) {
            GlobalHoverTarget = null;
        }
    }

    void OnDestroy() {
        if(GlobalHoverTarget == this) {
            GlobalHoverTarget = null;
        }
    }

    protected virtual void Update() {
        bool gvrClicked = GvrController.ClickButtonDown;
        bool gvrDown = GvrController.ClickButton;

        _isHover = (_hoverFrame--) > 0;

        if(_isHover) {
            //only allow marking this object as down if we click the button while hovering
            if(gvrClicked) {
                _isDown = true;
                if(OnPressed != null) {
                    OnPressed.Invoke();
                }
            }else if(!gvrDown) {
                _isDown = false;
            }
        }else{
            if(GlobalHoverTarget == this) {
                GlobalHoverTarget = null;
            }
            if(!gvrDown) {
                _isDown = false;
            }
        }

        if(_animator != null) {
            _animator.SetBool(ANIM_PROP_HOVER, _isHover);
            _animator.SetBool(ANIM_PROP_DOWN, _isDown);
        }
    }

    public void OnGvrPointerHover(PointerEventData eventData)
    {
        _hoverFrame = hoverFrames;
        GlobalHoverTarget = this;

        if(GvrController.ClickButtonDown) {
            _isDown = true;
        }
    }
}