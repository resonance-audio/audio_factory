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

Shader "Custom/PedestalShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "white" {}
		_NextTex ("Next Texture", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
		_TexBlend ("Texture Blend", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_EmissionColor ("Emission Color", Color) = (0,0,0,0)
		_Multiply ("Multiply", Vector) = (1,1,1,1)

		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      	_RimPower ("Rim Power", Range(0.0,4.0)) = 1.5
		_RimCutoff ("Rim Cutoff", Range(0.5, 1.0)) = 1
		_ClipCutoff ("Clip Cutoff", Range(0.0, 1.0)) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NextTex;
		sampler2D _BumpMap;

		struct Input {
			float2 uv_MainTex;
			float2 uv_NextTex;
			float3 viewDir;
			float2 uv_BumpMap;
		};

		half _TexBlend;
		half _Metallic;
		fixed4 _Color;
		fixed4 _EmissionColor;
		fixed4 _Multiply;

      	float4 _RimColor;
      	float _RimPower;
		half _RimCutoff;
		half _ClipCutoff;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 mainTex = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 nextTex = tex2D (_NextTex, IN.uv_NextTex);
			fixed4 texColor = lerp(mainTex, nextTex, _TexBlend) * _Color;

			o.Albedo = texColor.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;

			//rim effect
			o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
          	half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
			//half dif = abs(_RimCutoff - abs(rim));
			//rim = _RimCutoff - pow(dif, abs(_RimCutoff-_RimCutoffPower));
			clip(_ClipCutoff - rim);
			rim = _RimCutoff - abs(rim - _RimCutoff);

          	o.Emission = _Multiply * _RimColor.rgb * pow (rim, _RimPower*_RimCutoff) + _EmissionColor * texColor.rgb;

			o.Alpha = texColor.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
