Shader "Hidden/mask_noise" {
Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_area_radius ("Radius of the viewing zone", Float) = 0
		_electrode_radius ("Radius of one electrode", Float) = 0
		_electrode_distance ("Distance between electrodes", Float) = 0
		
		_levels ("Levels of grey", Int) = 0
		_invert ("Inverts colors if set to 1", Int) = 0
		
		_texelw ("Width of an UV texel (square)", Float) = 0
		
		_broken_chance("Chance that one electrode is burned out", Float) = 0
		_lumin_variance("Variability of luminosity for the electrodes", Float) = 0
		
		_seed ("Pseudorandom seed", Float) = 0
	}
	SubShader {
		
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			float _area_radius;
			float _electrode_radius;
			float _electrode_distance;
			
			int _levels;
			int _invert;
			
			float _texelw;
			
			float _broken_chance;
			float _lumin_variance;
			float _seed;
			
			//Pseudorandom. Wonderfully deterministic.
			float rand(float2 co, float seed){
				return frac(sin(dot(float3(co.x, co.y, seed) ,float3(12.9898,78.233, 84.637))) * 43758.5453);
			}
			
			float4 frag(v2f_img i) : COLOR {
				float2 setuv = float2(i.uv.x, i.uv.y);
				float4 c = tex2D(_MainTex, setuv);
				float4 result = c;
				result.rgb = float3(0.0, 0.0, 0.0);
				
				float2 targ;
				
				float2 center = float2(0.5, 0.5);
				
				if (distance(setuv, center) <= _area_radius) {
				// Translate to use the float system (or double).
					float left_col = setuv.x - fmod(setuv.x, (0.8660254*_electrode_distance));
					float left_band = left_col + (0.28867*_electrode_distance); // sqrt(3)/6
					float right_band = left_band + (0.28867*_electrode_distance);
					float right_col = left_col + (0.866*_electrode_distance);
					
					float upper_row = setuv.y - fmod(setuv.y, _electrode_distance);
					float middle_row = upper_row+(_electrode_distance/2u);
					float lower_row = upper_row+(_electrode_distance);
					
					// If we are left of an even column
					if((uint)floor(setuv.x/(0.8660254*_electrode_distance))%2 == 0) {
						if (setuv.x < left_band) {
							if (setuv.y > middle_row) {
								targ = float2(left_col, lower_row);
							} else {
								targ = float2(left_col, upper_row);
							}
						} else if (setuv.x > right_band) {
							targ = float2(right_col, middle_row);
						} else {
							if (setuv.y < middle_row) {
								// Zig-zagging part: using line equations to find the target
								if ((setuv.x - left_band)*1.73205 + (setuv.y - middle_row) > 0) {
									targ = float2(right_col, middle_row);
								} else {
									targ = float2(left_col, upper_row);
								}
							} else {
								if ((setuv.x - left_band)*1.73205 - (setuv.y - middle_row) > 0) {
									targ = float2(right_col, middle_row);
								} else {
									targ = float2(left_col, lower_row);
								}
							}
						}
					} else {
						if (setuv.x < left_band) {
							targ = float2(left_col, middle_row);
						} else if (setuv.x > right_band) {
							if (setuv.y > middle_row) {
								targ = float2(right_col, lower_row);
							} else {
								targ = float2(right_col, upper_row);
							}
						} else {
							if (setuv.y < middle_row) {
							if ((setuv.x - left_band)*1.73205 - (setuv.y - upper_row) > 0) {
									targ = float2(right_col, upper_row);
								} else {
									targ = float2(left_col, middle_row);
								}
							} else {
								if ((setuv.x - left_band)*1.73205 + (setuv.y - lower_row) > 0) {
									targ = float2(right_col, lower_row);
								} else {
									targ = float2(left_col, middle_row);
								}
							}
						}
					}
					
					float4 tpx = tex2D(_MainTex, targ);
					
					int2 pixel_targ = int2(floor(targ.x/_texelw), floor(targ.y/_texelw));

					// If broken.
					float chance1 = rand(pixel_targ, _seed);
					if (chance1 <= _broken_chance) {
						result.rgba = float4(0.0, 0.0, 0.0, 1.0);
					}
					else { // Varying luminosity, up to -30%
						float chance2 = rand(pixel_targ, chance1)*_lumin_variance;
						result.rgb = lerp(c.rgb, float3(0.0, 0.0, 0.0), chance2);
					}
				}
				
				return result;
			}
			ENDCG
		}
	}
}


