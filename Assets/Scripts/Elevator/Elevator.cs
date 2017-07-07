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
using System.Collections;
using UnityEngine;

public class Elevator : MonoBehaviour {

    static Elevator _inst = null;
    public static Elevator instance {
        get {
            if(_inst == null) _inst = FindObjectOfType(typeof(Elevator)) as Elevator;
            return _inst;
        }
    }


//------------------------------------------------------------
#region NESTED_DEFINITIONS

    const string ANIM_PROP_MOVING = "moving";
    //highest velocity value to clip to 0f, due to eye-balling animation curves
    const float VELOCITY_CLIP = 0.0005f;

    public enum State {
        None,
        //moving from the bottom of the shaft to the main floor
        FromBottomToFloor,
        //waiting at the floor
        IdleAtFloor,
        //moving from the floor to the ceiling
        FromFloorToCeil,
        //moving from the ceiling to the top
        FromCeilToTop,
    }

#endregion NESTED_DEFINITIONS

//------------------------------------------------------------
#region SERIALIZED_FIELDS
    [SerializeField] public ElevatorLever upLever;
    [SerializeField] public Transform roomSpecificAnimatorControllerTarget = null;
    [SerializeField] public float elevatorMovementSpeed = 1f;
    [SerializeField] public AnimationCurve velocityCurve = null;
    [SerializeField] public AnimationCurve stoppingCurve = null;
    [SerializeField] public float roundingThreshold = 0.0001f;
#endregion SERIALIZED_FIELDS

//------------------------------------------------------------
#region NON_SERIALIZED_FIELDS
    [NonSerialized] bool _hasPressedButton = false;
    [NonSerialized] bool _isIdle = false;
    [NonSerialized] bool _isMoving = false;
    [NonSerialized] float _movementTimer = 0f;
    [NonSerialized] GvrLaserPointer _laserPointer = null;
    [SerializeField] public Animator elevatorAnims = null;

#endregion NON_SERIALIZED_FIELDS

    //Whether the elevator transform is currently moving; NOT to be confused with the animator's "moving" parameter
    public bool IsMoving {
        get { return _isMoving; }
    }

//------------------------------------------------------------
#region METHODS

    protected void OnEnable() {
        _hasPressedButton = false;
        _laserPointer = GetComponentInChildren<GvrLaserPointer>();

        upLever.OnPressed = PressedButton;


#if UNITY_EDITOR
        StartCoroutine(_SetupEditorVRMode());
#endif
    }

#if UNITY_EDITOR
    IEnumerator _SetupEditorVRMode() {
        yield return new WaitForEndOfFrame();

    }
#endif

    public void SetElevatorAnims(Animator anims) {
        elevatorAnims = anims;
    }

