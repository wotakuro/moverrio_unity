/// Yusuke Kurokawa 2015/05/12 
Shader "Custom/NonLight/circleChange" {
	Properties {
		_MainTex("Base (RGBA)", 2D) = "white" {}
		_AfterTex("Base (RGBA)", 2D) = "white" {}
		_DirStart("start", Range (0.0, 7.0)) = 0.0
		_DirSize("size", Range (-7.0, 0.0)) = 0.0
	}
	SubShader {
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

		Pass{
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _AfterTex;
			float _DirStart;
			float _DirSize;
			float4 _MainTex_ST;

			/** shader */
			struct v2f {
			    float4  pos : SV_POSITION;
    			float2  uv : TEXCOORD0;
                float  col : COLOR0;
			};
			/* vertex shader */
			v2f vert (appdata_base v)
			{
			    v2f o;
				o.pos   = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv    = TRANSFORM_TEX (v.texcoord, _MainTex);
				o.col = o.pos.w * 0.01 ;
			   return o;
			}
			/* flagment shader */
			half4 frag (v2f i) : COLOR
			{
				float PI = 3.1416;
			    half4 texcol = tex2D (_MainTex, i.uv);
			    half4 texcol2 = tex2D (_AfterTex, i.uv);
				float x = i.uv.x - 0.5;
				float y = i.uv.y - 0.5;
				float tmp = i.uv.x;
				float r = atan2( y,  x ) + PI + _DirStart;
				tmp = fmod( r , 2 * PI) + _DirSize;
				tmp = clamp( ceil(tmp) , 0.0 , 1.0 );
				texcol = texcol * tmp + texcol2 * (1-tmp );			
				return texcol;
			}
			ENDCG
		}
	} 
}
