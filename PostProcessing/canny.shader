Shader "Hidden/canny" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}

		_texelw ("Width of an UV texel (square)", Float) = 0
		
		_invert ("Whather to invert colors", Int) = 0
	}
	SubShader {
	// Blurring pass
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float _texelw;
			
			//normpdf function gives us a Guassian distribution for each blur iteration
			float normpdf(float x, float sigma)
			{
				return 0.39894*exp(-0.5*x*x / (sigma*sigma)) / sigma;
			}

			//Gaussian blur
			float4 blur(sampler2D tex, float2 uv) {

				float4 col = tex2D(tex, uv);

				//total width/height of blur grid: has to be constant to allow for unrolling.
				const int mSize = 7;
				const int iter = (mSize - 1) / 2;

				for (int i = -iter; i <= iter; ++i) {
					for (int j = -iter; j <= iter; ++j) {
						col += tex2D(tex, float2(uv.x + i * _texelw, uv.y + j * _texelw)) * normpdf(float(i), 3);
					}
				}
				return col/mSize;
			}
			
			float4 frag(v2f_img i) : COLOR {
				
				// Blurring
				float4 result = blur(_MainTex, i.uv);
				
				float contrast = 1.0;
				float brightness = 0.0;
				float saturation = 2.0;
				
				// Changing contrast, brightness and saturation
				result.rgb = (result.rgb - 0.5f) * (contrast) + 0.5f; 
				result.rgb = result.rgb + brightness;        
				float3 intensity = dot(result.rgb, float3(0.299,0.587,0.114));
				result.rgb = lerp(intensity, result.rgb, saturation);
				
				// Edge gradient magnitude
				return result;
			}
			
			ENDCG
		}
		
		GrabPass {
			"_GaussedTex"
		}
		
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			sampler2D _GaussedTex;
			float _texelw;
			int _invert;
			
			float lum(float3 color) {
				return color.r*.3 + color.g*.59 + color.b*.11;
			}
			
			//Sobel operator
			float sobel(sampler2D tex, float2 uv){
				//float4 col = tex2D(tex, uv);
				
				float3 Gx = tex2D(tex, float2(uv.x-_texelw, uv.y-_texelw)).rgb
							+ 2*tex2D(tex, float2(uv.x-_texelw, uv.y)).rgb
							+ tex2D(tex, float2(uv.x-_texelw, uv.y+_texelw)).rgb
							+ (-1)*tex2D(tex, float2(uv.x+_texelw, uv.y-_texelw)).rgb
							+ (-2)*tex2D(tex, float2(uv.x+_texelw, uv.y)).rgb
							+ (-1)*tex2D(tex, float2(uv.x+_texelw, uv.y+_texelw)).rgb;
							
				float3 Gy = tex2D(tex, float2(uv.x-_texelw, uv.y-_texelw)).rgb
							+ 2*tex2D(tex, float2(uv.x, uv.y-_texelw)).rgb
							+ tex2D(tex, float2(uv.x+_texelw, uv.y-_texelw)).rgb
							+ (-1)*tex2D(tex, float2(uv.x-_texelw, uv.y+_texelw)).rgb
							+ (-2)*tex2D(tex, float2(uv.x, uv.y+_texelw)).rgb
							+ (-1)*tex2D(tex, float2(uv.x+_texelw, uv.y+_texelw)).rgb;
				float Gvx = max(max(max(Gx.r, Gx.g), Gx.b), lum(Gx));
				float Gvy = max(max(max(Gy.r, Gy.g), Gy.b), lum(Gy));
				float val = sqrt(Gvx*Gvx + Gvy*Gvy);
				
				return val;
			}
			
			float4 frag(v2f_img i) : COLOR {
			
				// Applying Sobel
				float sob = sobel(_GaussedTex, float2(i.uv.x, 1-i.uv.y));
				
				// Avoiding maximum suppression for the moment as I have to thicken the edge, not reduce it.
				
				if (_invert > 0.5) {
					sob = 1-sob;
				}
				
				float4 result = float4(sob, sob, sob, 1.0);
				return result;
			}
			
			ENDCG
		}
		
		GrabPass {
			"_SobeledTex"
		}
		
		// This brutally thickens the edges setting pixels to the maximum in a 5x5 grid.
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			sampler2D _SobeledTex;
			float _texelw;
			int _invert;
			
			float lum(float3 color) {
				return color.r*.3 + color.g*.59 + color.b*.11;
			}
			
			//Whiten the lines with high contrast
			float contr(sampler2D tex, float2 uv) {
				const int dist = 4;
				
				float4 col = tex2D(tex, uv);
				float minlum = 1.0;
				float maxlum = 0.0;
				
				for (int i = -dist; i <= dist; ++i) {
					for (int j = -dist; j <= dist; ++j) {
						
						if (length(float2(i, j)) > dist) {
							float4 pix = tex2D(tex, float2(uv.x+i*_texelw, uv.y+j*_texelw));
							minlum = min(minlum, lum(pix.rgb));
							maxlum = max(maxlum, lum(pix.rgb));
						}
					}
				}
				// First part whitens those who have a blacker part close. Second part whitens those close to a whiter part.
				if (lum(col) - minlum > 0.1 || maxlum - lum(col) > 0.1) {
					col.rgb = float3(1.0, 1.0, 1.0);
				}
				
				return col;
			}
			
			float4 frag(v2f_img i) : COLOR {
			
				float sob = contr(_SobeledTex, float2(i.uv.x, 1-i.uv.y));
				
				float4 result = float4(sob, sob, sob, 1.0);
				
				return result;
			}
			
			ENDCG
		}
	}
}