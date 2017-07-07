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

public interface IElevatorAnimatorHookups {
    void OnEnterState(Elevator.State state);
    void OnExitState(Elevator.State state);
}

public class ElevatorAnimator : StateMachineBehaviour {

    [SerializeField] public Elevator.State state;

	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        Elevator elevator = animator.GetComponent<Elevator>()
                         ?? animator.GetComponentInParent<Elevator>()
                         ?? animator.GetComponentInChildren<Elevator>();

        if(elevator != null && elevator is IElevatorAnimatorHookups) {
            ((IElevatorAnimatorHookups)elevator).OnEnterState(state);
        }
	}

	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateExit(animator, stateInfo, layerIndex);
        
        Elevator elevator = animator.GetComponent<Elevator>()
                         ?? animator.GetComponentInParent<Elevator>()
                         ?? animator.GetComponentInChildren<Elevator>();

        if(elevator != null && elevator is IElevatorAnimatorHookups) {
            ((IElevatorAnimatorHookups)elevator).OnExitState(state);
        }
	}

	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateUpdate(animator, stateInfo, layerIndex);
	}

	override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateMove(animator, stateInfo, layerIndex);
	}

	override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        base.OnStateIK(animator, stateInfo, layerIndex);
	}
}