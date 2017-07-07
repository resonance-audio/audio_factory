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
using UnityEngine.Events;
using System;

//Component which wraps a UnityEvent, along with facilities for invoking it on various behaviour messages
namespace Util.Events {
	public class EventInvoker : MonoBehaviour {

#region SERIALIZED_FIELDS
		[SerializeField] public InvokationType invokationType = InvokationType.OnEnable;
		[Tooltip("The delay in milliseconds before the event is invoked. Only valid for OnEnable(), Awake(), and Start()")]
		[SerializeField] int delay = 0;
		[SerializeField] UnityEvent _event = default(UnityEvent);
#endregion SERIALIZED_FIELDS

#region INNER_DEFINITIONS
		//On which message(s) to invoke the contained event
		[Flags]
		public enum InvokationType {
			None       = 0,
			OnEnable   = 1,
			Awake      = 2,
			Start      = 4,
			Update     = 8,
			LateUpdate = 16,
			OnDisable  = 32,
		}
		//Whether the specified invokation type allowed for delayed-invokes. IE: it doesn't make sense to delay an invoke on Update/LateUpdate
		const InvokationType DELAYED_INVOKE_ALLOWED =
			InvokationType.OnEnable
		| InvokationType.Awake
		| InvokationType.Start;
#endregion INNER_DEFINITIONS

#region METHODS
		void OnEnable() {
			if(ShouldInvoke(InvokationType.OnEnable)) Invoke();
		}

		void Awake() {
			if(ShouldInvoke(InvokationType.Awake)) Invoke();
		}

		void Start() {
			if(ShouldInvoke(InvokationType.Start)) Invoke();
		}

		void Update() {
			if(ShouldInvoke(InvokationType.Update)) Invoke();
		}

		void LateUpdate() {
			if(ShouldInvoke(InvokationType.LateUpdate)) Invoke();
		}

		void OnDisable() {
			if(ShouldInvoke(InvokationType.OnDisable)) Invoke();
		}

		bool ShouldInvoke(InvokationType type) {
			return (type & this.invokationType) != InvokationType.None;
		}

		//to avoid ambiguity when hooked up via animations/events, since Invoke is such a common method name
		void _EventInvoker_Invoke() {
			Invoke();
		}
		
		void Invoke() {
			if(_event != null) {
				if(delay > 0) {
					if((invokationType & DELAYED_INVOKE_ALLOWED) == InvokationType.None) {
						throw new InvalidOperationException("Delayed invoke is not allowed for " + invokationType.ToString() + " invokation type");
					}else{
						StartCoroutine(_Delayed_Invoke(delay));
					}
				}else{
					if(_event != null) _event.Invoke();
				}
			}
		}

		IEnumerator _Delayed_Invoke(int delay) {
			yield return new WaitForSeconds(1000f * (float)delay);
			if(_event != null) _event.Invoke();
		}
#endregion METHODS

	}
}