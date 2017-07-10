// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

Shader "Custom/ElevatorDisplayShader" {
 Properties {
   _Color ("Color", Color) = (1,1,1,1)
   _Multiply ("Multiply", Vector) = (1,1,1,1)
   _MainTex ("Texture", 2D) = "white" {}
   _Offset ("Offset", Vector) = (0,0,0,0)
   _ColorOffsetMultiply("ColorOffsetMultiply", Vector) = (1,1,1,1)
   _WavePeriod("WavePeriod", Vector) = (0,0,0,0)
   _WaveMagnitude("_WaveMagnitude", Vector) = (0,0,0,0)
 }
 SubShader {
   Tags { "RenderType"="Opaque" }

   Pass {
     CGPROGRAM
     #pragma vertex vert
     #pragma fragment frag

     #include "UnityCG.cginc"

     struct appdata {
       float4 vertex : POSITION;
       float4 color : COLOR;
       float2 uv : TEXCOORD0;
     };

     struct v2f {
       float2 uv : TEXCOORD0;
       float4 color : COLOR;
       float4 vertex : SV_POSITION;
     };

     sampler2D _MainTex;
     float4 _MainTex_ST;
     float4 _Color;
     half4 _Offset;
     float4 _ColorOffsetMultiply;
	   half4 _Multiply;
     half4 _WavePeriod;
     half4 _WaveMagnitude;

     v2f vert (appdata v) {
       v2f o;
       o.vertex = UnityObjectToClipPos(v.vertex);
       o.color = v.color;
       o.uv = TRANSFORM_TEX(v.uv, _MainTex);
       return o;
     }

     fixed4 frag (v2f i) : COLOR {
       half2 uvOffset = half2(0,0); 
       if(i.uv.y % _Offset.z <= _Offset.z/2.0) uvOffset = _Offset.xy;
       float rComp = (tex2D(_MainTex, i.uv - uvOffset * _ColorOffsetMultiply.x + uvOffset * sin(i.uv.y * _WavePeriod.x) * _WaveMagnitude.x) * i.color * _Color * _Multiply).r;
       float gComp = (tex2D(_MainTex, i.uv - uvOffset * _ColorOffsetMultiply.y + uvOffset * sin(i.uv.y * _WavePeriod.y) * _WaveMagnitude.y) * i.color * _Color * _Multiply).g;
       float bComp = (tex2D(_MainTex, i.uv - uvOffset * _ColorOffsetMultiply.z + uvOffset * sin(i.uv.y * _WavePeriod.z) * _WaveMagnitude.z) * i.color * _Color * _Multiply).b;
       return fixed4(rComp, gComp, bComp, 1.0);
     }
     ENDCG
   }
 }
}
