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

public class Oscilloscope : MonoBehaviour {

    const int SHADER_RESOLUTION = 100;
    const string SHADER_PROP_VALUES = "_Values";

    [SerializeField] Renderer target = null;
    [SerializeField] GvrAudioSource source = null;
    [SerializeField] int resolution = 20;
    [SerializeField] int accuracy = 1;
    [SerializeField] int fixedSampleOffset = 0;
    [SerializeField] float multiplier = 1f;
    [SerializeField] Light specialLight = null;
    [SerializeField] float specialLightStarting = 0f;
    [SerializeField] float specialLightMultiplier = 1f;
    [SerializeField] AnimationCurve specialLightCurve = null;
    [SerializeField] float emissiveRendererStarting = 0f;
    [SerializeField] float emissiveRendererMultiplier = 1f;
    [SerializeField] AnimationCurve emissiveRendererCurve = null;
    [SerializeField] Color emissiveColorStart = Color.black;
    [SerializeField] Color emissiveColorEnd = Color.white;
    [SerializeField] MaterialPropertySetter[] emissiveSetters = null;
    [SerializeField] Renderer onOnlyWhenPlaying;

    [NonSerialized] float[] values = new float[SHADER_RESOLUTION];
    [NonSerialized] AudioClip clip = null;
    [NonSerialized] float[] samples = null;
    [NonSerialized] float _lastCalculatedAverage = 0f;

    static float[] zeroArray = new float[SHADER_RESOLUTION];

    public float CurrentSampleAverage {
        get {
            return _lastCalculatedAverage;
        }
    }

    //a safe, cycling read of the current samples
    float _Read(int position) {
        if(samples == null || samples.Length == 0) return 0f;
        position = position%samples.Length;
        if(position < 0) position = samples.Length-position;
        position = Mathf.Clamp(position, 0, samples.Length);
        return samples[position];
    }

    float _Average(float[] values) {
        if(values == null || values.Length == 0) return 0f;
        float ret = 0f;
        for(int i=0; i<values.Length; ++i) ret += values[i];
        ret /= (float)values.Length;
        return ret;
    }

    void OnValidate() {
        resolution = Mathf.Max(resolution, 1);
        accuracy = Mathf.Clamp(accuracy, 1, resolution);

        specialLightCurve = specialLightCurve ?? new AnimationCurve();
        if(specialLightCurve.keys.Length < 2) {
            Keyframe[] keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(1f, 1f);
            specialLightCurve.keys = keys;
        }
        emissiveRendererCurve = emissiveRendererCurve ?? new AnimationCurve();
        if(emissiveRendererCurve.keys.Length < 2) {
            Keyframe[] keys = new Keyframe[2];
            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(1f, 1f);
            emissiveRendererCurve.keys = keys;
        }
    }

    void Update() {
        
        if(onOnlyWhenPlaying != null) {
            onOnlyWhenPlaying.enabled = source != null && source.isPlaying;
        }

        if(source != null) {
            if(!source.isPlaying) {

                if(target != null) {
                    target.sharedMaterial.SetFloatArray(SHADER_PROP_VALUES, zeroArray);
                }
                if(specialLight != null) {
                    specialLight.intensity = specialLightStarting;
                }
                if(emissiveSetters != null) {
                    foreach(var setter in emissiveSetters) {
                        setter.value_color = emissiveColorStart;
                    }
                }
                _lastCalculatedAverage = 0f;
                return;
            }else{

                AudioClip newClip = source.clip;
                if(newClip != clip) {
                    clip = newClip;
                    values = new float[SHADER_RESOLUTION];
                    samples = new float[clip.samples];
                    clip.GetData(samples, 0);
                }

                int start = source.timeSamples * clip.channels;
                accuracy = Mathf.Clamp(accuracy, 1, resolution);

                for(int i=0; i<values.Length; ++i) {
                    float average = 0f;
                    for(int v=0; v<resolution; v+=accuracy) {
                        average += Mathf.Abs(multiplier * _Read(fixedSampleOffset + start + i*resolution + v));
                    }
                    average /= (resolution/accuracy);
                    values[i] = average;
                }

                if(target != null) {
                    target.sharedMaterial.SetFloatArray(SHADER_PROP_VALUES, values);
                }

                _lastCalculatedAverage = _Average(values);

                if(specialLight != null) {
                    specialLight.intensity = specialLightStarting + (specialLightMultiplier * specialLightCurve.Evaluate(_lastCalculatedAverage));
                }
                if(emissiveSetters != null) {
                    foreach(var setter in emissiveSetters) {
                        float frac = emissiveRendererCurve.Evaluate(emissiveRendererMultiplier * _lastCalculatedAverage + emissiveRendererStarting);
                        setter.value_color = Color.Lerp(emissiveColorStart, emissiveColorEnd, Mathf.Clamp(frac, 0f, 1f));
                    }
                }
            }
        }
    }
}