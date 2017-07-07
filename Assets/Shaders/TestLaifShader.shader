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

Shader "Custom/TestLaifShader"
{
	Properties
	{
		//This shader combines up to 3 sin-wave offsets to vertices, one for each color channel

		//----------------------------------------------------------------
		//WAVE RELATED FIELDS
		//Multiplies the wave values via texture sampling
		_WaveMultiplierTex ("Wave Multiplier Texture", 2D) = "white" {}
		//Overall magnitude of the effect
		_WaveMultiplier ("Wave Magnitude", Vector) = (1,1,1,1)
		//Amount which we multiply the built-in _Time variable that is fed into sin()
		_WaveTimeScale ("Wave Time Scale", Vector) = (1,1,1,1)
		//The relative period/frequency of each wave
		_WavePeriod ("Wave Period", Vector) = (1,1,1,1)
		//the direction along the surface which the RED wave moves over time
		_RedWaveDirection ("Red Wave Direction", Vector) = (0,0,1,0)
		//the direction along the surface which the BLUE wave moves over time
		_GreenWaveDirection ("Green Wave Direction", Vector) = (1,0,0,0)
		//the direction along the surface which the GREEN wave moves over time
		_BlueWaveDirection ("Blue Wave Direction", Vector) = (0,1,0,0)
		//the direction which the RED offset is applied
		_RedDirection ("Red Direction", Vector) = (1,0,0,0)
		//the direction which the GREEN offset is applied
		_GreenDirection ("Green Direction", Vector) = (0,1,0,0)
		//the direction which the BLUE offset is applied
		_BlueDirection ("Blue Direction", Vector) = (0,0,1,0)

		//----------------------------------------------------------------
		//LIGHTING RELATED FIELDS
		_MainTex ("Texture", 2D) = "white" {}
		_LightTex ("Light Texture", 2D) = "white" {}
		_EmissionColor ("Emission Color", Color) = (1,1,1,1)
		_BaseColor ("Base Color", Color) = (0,0,0,0)
		_LightColor ("Light Color", Color) = (1,1,1,1)
		_LightPosition ("Light Position", Float) = 0
		_Cutoff ("Cutoff", Float) = 0.5
	}
	SubShader
	{
		Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
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
				half4 normal : NORMAL;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 normal : NORMAL;
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

			half4 _RedDirection;
			half4 _GreenDirection;
			half4 _BlueDirection;

			half4 _RedWaveDirection;
			half4 _GreenWaveDirection;
			half4 _BlueWaveDirection;

			v2f vert (appdata v)
			{
				v2f o;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				half4 waveMultiply = _WaveMultiplier * tex2Dlod(_WaveMultiplierTex, half4(o.uv.x, o.uv.y, 0, 0));
				half4 redMod = sin(dot(v.vertex, _RedWaveDirection)*_WavePeriod.r + _Time.x*_WaveTimeScale.r)* waveMultiply.r * _RedDirection;
				half4 greenMod = sin(dot(v.vertex, _GreenWaveDirection)*_WavePeriod.g + _Time.x*_WaveTimeScale.g)* waveMultiply.g * _GreenDirection;
				half4 blueMod = sin(dot(v.vertex, _BlueWaveDirection)*_WavePeriod.b + _Time.x*_WaveTimeScale.b)* waveMultiply.b * _BlueDirection;
				o.vertex = UnityObjectToClipPos(v.vertex + v.normal * (redMod + greenMod + blueMod));
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				clip(col.a - _Cutoff);
				fixed a = (col.a - _Cutoff) / (1.0 - _Cutoff);
				fixed4 light = tex2D(_LightTex, i.uv);
				light = fixed4(clamp(1.0 - abs(light.r - _LightPosition), 0, 1), 
							   clamp(1.0 - abs(light.g - _LightPosition), 0, 1),
							   clamp(1.0 - abs(light.b - _LightPosition), 0, 1),
							   light.a);

				col = _BaseColor + (lerp(lerp(col, _BaseColor, half4(1,1,1,1)-col), half4(1,1,1,1), _EmissionColor.a)  * _EmissionColor) + (light * _LightColor * _LightColor.a);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				col.a = a;
				return col;
			}
			ENDCG
		}
	}
}
