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

//Wrapper struct around some room-specific lighting/fog fields, with some helper methods for lerping between them.
// Creation-methods will acquire lighting/fog settings from {RenderSettings} and {BaseRoomManager.instance}
[Serializable]
public struct RoomLightingSettings {
    
    //---------------------------------------------------------------------
    #region SERIALIZED_FIELDS
        [SerializeField] public bool fogEnabled;
        [SerializeField] public FogMode fogMode;
        [SerializeField] public Color fogColor;
        [SerializeField] public float fogDensity;
        [SerializeField] public float fogStartDistance;
        [SerializeField] public float fogEndDistance;
        [SerializeField] public Color cameraClearColor;
        [SerializeField] public float pedestalBlend;
    #endregion SERIALIZED_FIELDS
    
    
    //---------------------------------------------------------------------
    #region STATIC_METHODS
        public override string ToString() {
            return string.Format("({0},{1},{2},{3},{4},{5},{6},{7})", fogEnabled, fogMode, fogColor, fogDensity, fogStartDistance, fogEndDistance, cameraClearColor, pedestalBlend);
        }

        //Create a new RoomLightingSettings struct with the current room's lighting settings
        public static RoomLightingSettings CreateFromCurrentRoom() {
            var roomMan = BaseRoomManager.instance;
            return new RoomLightingSettings() {
                fogEnabled       = RenderSettings.fog,
                fogMode          = RenderSettings.fogMode,
                fogColor         = RenderSettings.fogColor,
                cameraClearColor = roomMan != null ? roomMan.cameraClearColor : Color.black,
                fogDensity       = RenderSettings.fogDensity,
                fogStartDistance = RenderSettings.fogStartDistance,
                fogEndDistance   = RenderSettings.fogEndDistance,
            };
        }

        //Create a new RoomLightingSettings which is lerped between the two specified RoomLightingSettings, by a 0-1 factor
        public static RoomLightingSettings Lerp(RoomLightingSettings start, RoomLightingSettings end, float lerp) {
            lerp = Mathf.Clamp(lerp, 0f, 1f);

            return new RoomLightingSettings() {
                //no way to lerp this value, just snap when we're half-way
                fogEnabled          = lerp < 0.5f ? start.fogEnabled : end.fogEnabled,
                //no way to lerp this value, just snap when we're half-way
                fogMode             = lerp < 0.5f ? start.fogMode : end.fogMode,
                fogColor            = Color.Lerp(start.fogColor, end.fogColor, lerp),
                cameraClearColor    = Color.Lerp(start.cameraClearColor, end.cameraClearColor, lerp),
                fogDensity          = Mathf.Lerp(start.fogDensity, end.fogDensity, lerp),
                fogStartDistance    = Mathf.Lerp(start.fogDensity, end.fogDensity, lerp),
                fogEndDistance      = Mathf.Lerp(start.fogEndDistance, end.fogEndDistance, lerp),
                pedestalBlend       = lerp
            };
        }

        //Hand off a RoomLightingSettings to set all of the corresponding RenderSettings fields with
        public static void Set(RoomLightingSettings settings, Material blendMaterial = null) {
            RenderSettings.fog              = settings.fogEnabled;
            RenderSettings.fogMode          = settings.fogMode;
            RenderSettings.fogColor         = settings.fogColor;
            RenderSettings.fogDensity       = settings.fogDensity;
            RenderSettings.fogStartDistance = settings.fogStartDistance;
            RenderSettings.fogEndDistance   = settings.fogEndDistance;

            if(Camera.main != null) {
                Camera.main.backgroundColor = settings.cameraClearColor;
            }
        
            if(blendMaterial != null) {
                blendMaterial.SetFloat("_TexBlend", settings.pedestalBlend);
            }
        }
    #endregion STATIC_METHODS
}