    protected void Update() {
        //we're probably playing from a specific scene; don't move the elevator
        if(GameManager.instance == null) return;

        if(velocityCurve == null) velocityCurve = new AnimationCurve();
        velocityCurve.postWrapMode = WrapMode.ClampForever;
        velocityCurve.preWrapMode = WrapMode.ClampForever;

        if(GameManager.instance.CurrentRoom != null && _laserPointer != null) {
            _laserPointer.maxLaserDistance = GameManager.instance.CurrentRoom.maxLaserDistance;
            _laserPointer.maxReticleDistance = GameManager.instance.CurrentRoom.maxReticleDistance;
        }

        if(GameManager.instance.CurrentRoomManager != null) {


            //the current room is complete and we've pressed the elevator button
            if(GameManager.instance.CurrentRoomManager.State != BaseRoomManager.RoomState.Incomplete && (_hasPressedButton || GameManager.instance.CurrentRoomManager.State == BaseRoomManager.RoomState.Skip)) {
                //clamp the velocity curve fudging by the VELOCITY_CLIP value
                float velocity = velocityCurve.Evaluate(_movementTimer);
                if(velocity < VELOCITY_CLIP) velocity = 0f;

                transform.position = transform.position + Vector3.up * elevatorMovementSpeed * velocity * Time.deltaTime;
                elevatorAnims.SetBool(ANIM_PROP_MOVING, true);
                _isMoving = (velocity != 0f);
                _isIdle = false;
                upLever.On = false;
                _movementTimer += Time.deltaTime;
                _hasPressedButton = false;
                if(GameManager.instance.CurrentRoomManager.State == BaseRoomManager.RoomState.Skip) {
                    GameManager.instance.UnloadPreviousRoom();
                }
                var nextRoom = GameManager.instance.config.roomConfiguration.GetNextRoom(GameManager.instance.CurrentRoom);
                GameManager.instance.LoadRoom(nextRoom);
                //create the animator on this elevator which is specific to the current room
                CreateRoomSpecificAnimatorController(nextRoom);
            }else{
                //we're done loading the next room, but we're still moving through the elevator shaft
                if(GameManager.instance.CurrentRoomManager.floorPlane.transform.position.y - transform.position.y > roundingThreshold){

                    //if we've passed through the previous room's ceiling, trigger the lighting transition
                    if(GameManager.instance.PreviousRoomManager != null && transform.position.y >= GameManager.instance.PreviousRoomManager.ceilingPlane.transform.position.y) {
                        GameManager.instance.StartLightingTransition();
                    }

                    float delta = GameManager.instance.CurrentRoomManager.floorPlane.transform.position.y - transform.position.y;
                    float speed = 0f;
                    float stoppingTime = stoppingCurve.keys[stoppingCurve.length-1].time;

                    if(delta < stoppingTime) {
                        speed = elevatorMovementSpeed * stoppingCurve.Evaluate(delta / stoppingTime);
                    }else{
                        speed = elevatorMovementSpeed * velocityCurve.Evaluate(_movementTimer);
                    }
                    transform.position = transform.position + Vector3.up * speed * Time.deltaTime;
                    
                    elevatorAnims.SetBool(ANIM_PROP_MOVING, true);
                    _isMoving = true;
                    _isIdle = false;
                    upLever.On = false;
                    _movementTimer += Time.deltaTime;
                }else {
                    //we've reached the floor of the next room; stop the elevator and unload the previous room
                    transform.position = new Vector3(transform.position.x, GameManager.instance.CurrentRoomManager.floorPlane.transform.position.y, transform.position.z);
                    elevatorAnims.SetBool(ANIM_PROP_MOVING, false);
                    _isMoving = false;
                    if(GameManager.instance.CurrentRoomManager.State == BaseRoomManager.RoomState.Complete) {
                        upLever.On = true;
                    }else{
                        upLever.On = false;
                    }
                    _movementTimer = 0f;

                    //invoke this once upon becoming idle
                    if(!_isIdle) {
                        _isIdle = true;
                        GameManager.instance.UnloadPreviousRoom();
                        CreateRoomSpecificAnimatorController(GameManager.instance.CurrentRoom);
                    }
                }
            }
        }else{
            if(GameManager.instance != null && GameManager.instance.CurrentRoom != null) {
                //we've passed the ceiling of the previous room, but we're not done loading the next room. Just keep moving upwards
                float speed = elevatorMovementSpeed * velocityCurve.Evaluate(_movementTimer);
                transform.position = transform.position + Vector3.up * speed * Time.deltaTime;

                elevatorAnims.SetBool(ANIM_PROP_MOVING, true);
                _isMoving = true;
                _isIdle = false;
                upLever.On = false;
                _movementTimer += Time.deltaTime;
            }
        }
    }

    RoomSpecificElevatorAnimatorController CreateRoomSpecificAnimatorController(RoomDefinition room) {

        //no matter what's null, destroy the current controller
        RoomSpecificElevatorAnimatorController contr = null;
        if(roomSpecificAnimatorControllerTarget != null) {
            contr = roomSpecificAnimatorControllerTarget.GetComponent<RoomSpecificElevatorAnimatorController>();
        }

        //if null room, error out
        if(room == null) {
            UnityEngine.Debug.LogError("Null room definition");
            return default(RoomSpecificElevatorAnimatorController);
        }
        //if not room anim controller, error out
        if(room.roomSpecificElevatorAnimController == null) {
            UnityEngine.Debug.LogError("No room specific elevator animator controller");
            return default(RoomSpecificElevatorAnimatorController);
        }else{
            //if the controller already exists and is of the same type as the current room, just return it
            if(contr != null && contr.GetType() == room.roomSpecificElevatorAnimController.GetType()) {
                return contr;
            }
        }

        //destroy the old controller
        if(contr != null) {
            Component.Destroy(contr);
        }

        if(roomSpecificAnimatorControllerTarget != null) {
            roomSpecificAnimatorControllerTarget.GetOrAddComponent<Animator>();
            
            //add a new controller component of the proper type
            contr = roomSpecificAnimatorControllerTarget.gameObject.AddComponent(room.roomSpecificElevatorAnimController.GetType()) as RoomSpecificElevatorAnimatorController;
            //initialize the controller with the prefab values, since we didn't instantiate from prefab exactly, but rather just created a new component
            contr.Initialize(room.roomSpecificElevatorAnimController);

            return contr;
        }else{
            return null;
        }
    }

    public void PressedButton() {
        if(_isIdle && !_hasPressedButton && (GameManager.instance.CurrentRoomManager == null || GameManager.instance.CurrentRoomManager.State == BaseRoomManager.RoomState.Complete)) {
            _hasPressedButton = true;
        }
    }

    #endregion METHODS
}