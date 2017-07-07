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

Shader "Custom/MysteryShader"
{
	Properties {
		_TimeScale ("Time Scale", Float) = 1
		_WarpTexture ("Warp Texture", 2D) = "white" {}
		_WarpPower ("Warp Power", Float) = 1
		_WarpAmount ("Warp Amount", Float) = 1
		_GlassColor ("Glass Color", Color) = (1,1,1,1)
		_GlassPower ("Glass Power", Float) = 1
		_VertWarpPower ("Vert Warp Power", Vector) = (0,0,0,0)
		_VertWarpPeriod ("Vert Warp Period", Vector) = (10,10,10,10)
	}

    SubShader
    {
        // Draw ourselves after all opaque geometry
        Tags { "Queue" = "Transparent+1" }

        // Grab the screen behind the object into _BackgroundTexture
        GrabPass
        {
            "_BackgroundTexture"
        }


        // Render the object with the texture generated above, and invert the colors
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BackgroundTexture;
			half4 _BackgroundTexture_ST;
			sampler2D _WarpTexture;
			half4 _WarpTexture_ST;
			half _WarpPower;
			half _WarpAmount;
			half4 _VertWarpPower;
			half4 _VertWarpPeriod;
			half _TimeScale;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half4 normal : NORMAL;
			};

            struct v2f
            {
                float4 grabPos : TEXCOORD0;
                float4 pos : SV_POSITION;
				float4 worldPos : TEXCOORD1;
				float3 viewDir : TEXCOORD2;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD3;
            };

            v2f vert(appdata v) {
                v2f o;
				v.vertex.x += _VertWarpPower.x*sin(_Time.x*25 + v.vertex.y*_VertWarpPeriod.x);
				v.vertex.y += _VertWarpPower.y*sin(_Time.x*25 + v.vertex.x*_VertWarpPeriod.y);
				v.vertex.z += _VertWarpPower.z*sin(_Time.x*25 + v.vertex.x*_VertWarpPeriod.z);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.vertex;
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.grabPos = ComputeGrabScreenPos(o.pos);
				o.normal = v.normal;
				o.uv = TRANSFORM_TEX(v.uv, _WarpTexture);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
				half rim = 1.0 - dot (normalize(i.viewDir), normalize(i.normal));
				rim = pow(rim, _WarpPower);

				half time = _Time.x * _TimeScale;
				half4 warp = half4(0.25*UnpackNormal(tex2D(_WarpTexture, i.uv + half2(time,time)))
						   + 0.25*UnpackNormal(tex2D(_WarpTexture, i.uv/2 + half2(-time,time)))
						   + 0.25*UnpackNormal(tex2D(_WarpTexture, i.uv/4 + half2(time,-time)))
						   + 0.25*UnpackNormal(tex2D(_WarpTexture, i.uv/8 + half2(-time,-time)))
						   ,0);
                half4 bgcolor = tex2Dproj(_BackgroundTexture, i.grabPos + warp*rim*half4(_WarpAmount,_WarpAmount,0,0));
				return bgcolor;
            }
            ENDCG
        }

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			sampler2D _WarpTexture;
			half4 _WarpTexture_ST;
			half _GlassPower;
			half4 _GlassColor;
			half4 _VertWarpPower;
			half4 _VertWarpPeriod;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float3 viewDir : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD2;
			};


			v2f vert (appdata v)
			{
				v2f o;
				v.vertex.x += _VertWarpPower.x*sin(_Time.x*25 + v.vertex.y*_VertWarpPeriod.x);
				v.vertex.y += _VertWarpPower.y*sin(_Time.x*25 + v.vertex.x*_VertWarpPeriod.y);
				v.vertex.z += _VertWarpPower.z*sin(_Time.x*25 + v.vertex.x*_VertWarpPeriod.z);
				o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
				o.normal = v.normal;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _WarpTexture);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				half rim = 1 - dot (normalize(i.viewDir), normalize(i.normal));
				half spec = dot (normalize(i.viewDir), normalize(i.normal));
				rim = pow(rim, _GlassColor.a * _GlassPower);
				return rim * _GlassColor;
			}
            ENDCG
        }
    }
}