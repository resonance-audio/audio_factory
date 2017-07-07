// Copyright 2017 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

Shader "Custom/BasementCompletionWalls" {
	Properties {
		_Start ("Start", Color) = (1,1,1,1)
		_End ("End", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Mod ("Mod", Float) = 0.1
		_ClipMod ("ClipMod", Float) = 0.1
		_TimeScale ("Time Scale", Float) = 1
		_TimeOffset ("Time Offset", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		Cull Off
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows nofog

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		float _Mod;
		float _ClipMod;
		float _TimeOffset;
		float _TimeScale;
		fixed4 _Start;
		fixed4 _End;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			half value = (fmod(_Time.x+_TimeOffset,1.0)) * _TimeScale;
			half pos = IN.uv_MainTex.x + (value - fmod(value, _Mod));
			half clipPos = fmod(IN.uv_MainTex.x, _ClipMod);
			clip(clipPos - _ClipMod/2);
			half frac = pos - fmod(pos, _Mod);
			frac = fmod(abs(frac-0.001),1);
			fixed4 c = lerp(_Start, _End, frac);
			o.Albedo = half4(1,1,1,1);
			o.Emission = c.rgb;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
