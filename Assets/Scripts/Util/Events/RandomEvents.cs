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
using System.Collections;
using System;
using UnityEngine.Events;

namespace Util.Events {
	[Serializable]
	public class RandomEvents : MonoBehaviour {

		[Serializable]
		public struct Event {

			[SerializeField, Range(0f, 1f)] 
			public float weight;

			[SerializeField] 
			public UnityEvent unityEvent;
		}

		[SerializeField] public Event[] events;

		public void InvokeRandom() {
			if(events == null || events.Length == 0) return;
			
			float total = 0f;
			for(int i=0; i<events.Length; ++i) {
				total += events[i].weight;
			}

			float rand = UnityEngine.Random.Range(0f, total);
			float curr = 0f;
			for(int i=0; i<events.Length-1; ++i) {
				curr += events[i].weight;
				if(rand <= curr) {
					events[i].unityEvent.Invoke();
					return;
				}
			}

			events[events.Length-1].unityEvent.Invoke();
		}
	}
}