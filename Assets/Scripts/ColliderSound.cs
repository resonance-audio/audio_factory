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

public class ColliderSound : MonoBehaviour {

    [SerializeField] GvrAudioSource source = null;
    [SerializeField] ColliderSoundClip[] clips = null;
    [SerializeField] Collider[] specificTargets = null;
    [SerializeField] float minimumDelayBetweenNeighbors = 0f;

    [NonSerialized] List<ColliderSoundClip> _clipCache = new List<ColliderSoundClip>();
    [NonSerialized] Vector3? _lastPosition = null;
    [NonSerialized] float _velocity = 0f;
    [NonSerialized] int? _neighborId = null;

    static Dictionary<int, float> _playTimes = new Dictionary<int, float>();

    [Serializable]
    public struct ColliderSoundClip {
        [SerializeField] public AudioClip clip;
        [SerializeField] public float velocityMin;
        [SerializeField] public float velocityMax;
        [SerializeField] public float randomWeight;
        [SerializeField] public Vector2 volumeRange;
        [SerializeField] public Vector2 pitchRange;

        public bool IsValid {
            get {
                return clip != null && velocityMin < velocityMax && randomWeight > 0f;
            }
        }
    }

    int GetNeighborId() {
        int lowest = int.MaxValue;
        foreach(var neighbor in transform.parent.GetComponentsInChildren<ColliderSound>()) {
            if(neighbor != null) {
                int id = neighbor.GetInstanceID();
                if(id < lowest) {
                    lowest = id;
                }
            }
        }
        return lowest;
    }

    void Update() {
        _neighborId = _neighborId ?? GetNeighborId();
        
        if(!_lastPosition.HasValue) {
            _lastPosition = transform.position;
        }

        _velocity = (transform.position - _lastPosition.Value).magnitude;
        _lastPosition = transform.position;
    }

    ColliderSoundClip GetRandomSoundClip(float velocity) {
        _clipCache.Clear();
        _clipCache.AddRange(clips);

        float totalWeight = 0f;
        for(int c=_clipCache.Count-1; c>=0; --c) {
            var clip = _clipCache[c];
            if(!clip.IsValid || clip.velocityMin > velocity || clip.velocityMax < velocity) {
                _clipCache.RemoveAt(c);
                continue;
            }else{
                totalWeight += _clipCache[c].randomWeight;
            }
        }
        _clipCache.Shuffle();

        float roll = UnityEngine.Random.Range(0f, totalWeight);
        for(int c=0; c<_clipCache.Count; ++c) {
            roll -= _clipCache[c].randomWeight;
            if(roll <= 0f) {
                return _clipCache[c];
            }
        }

        return _clipCache.Count > 0 ? _clipCache[0] : default(ColliderSoundClip);
    }

    bool ShouldPlay(int neighborId, float time) {
        _playTimes = _playTimes ?? new Dictionary<int,float>();
        if(!_playTimes.ContainsKey(neighborId)) {
            _playTimes[neighborId] = time;
            return true;
        }else{
            float timeSinceLastPlayed = time - _playTimes[neighborId];
            if(timeSinceLastPlayed >= minimumDelayBetweenNeighbors) {
                _playTimes[neighborId] = time;
                return true;
            }else{
                return false;
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        //null other collider; does this ever happen?
        if(other == null) return;
        //no source to play; no point in this function
        if(source == null) return;

        if(specificTargets != null && specificTargets.Length > 0) {
            bool good = false;
            for(int s=0; s<specificTargets.Length; ++s) {
                if(specificTargets[s] == other) {
                    good = true;
                    break;
                }
            }

            if(!good) return;
        }

        _neighborId = _neighborId ?? GetNeighborId();
        if(ShouldPlay(_neighborId.Value, Time.realtimeSinceStartup)) {
            var sound = GetRandomSoundClip(_velocity);
            if(sound.IsValid) {
                float volume = (_velocity - sound.velocityMin) / (sound.velocityMax - sound.velocityMin);
                volume *= UnityEngine.Random.Range(sound.volumeRange.x, sound.volumeRange.y);
                float pitch = UnityEngine.Random.Range(sound.pitchRange.x, sound.pitchRange.y);
                source.pitch = pitch;
                source.PlayOneShot(sound.clip, volume);
            }
        }

    }
}