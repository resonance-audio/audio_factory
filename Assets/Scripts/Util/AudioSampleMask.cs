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

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
#endif
using UnityEngine;

//Essentially a float-array wrapper. Used for pre-sampling from audio-clips.
public class AudioSampleMask : ScriptableObject {

    //TODO: make this configurable via an editor-dialog of some sort
    const int SAMPLE_STEP_SIZE = 1000;

    [SerializeField, ReadOnly] public AudioClip clip;
    [SerializeField, ReadOnly] public float[] values;

#if UNITY_EDITOR
    [MenuItem("Assets/Create/AudioEngineer/Audio Sample Mask", true)]
    public static bool CreateAudioSampleMaskValidate() {
        return Selection.objects != null && Selection.objects.Any(obj => (obj is AudioClip));
    }
    [MenuItem("Assets/Create/AudioEngineer/Audio Sample Mask", false)]
    public static void CreateAudioSampleMask() {
        if(Selection.objects != null && Selection.objects.Any(obj => (obj is AudioClip))) {

            var clips = Selection.objects
                .Where(obj => obj is AudioClip)
                .Select(clip => (AudioClip)clip);

            foreach(var clip in clips) {
                AudioSampleMask newMask = ScriptableObject.CreateInstance<AudioSampleMask>();
                newMask.clip = clip;
                newMask.values = AudioSampler.LoadSamples(clip, SAMPLE_STEP_SIZE);
                
                string path = AssetDatabase.GetAssetPath(clip).Split('.')[0];
                AssetDatabase.CreateAsset(newMask, path + "_Mask.asset");
            }
        }
    }
    [MenuItem("Assets/Create/AudioEngineer/Regenerate Audio Sample Mask", true)]
    public static bool RegenerateAudioSampleMaskValidate() {
        return Selection.objects != null && Selection.objects.Any(obj => (obj is AudioSampleMask));
    }
    
    [MenuItem("Assets/Create/AudioEngineer/Regenerate Audio Sample Mask", false)]
    public static void RegenerateAudioSampleMask() {
        if(Selection.objects != null && Selection.objects.Any(obj => (obj is AudioSampleMask))) {

            var masks = Selection.objects
                .Where(obj => obj is AudioSampleMask)
                .Select(mask => (AudioSampleMask)mask);

            foreach(var mask in masks) {
                if(mask != null && mask.clip != null) {
                    mask.values = AudioSampler.LoadSamples(mask.clip, SAMPLE_STEP_SIZE);
                }
            }
        }
    }
#endif
}