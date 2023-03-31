Shader "FoW/DepthOnly"
{
	Properties
	{
	}
	SubShader
	{
		Cull Off
		ZWrite On
		ZTest LEqual
		ColorMask 0
		Tags { "RenderType" = "Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma multi_compile GAUSSIAN3 GAUSSIAN5 ANTIALIAS

			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return fixed4(1, 0, 0, 1);
			}
			ENDCG
		}
	}
}
