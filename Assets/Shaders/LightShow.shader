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

Shader "Unlit/LightShow"
{
	Properties
	{

		_Color ("Color", Color) = (1,1,1,1)

		_Wave1 ("Wave 1 Texture", 2D) = "black" {}
		_Distance1 ("Wave Distance 1", Float) = 1

		_Wave2 ("Wave 2 Texture", 2D) = "black" {}
		_Distance2 ("Wave Distance 2", Float) = 1

		_Wave3 ("Wave 3 Texture", 2D) = "black" {}
		_Distance3 ("Wave Distance 3", Float) = 1

		_Wave4 ("Wave 3 Texture", 2D) = "black" {}
		_Distance4 ("Wave Distance 4", Float) = 1


		_Range ("Wave Range", Float) = 1

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
				float2 worldPos : TEXCOORD2;
			};

			float4 _Color;

			sampler2D _Wave1;
			float4 _Wave1_ST;

			sampler2D _Wave2;
			float4 _Wave2_ST;

			sampler2D _Wave3;
			float4 _Wave3_ST;

			sampler2D _Wave4;
			float4 _Wave4_ST;

			fixed _Distance1;
			fixed _Distance2;
			fixed _Distance3;
			fixed _Distance4;

			fixed _Range;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _Wave1);
				UNITY_TRANSFER_FOG(o,o.vertex);
				fixed4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldPos = float2(distance(worldPos, _WorldSpaceCameraPos), 0);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{

				float dist = i.worldPos.x;

				fixed4 waveColor1 = tex2D(_Wave1, float2((_Distance1 - dist) / _Range, 0.5f));
				fixed4 waveColor2 = tex2D(_Wave2, float2((_Distance2 - dist) / _Range, 0.5f));
				fixed4 waveColor3 = tex2D(_Wave3, float2((_Distance3 - dist) / _Range, 0.5f));
				fixed4 waveColor4 = tex2D(_Wave4, float2((_Distance4 - dist) / _Range, 0.5f));

				fixed4 waveColor = waveColor1 + waveColor2 + waveColor3 + waveColor4;

				fixed4 baseColor = _Color + waveColor;

				UNITY_APPLY_FOG(i.fogCoord, baseColor);

				return baseColor;
			}
			ENDCG
		}
	}
}
