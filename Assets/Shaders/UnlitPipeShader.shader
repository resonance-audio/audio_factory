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

Shader "Custom/UnlitPipeShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_EmissionColor ("Emission Color", Color) = (0,0,0,0)
		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
      	_RimPower ("Rim Power", Range(0.25,4.0)) = 1.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull Back
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float3 viewDir : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
			};

			fixed4 _Color;
			fixed4 _EmissionColor;
			fixed4 _RimColor;
			fixed _RimPower;

			v2f vert (appdata v)
			{
				v2f o;
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
				o.normal = v.normal;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half rim = 1.0 - dot (normalize(i.viewDir), normalize(i.normal));
				float3 col = _RimColor.rgb * pow(rim, _RimPower) + _EmissionColor * _Color.rgb;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return half4(col, _Color.a);
			}
			ENDCG
		}
	}
	FallBack "Color"
}