﻿Shader "Custom/Game/CustomGUI" {
	Properties {
		_MainTex("Base (RGBA)", 2D) = "white" {}
		_Color (" Color", Color) = (0.0,1.0,0.0,1.0)
	}
	SubShader {
        Tags {"Queue" = "Transparent" }

//		Lighting Off Cull Off ZTest Always ZWrite Off Fog { Mode Off }
        Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _Color;
			float4 _MainTex_ST;

			/** shader */
			struct v2f {
			    float4  pos : SV_POSITION;
    			float2  uv : TEXCOORD0;
			};
/* vertex shader */
v2f vert (appdata_base v)
{
    v2f o;
    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    o.uv  = TRANSFORM_TEX (v.texcoord, _MainTex);
    return o;
}
/* flagment shader */
half4 frag (v2f i) : COLOR
{
    half4 texcol = tex2D (_MainTex, i.uv);
    texcol.r *= _Color.r;
    texcol.g *= _Color.g;
    texcol.b *= _Color.b;
	return texcol;
}

			ENDCG
		}
	} 
}
