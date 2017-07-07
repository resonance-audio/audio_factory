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

Shader "Custom/ElevatorDisplayShader_2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Multiply ("Multiply", 2D) = "white" {}
		_TextureMultiplier ("Texture Multiplier", Vector) = (1,1,1,1)
		_Mod ("Modulo", float) = 0
		_Clip ("Edge clip", float) = 0.001
		_Color ("Color", Color) = (1,1,1,1)
		_TimeScale ("Time Scale", float) = 10.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
			sampler2D _Multiply;
			float4 _MainTex_ST;
			half4 _TextureMultiplier;
			float _Mod;
			float _Clip;
			float4 _Color;
			half _TimeScale;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float time = _Time.x * _TimeScale;
				float2 uv = i.uv + float2(time.x-time.x%_Mod, 0.0);
				float2 mulUv = (uv%_Mod)/float2(1*_Mod, 1*_Mod);
				clip(mulUv.x - _Clip);
				clip(mulUv.y - _Clip);
				clip(1.0 - mulUv.x - _Clip);
				clip(1.0 - mulUv.y - _Clip); 
				float mul = 0.9 + 0.2 * sin((uv.y)*100 + _Time.x*25);
				fixed4 col = mul * tex2D(_MainTex, uv-uv%_Mod) * tex2D(_Multiply, mulUv) * _Color * _TextureMultiplier;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				//clip(col.r+col.g+col.b-0.1);
				return col;
			}
			ENDCG
		}
	}
}
