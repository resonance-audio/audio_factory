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

﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ElevatorVOSequencer : MonoBehaviour {

    [System.Serializable]
    public class VOEvent {
        public AudioClip clip;
        public float delay;
        public UnityEvent callback;

        public string Name {
            get { return clip.name; }
        }

        bool active;
        public bool Active {
            get { return active; }
            set { active = value; }
        }

        float timer;
        public float Timer {
            get { return timer; }
            set { timer = value; }
        }
    }

    ElevatorSpeaker speaker;

    [HideInInspector]
    public List<VOEvent> VOEvents = new List<VOEvent>();

    void Start() {
        speaker = ElevatorSpeaker.instance;
    }

    void Update () {
        foreach(VOEvent voEvent in VOEvents) {
            if (voEvent.Active) {
                voEvent.Timer -= Time.deltaTime;
                if (voEvent.Timer < 0) FireEvent(voEvent);
            }
        }
	}

    void FireEvent(VOEvent voEvent) {
        voEvent.Active = false;
        speaker.Play(voEvent.clip);
        if (voEvent.callback != null)
            voEvent.callback.Invoke();
    }

    public void StopAllEvents() {
        foreach(VOEvent voEvent in VOEvents) {
            voEvent.Active = false;
            voEvent.Timer = voEvent.delay;
        }
    }

    public void AddEvent(string _name) {
        AddEvent(_name, null);
    }

    public void AddEvent(string _name, UnityEvent callback) {

        bool eventFound = false;
        foreach(VOEvent voEvent in VOEvents) {
            if (voEvent.Name.Equals(_name)) {
                voEvent.Timer = voEvent.delay;
                voEvent.Active = true;
                voEvent.callback = callback;
                eventFound = true;
            }
        }
        if (!eventFound) 
            Debug.Log("No Event called " + _name + " was found");
    }
        

    public void ReplaceEvent(string _name) {
        ReplaceEvent(_name, null);
    }

    public void ReplaceEvent(string _name, UnityEvent callback) {
        StopAllEvents();
        AddEvent(_name, callback);
    }


    public void StopEvent(string _name) {
        foreach(VOEvent voEvent in VOEvents) {
            if (voEvent.Name.Equals(_name)) {
                voEvent.Active = false;
                voEvent.Timer = voEvent.delay;
                if (voEvent.callback != null) 
                    voEvent.callback.RemoveAllListeners();
            }
        }
    }
}
