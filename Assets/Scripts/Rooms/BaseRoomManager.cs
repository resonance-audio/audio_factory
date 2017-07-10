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
using UnityEngine.Events;

//Base class for each room's main manager object, which determines the room's completion status
public abstract class BaseRoomManager : MonoBehaviour {

    //-----------------------------------------------------------------------------------------
    //Elevator planes
    [TooltipAttribute("The very bottom of the elevator shaft, connected to the previous room")]
    [SerializeField] public ElevatorPlane bottomPlane = null;
    [TooltipAttribute("The floor plane of the current room")]
    [SerializeField] public ElevatorPlane floorPlane = null;
    [TooltipAttribute("The ceiling plane of the current room")]
    [SerializeField] public ElevatorPlane ceilingPlane = null;
    [TooltipAttribute("The very top of the elevator shaft, connected to the next room")]
    [SerializeField] public ElevatorPlane topPlane = null;
    //-----------------------------------------------------------------------------------------

    [SerializeField] public Color cameraClearColor = Color.white;
    [SerializeField] public Texture pedestalTexture = null;
    [SerializeField] public Texture descriptionTexture = null;
    [SerializeField] public UnityEvent onInRoom = null;


    public static BaseRoomManager instance { get; protected set; }

    protected Elevator elevator;
    protected ElevatorVOSequencer VOSequencer;
    protected bool inRoom;


    public enum RoomState {
        Incomplete = 0, //the room is incomplete; the elevator should stop and wait for completion
        Complete   = 1, //the room is complete, elevator is either moving, or waiting for user input
        Skip       = 2, //the room is some aesthetic-only room, a shaft, etc. Elevator passes through without stopping
    }

    public abstract RoomState State { get; } 

    protected virtual void Awake() {

        //create the game manager object if we need to
        GameManager.Init(transform.position);
        instance = this;
        elevator = GameObject.Find("P_Elevator(Clone)").GetComponent<Elevator>();
        VOSequencer = GetComponent<ElevatorVOSequencer>();
    }

    protected virtual void InRoom() {
        if(onInRoom != null) {
            onInRoom.Invoke();
        }
    }

    protected virtual void Update() {
        if (!inRoom) {
            if (!elevator.IsMoving && Mathf.Abs(elevator.transform.position.y - floorPlane.transform.position.y) < elevator.roundingThreshold) {
                inRoom = true;
                InRoom();
            }
        } 
    }
}