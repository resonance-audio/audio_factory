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

public class ElevatorButton : InteractableButton {

    const string ANIM_PROP_STATE = "state";

    public enum State {
        Disabled = 0,
        Enabled = 1,
        Description = 2,
    }

    [SerializeField] public Renderer[] setRoomDescriptionRenderers = null;
    [NonSerialized] public State state = State.Disabled;

    //for animators
    public void SetState(int state) {
        this.state = (State)state;
    }

    protected override void Update() {
        base.Update();

        if(_animator != null) {
            _animator.SetInteger(ANIM_PROP_STATE, (int)state);
        }

        if(GameManager.instance != null && GameManager.instance.CurrentRoomManager != null && setRoomDescriptionRenderers != null) {
            foreach(var rend in setRoomDescriptionRenderers) {
                rend.sharedMaterial.mainTexture = GameManager.instance.CurrentRoomManager.descriptionTexture;
            }
        }
    }
}