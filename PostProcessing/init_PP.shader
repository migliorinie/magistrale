Shader "Hidden/init_PP" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_area_radius ("Radius of the viewing zone", Float) = 0
		_electrode_radius ("Radius of one electrode", Float) = 0
		_electrode_distance ("Distance between electrodes", Float) = 0
		
		_levels ("Levels of grey", Int) = 0
		_invert ("Inverts colors if set to 1", Int) = 0
		
		_texelw ("Width of an UV texel (square)", Float) = 0
		
		_centerx ("Center offset on X axis", Float) = 0
		_centery ("Center offset on Y axis", Float) = 0
		
		_dimension_variance ("Dimension variability", Float) = 0
		
		_broken_chance("Chance that one electrode is burned out", Float) = 0
		_lumin_variance("Variability of luminosity for the electrodes", Float) = 0
		_random_activation("Percentage of electrodes that activate randomly", Float) = 0
		
		_seed_mask ("Pseudorandom seed for the mask", Float) = 0
		
		_seed_main ("Pseudorandom seed for the main part", Float) = 0
		
		_is_black ("Whether to return black", Int) = 0
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
			float _centerx;
			float _centery;
			
			float _seed_main;
			float _seed_mask;
			
			float _dimension_variance;
			float _broken_chance;
			float _lumin_variance;
			float _random_activation;
			
			int _is_black;
			
			//Pseudorandom. Wonderfully deterministic.
			float rand(float2 co, float seed){
				return frac(sin(dot(float3(co.x, co.y, seed) ,float3(12.9898,78.233, 84.637))) * 43758.5453);
			}
			
			//normpdf function gives us a Guassian distribution for each blur iteration
			float normpdf(float x, float sigma)
			{
				return 0.39894*exp(-0.5*x*x / (sigma*sigma)) / sigma;
			}

			//Gaussian blur
			float4 blur(sampler2D tex, float2 uv) {
				float4 col = tex2D(tex, uv);

				//total width/height of blur grid: has to be constant to allow for unrolling.
				const int mSize = 5;
				const int iter = 3;

				for (int i = -iter; i <= iter; ++i) {
					for (int j = -iter; j <= iter; ++j) {
						col += tex2D(tex, float2(uv.x + i * _texelw, uv.y + j * _texelw)) * normpdf(float(i), 1);
					}
				}
				return col/mSize;
			}
			
			float4 frag(v2f_img i) : COLOR {
				if (_is_black > 0.5) {
					return float4(0.0, 0.0, 0.0, 1.0);
				}
				
				float2 setuv = float2(i.uv.x, i.uv.y);
				float4 c = tex2D(_MainTex, setuv);
				float4 result = c;
				
				result.rgb = float3(0.0, 0.0, 0.0);
				
				float2 targ;
				
				float2 center = float2(_centerx, _centery);
				
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
					
					// Taking Gaussian average of the closest pixels.
					float4 tpx = blur(_MainTex, targ);
					
					int2 pixel_setuv = int2(floor(setuv.x/_texelw), floor(setuv.y/_texelw));
					int2 pixel_targ = int2(floor(targ.x/_texelw), floor(targ.y/_texelw));
					
					// Linear space centered in 1
					float var = rand(targ, _seed_main)*_dimension_variance*2 + (1-_dimension_variance);
					
					if (abs(distance(targ, center) - _area_radius) > _electrode_radius) {
																// Rounding to 2 decimal places
						//if (distance(setuv, targ) <= round((_electrode_radius)*var*100)*0.01) {
						if (distance(setuv, targ) <= _electrode_radius*var) {
							
							//tpx.rgb = lerp(tpx.rgb, float3(1.0, 1.0, 1.0), 0.3);
							
							// TODO: Possibly have it work on average or maximum of a small kernel.
							float lum = tpx.r*.3 + tpx.g*.59 + tpx.b*.11;
							
							float thresh = 1.0/_levels;
							
							// Normalizing the range to [0, 1]
							lum = floor(lum*(_levels)-0.01)/(_levels-1u);
							if (_invert > 0.5) {
								lum = 1-lum;
							}
							
							// Setting to quantified b/w
							float3 bw = float3(lum, lum, lum); 
							result.rgb = bw;
							
							// If broken.
							float chance1 = rand(pixel_targ, _seed_main);
							if (chance1 < _broken_chance) {
								result.rgba = float4(0.0, 0.0, 0.0, 1.0);
							}
							else { // Varying luminosity, up to -30%
								float chance2 = rand(pixel_targ, _seed_mask)*_lumin_variance;
								result.rgb = lerp(result.rgb, float3(0.0, 0.0, 0.0), chance2);
								
								float chance3 = rand(pixel_targ, chance2);
								if (chance3 <= _random_activation) {
									float chance4 = rand(pixel_targ, chance3);
									result.rgb = float3(chance4, chance4, chance4);
								}
							}
						}
					}
					
					
				}
				
				return result;
			}
			ENDCG
		}
		
		GrabPass{
			"_Filtered_Tex"
		}
		
		// Blurring pass
		Pass {
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			sampler2D _Filtered_Tex;
			sampler2D _MainTex;
			
			float _texelw;
			int _is_black;
			
			//normpdf function gives us a Guassian distribution for each blur iteration
			float normpdf(float x, float sigma)
			{
				return 0.39894*exp(-0.5*x*x / (sigma*sigma)) / sigma;
			}

			//Gaussian blur
			float4 blur(sampler2D tex, float2 uv) {
				float4 col = tex2D(tex, uv);

				//total width/height of blur grid: has to be constant to allow for unrolling.
				const int mSize = 5;
				const int iter = (mSize - 1) / 2;

				for (int i = -iter; i <= iter; ++i) {
					for (int j = -iter; j <= iter; ++j) {
						col += tex2D(tex, float2(uv.x + i * _texelw, uv.y + j * _texelw)) * normpdf(float(i), 3);
					}
				}
				return col/mSize;
			}
			
			// Blurring
			float4 frag(v2f_img i) : COLOR {
				if (_is_black > 0.5) {
					return float4(0.0, 0.0, 0.0, 1.0);
				}
				float2 setuv = float2(i.uv.x, i.uv.y);
				float4 c = tex2D(_Filtered_Tex, setuv);
				float4 result = blur(_Filtered_Tex, setuv);
				
				return result;
			}
			ENDCG
		}
	}
}


