Shader "Hidden/black" {
Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			float4 frag(v2f_img i) : COLOR {
				return float4(0.0, 0.0, 0.0, 1.0);
			}
			ENDCG
		}
	}
}


