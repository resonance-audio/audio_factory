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

Shader "Custom/OscilliscopeShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Thickness ("Thickness", float) = 0.05
		_SlopeCorrection ("Slope Correction", float) = 1.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM

		#define RESOLUTION 100

		// Upgrade NOTE: excluded shader from DX11, Xbox360, OpenGL ES 2.0 because it uses unsized arrays
		#pragma exclude_renderers d3d11 xbox360 gles
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		const uniform float _Values[RESOLUTION];
		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Thickness;
		half _SlopeCorrection;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {

			//This shader reads a single-dimensional data array, and clips across the Y-coordinate according to those data values. The extra work here is to lerp between the array indices properly


			//the index in the array which this x-coordinate will read the data from
			int index = IN.uv_MainTex.x*RESOLUTION;

			float val_0 = _Values[index];
			float val_1 = _Values[index+1];
			float val_2 = _Values[index+2];

			//the fraction between data-indices which the current x-coordinate is at
			float indexFraction = fmod(IN.uv_MainTex.x, 1.0/RESOLUTION);
			
			//difference between the current and next value
			float diff = val_1 - val_0;
			float nextDiff = val_2 - val_1;

			//clip twice, once for above the line, once for below the line

			float slopeCorrection = lerp(abs(diff), abs(nextDiff), indexFraction * RESOLUTION) * _SlopeCorrection;
			
			//clip the LOWER bound
			clip(IN.uv_MainTex.y - val_0 - indexFraction*diff*RESOLUTION + _Thickness + slopeCorrection);
			//clip the UPPER bound
			clip(val_0 - IN.uv_MainTex.y + indexFraction*diff*RESOLUTION + _Thickness + slopeCorrection);

			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Emission = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
