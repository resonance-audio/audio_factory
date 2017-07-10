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
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class AudioSampler : MonoBehaviour {

    const int MAX_DEBUG_CURVE_KEYS = 1000;

    //-----------------------------------------------------------------------
    #region SERIALIZED_FIELDS
        [SerializeField] GvrAudioSource source = null;
        //Controls the accuracy of the cached/compressed samples; lower value is more precise,
        //   but a higher value will reduce noise in the samples
        [SerializeField, RangeAttribute(10, 5000)] int stepSize = 1000;
        //Offset in seconds from the source's position to read from
        [SerializeField] float fixedTimeOffset = 0f;
        //Value to scale the samples by
        [SerializeField] float multiplier = 1f;
        //Whether to interpolate between read values, or just read at the current position
        [SerializeField] InterpolationMode interpolationMode = InterpolationMode.Linear;
        //Events to invoke when entering/leaving certain different sample ranges
        [SerializeField] ThresholdEvents[] thresholdEvents = null;
        //Debug curves to show what the cached samples look like
        [SerializeField, ReadOnly] List<AnimationCurve> _debugCurves;
        //Pre-calculated sample values so that we can bypass the caching step at runtime
        [SerializeField] List<AudioSampleMask> sampleMasks = new List<AudioSampleMask>();
    #endregion SERIALIZED_FIELDS

    //-----------------------------------------------------------------------
    #region NON_SERIALIZED_FIELDS
        //internal buffer for loading the clip's samples into
        [NonSerialized] float[] samples = null;
        //internal buffer used for loading each step of the clip's samples
        [NonSerialized] float[] stepBuffer = null;
        //if any of the events require calculating the values of future frames
        [NonSerialized] float[] eventsNextFramesBuffer = null;
        [NonSerialized] AudioClip clip = null;
        //the average as calculated since the previous frame
        [NonSerialized] float _lastCalculatedAverage = 0f;
        //the compressed samples loaded for each clip, mapped by its instance id
        [NonSerialized] Dictionary<int, float[]> _cachedSamples = new Dictionary<int, float[]>();
    #endregion NON_SERIALIZED_FIELDS

    //-----------------------------------------------------------------------
    #region INNER_DEFINITIONS
        [Serializable]
        public enum InterpolationMode {
            None,
            Linear,
        }

        [Serializable]
        public class ThresholdEvents {
            [Serializable]
            public enum InvokeMode {
                //invoke once each time upon entering this threshold
                OnEnterThreshold = 0,
                //invoke every frame that the value is within this threshold
                EveryFrameWithinThreshold = 1,
            }
            
            [SerializeField] InvokeMode mode;
            //start-value which will invoke this event
            [SerializeField] float start = 0f;
            //end-value which will invoke this event
            [SerializeField] float end = 1f;
            [SerializeField] float minimumDelay = 0.01f;
            //how many consecutive frames this event requires the value to be inside its range
            //  before the 'inside' event gets invoked
            [SerializeField, RangeAttribute(1,10)] public int readsPerFrame = 1;
            //event invoked if the current sample is within this threshold
            [SerializeField, FormerlySerializedAs("_event")] UnityEvent _onInside = null;
            //event invoked if the current sample is within this threshold
            [SerializeField] UnityEvent _onOutside = null;

            [NonSerialized] bool _previouslyInside = false;
            [NonSerialized] float _timeLastInside = 0f;
            [NonSerialized] float _timeLastOutside = 0f;

            public void Reset() {
                _previouslyInside = false;
                Update(0f);
                _timeLastInside = 0f;
                _timeLastOutside = 0f;
            }

            public bool Within(float value) {
                return value >= start && value <= end;
            }

            public bool WithinNextValues(float[] nextFramesValues) {
                if(nextFramesValues == null) return false;
                for(int v=0; v<nextFramesValues.Length && v<(readsPerFrame-1); ++v)
                    if(!Within(nextFramesValues[v])) return false;
                return true;
            }

            public void Update(float value, float[] nextFramesValues = null) {

                bool inside = Within(value);
                if(nextFramesValues != null) {
                    inside &= WithinNextValues(nextFramesValues);
                }
                float time = Time.timeSinceLevelLoad;

                if(inside && (time - _timeLastInside >= minimumDelay)) {
                    if(_onInside != null) {
                        switch(mode) {
                            default: case InvokeMode.OnEnterThreshold:
                                if(!_previouslyInside) {
                                    _timeLastInside = time;
                                    _onInside.Invoke();
                                }
                                break;
                            case InvokeMode.EveryFrameWithinThreshold:
                                _timeLastInside = time;
                                _onInside.Invoke();
                                break;
                        }
                    }
                }else if(!inside && (time - _timeLastOutside >= minimumDelay)) {
                    if(_onOutside != null) {
                        switch(mode) {
                            default: case InvokeMode.OnEnterThreshold:
                                if(_previouslyInside) {
                                    _timeLastOutside = time;
                                    _onOutside.Invoke();
                                }
                                break;

                            case InvokeMode.EveryFrameWithinThreshold:
                                _timeLastOutside = time;
                                _onOutside.Invoke();
                                break;
                        }
                    }
                }

                _previouslyInside = inside;
            }
        }
    #endregion INNER_DEFINITIONS

    //-----------------------------------------------------------------------
    #region METHODS

        public float CurrentSampleAverage {
            get {
                return _lastCalculatedAverage;
            }
        }

        public static float[] LoadSamples(AudioClip clip, int stepSize) {
            if(clip == null) throw new ArgumentNullException("clip");

            float[] stepBuffer = new float[stepSize];
            return LoadSamples(clip, stepSize, stepBuffer);
        }

        public static float[] LoadSamples(AudioClip clip, int stepSize, float[] stepBuffer) {
            if(clip == null) throw new ArgumentNullException("clip");

            int stepCount = clip.samples / stepSize;
            float[] newSamples = new float[stepCount];

            if(stepBuffer == null || stepBuffer.Length != stepSize)
                stepBuffer = new float[stepSize];
                
            for(int step=0; step<stepCount; ++step) {
                int stepOffset = stepSize * step;
                clip.GetData(stepBuffer, stepOffset);

                //get the average value (taking abs values) for this step
                float bufferAverage = 0f;
                for(int i=0; i<stepSize; ++i) {
                    bufferAverage += Mathf.Abs(stepBuffer[i]);
                }
                bufferAverage /= (float)stepSize;
                newSamples[step] = bufferAverage;
            }

            return newSamples;
        }

        float _Read(float time) {
            if(source == null || source.clip == null) return 0f;
            if(samples == null || samples.Length == 0) return 0f;

            return _Read(time, source.clip.length, samples);
        }

        //Get the sample value at the specified time; accuracy will depend on this object's stepSize
        float _Read(float time, float clipTime, float[] clipSamples)
        {
            if(clipSamples == null || clipSamples.Length == 0) return 0f;

            float normalizedTime = (time / clipTime) % 1f;
            float stepTime = 1f / stepSize;

            int startPos = (int)Mathf.Floor((float)clipSamples.Length * normalizedTime);

            //the lower-bound sample
            float firstValue = clipSamples[startPos];
            switch(interpolationMode) {

                //no interpolation
                default: case InterpolationMode.None:
                return firstValue * multiplier;

                //linear interpolation
                case InterpolationMode.Linear:
                //the fractions which the specified time is between our cached samples
                float remainder = normalizedTime % stepTime;
                float normalizedRemainder = remainder / stepTime;
                float normalizedEndTime = ((time+stepTime) / clipTime) % 1f;
                int endPos   = (int)Mathf.Floor((float)clipSamples.Length * normalizedEndTime);
                //the upper-bound sample
                float secondValue = clipSamples[endPos];
                return Mathf.Lerp(firstValue, secondValue, normalizedRemainder) * multiplier;
            }
        }

        void Update() {
            if (source == null) return;

            if (!source.isPlaying) {
                _lastCalculatedAverage = 0f;
                if(thresholdEvents != null) {
                    for(int e=0; e<thresholdEvents.Length; ++e) {
                        thresholdEvents[e].Reset();
                    }
                }
                return;
            } else {
                AudioClip newClip = source.clip;
                if (newClip != clip) {
                    clip = newClip;

                    int clipId = clip.GetInstanceID();
                    if(!_cachedSamples.ContainsKey(clipId)) {

                        //if there is a mask referenced by this sampler which corresponds to the sound, use those samples instead
                        float[] newSamples = null;
                        if(sampleMasks != null) {
                            for(int m=0; m<sampleMasks.Count; ++m) {
                                if(sampleMasks[m].clip == clip) {
                                    newSamples = sampleMasks[m].values;
                                }
                            }
                        }
                        if(newSamples == null) {
                            UnityEngine.Debug.Log("Could not find mask for " + clip.name + ", " + gameObject.name);
                            newSamples = LoadSamples(clip, stepSize, stepBuffer);
                        }

                        _cachedSamples[clipId] = newSamples;

                      //------------------------------------------------------------------
                      //If we're in editor, generate animationCurves containing the samples for debugging
                      #if UNITY_EDITOR
                      #region DEBUG_ANIMATION_CURVES
                        AnimationCurve curve = new AnimationCurve();
                        Keyframe[] keys;
                        if(newSamples.Length < MAX_DEBUG_CURVE_KEYS) {
                            keys = new Keyframe[newSamples.Length];
                            float keyLength = 1f / (float)keys.Length * clip.length;
                            for(int k=0; k<keys.Length; ++k) {
                                keys[k] = new Keyframe((float)k * keyLength, newSamples[k]);
                            }
                        }else{
                            keys = new Keyframe[MAX_DEBUG_CURVE_KEYS];
                            float keyLength = 1f / (float)keys.Length * clip.length;
                            float clipLength = clip.length;
                            for(int k=0; k<keys.Length; ++k) {
                                keys[k] = new Keyframe((float)k * keyLength, _Read((float)k * keyLength, clipLength,newSamples));
                            }
                        }
                        curve.keys = keys;
                        _debugCurves.Add(curve);
                      #endregion DEBUG_ANIMATION_CURVES
                      #endif
                    }

                    samples = _cachedSamples[clipId];
                }

                //set the last-calculated-average
                _lastCalculatedAverage = _Read(source.time + fixedTimeOffset);


                //update all threshold events
                if(thresholdEvents != null) {
                    //the maximum number of 
                    int readsPerFrame = 1;
                    for(int t=0; t<thresholdEvents.Length; ++t) {
                        if(thresholdEvents[t].readsPerFrame > readsPerFrame) {
                            readsPerFrame = thresholdEvents[t].readsPerFrame;
                        }
                    }
                    
                    if(eventsNextFramesBuffer == null || eventsNextFramesBuffer.Length != readsPerFrame) {
                        eventsNextFramesBuffer = new float[readsPerFrame];
                    }
                    for(int i=0; i<eventsNextFramesBuffer.Length; ++i) {
                        eventsNextFramesBuffer[i] = _Read(source.time + fixedTimeOffset + (Time.deltaTime * (float)(i+1)));
                    }
                    
                    for(int t=0; t<thresholdEvents.Length; ++t) {
                        if(thresholdEvents[t] != null) {
                            thresholdEvents[t].Update(_lastCalculatedAverage, eventsNextFramesBuffer);
                        }
                    }
                }
            }
        }
    #endregion METHODS
}