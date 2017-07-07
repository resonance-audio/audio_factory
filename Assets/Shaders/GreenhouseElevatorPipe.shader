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

Shader "Unlit/GreenhouseElevatorPipe"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BaseTex ("Base Texture", 2D) = "white" {}
		_Color ("Color", Color) = (0.0,0.0,0.0,1.0)

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
		        float2 uv2 : TEXCOORD1;
		    };
		 
		    struct v2f
		    {
		        float2 uv : TEXCOORD0;
		        float2 uv2 : TEXCOORD2;
		        UNITY_FOG_COORDS(1)
		        float4 vertex : SV_POSITION;
		    };


			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BaseTex;
			float4 _BaseTex_ST;
			float4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv2, _BaseTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 gradient = tex2D(_MainTex, i.uv);
				fixed4 base = tex2D(_BaseTex, i.uv2);

				fixed4 col = lerp(gradient * _Color, base, 1 - gradient.r);

				UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}
			ENDCG
		}
	}
}
