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

//Provides facilities for moving a transform along a SplinePath/Rope
[ExecuteInEditMode]
public class SplineMover : MonoBehaviour {
    [SerializeField] public BaseSplinePath path;
    [SerializeField] public float motionSpeed = 1f;
    [SerializeField] public AnimationCurve motion = new AnimationCurve();
    [SerializeField] public bool reverse = false;
    [SerializeField] public Vector2 bounds = new Vector2(0f, 999999f);
    [SerializeField] public Mode mode = Mode.Auto;
    [SerializeField] public float _time = 0f;
    [SerializeField] public bool loop = true;

    public enum Mode {
        Auto   = 0,
        Manual = 1,
    }

    void OnEnable() {
        _time = Mathf.Clamp(0f, bounds.x, bounds.y);
    }

    void Update() {
        if(mode == Mode.Auto) {
            if(Application.isPlaying) {
                _time += Time.deltaTime * motionSpeed;
            }else{
                _time = 0f;
            }
        }

        _time = Mathf.Clamp(_time, bounds.x, bounds.y);

        if(path != null && motion != null) {
            float range = bounds.y - bounds.x;

            float adjustedTime = loop
               ? (bounds.x + (_time - bounds.x)%range)%1f
               : Mathf.Clamp(_time, bounds.x, bounds.y);

            float eval = motion.Evaluate(adjustedTime);
            if(reverse) eval = 1f - eval;
            transform.position = path.Evaluate(eval);
        }
    }

    void OnValidate() {
        if(motion == null) {
            motion = new AnimationCurve();
        }

        if(motion.keys == null || motion.keys.Length < 2) {
            Keyframe[] keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(0f, 1f);
        }
    }
}