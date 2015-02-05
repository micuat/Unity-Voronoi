Shader "Custom/GlassReflective" {
	Properties {
		_SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
		_Cube ("Reflection Cubemap", Cube) = "black" { TexGen CubeReflect }
	}
	SubShader {
		Tags {
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
		}
		LOD 300
		
		CGPROGRAM
			#pragma surface surf BlinnPhong decal:add nolightmap
			
			samplerCUBE _Cube;
			
			fixed4 _ReflectColor;
			half _Shininess;
			
			struct Input {
				float3 worldRefl;
				float3 viewDir;
			};
			
			void surf (Input IN, inout SurfaceOutput o) {
				o.Albedo = 0;
				o.Gloss = 1;
				o.Specular = _Shininess;
				
				fixed4 reflcol = texCUBE (_Cube, IN.worldRefl);
				o.Emission = reflcol.rgb * _ReflectColor.rgb;
//				if( abs(dot(IN.viewDir, o.Normal)) < 0.2 )
//					o.Emission = float4(1,1,1,1);
				o.Alpha = reflcol.a * _ReflectColor.a;
			}
		ENDCG
	}
	FallBack "Transparent/VertexLit"
}