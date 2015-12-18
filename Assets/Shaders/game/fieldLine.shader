Shader "Custom/Game/FieldLine" {
	Properties {
		_LineColor ("Line Color", Color) = (0.0,1.0,0.0,1.0)
		//_FieldLineLight("lineTm", Float ) = 0.5 /** 0.0 - 1.0 */
	}
	SubShader {

		Pass{
		
			CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"

			sampler2D _MainTex;
			float _FieldLineLight;
			float4 _LineColor;
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
	// half4 pos = mul(UNITY_MATRIX_MV , v.vertex);
    o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
    o.col = o.pos.w * 0.02;
	//o.col = pos.z * 0.0002;
    return o;
}
/* flagment shader */
half4 frag (v2f i) : COLOR
{
	half4 texcol = (1.0,1.0,1.0,1.0);


	half tmp = (1.0 - (i.col) * (1.0 -  _FieldLineLight ) );

	tmp = clamp( tmp , 0.0 , 1.0 );
	half tmp2 = ( (tmp - 0.1)* 10.0) + min( (( tmp - 0.25 ) * -20.0) , 0.0 );

	tmp2 = clamp( tmp2 , 0.0 , 1.0);


	texcol.r = (texcol.r * 0.08 + (_LineColor.r) * tmp2 );
	texcol.g = (texcol.g * 0.08 + (_LineColor.g) * tmp2 );
	texcol.b = (texcol.b * 0.08 + (_LineColor.b) * tmp2 );
	texcol.a = 1.0;

	return texcol;
}

			ENDCG
		}
	} 
}
