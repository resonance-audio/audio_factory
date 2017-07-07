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

Shader "Custom/FinaleLaifShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_WaveMultiplierTex ("Wave Multiplier Texture", 2D) = "white" {}
		_LightTex ("Light Texture", 2D) = "white" {}
		_EmissionColor ("Emission Color", Color) = (1,1,1,1)
		_BaseColor ("Base Color", Color) = (0,0,0,0)
		_LightColor ("Light Color", Color) = (1,1,1,1)
		_WaveMultiplier ("Wave Multiplier", Vector) = (1,1,1,1)
		_WaveTimeScale ("Wave Time Scale", Vector) = (1,1,1,1)
		_WavePeriod ("Wave Period", Vector) = (1,1,1,1)
		_LightPosition ("Light Position", Float) = 0
		_Cutoff ("Cutoff", Float) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _WaveMultiplierTex;
			sampler2D _LightTex;
			float4 _MainTex_ST;
			half4 _LightColor;
			half4 _WaveMultiplier;
			half4 _WaveTimeScale;
			half4 _WavePeriod;
			half4 _EmissionColor;
			half4 _BaseColor;
			half _LightPosition;
			fixed _Cutoff;

			v2f vert (appdata v)
			{
				v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				half4 vCoords = half4(v.vertex.z, v.vertex.x, v.vertex.y, 0.0);
				half4 waveMultiply = _WaveMultiplier * tex2Dlod(_WaveMultiplierTex, half4(o.uv.x, o.uv.y, 0, 0));
				half4 modified = v.vertex + sin(vCoords*_WavePeriod + _Time.x*_WaveTimeScale)*waveMultiply;
				o.vertex = UnityObjectToClipPos(modified);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 light = tex2D(_LightTex, i.uv);
				light = fixed4(clamp(1.0 - abs(light.r - _LightPosition), 0, 1), 
							   clamp(1.0 - abs(light.g - _LightPosition), 0, 1),
							   clamp(1.0 - abs(light.b - _LightPosition), 0, 1),
							   light.a);

				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				if (col.a < _Cutoff){
					discard;
				}
				return _BaseColor + (col * _EmissionColor) + (light * _LightColor * _LightColor.a);
			}
			ENDCG
		}
	}
}
