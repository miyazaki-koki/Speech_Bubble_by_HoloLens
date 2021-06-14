Shader "Custom/Frame" {
	Properties{
		_MainTex("Texture", 2D) = "white"{}
		_Value("Alpha_range",Range(0.0,1.0)) = 0.6
	}
		SubShader{
		Tags{ "Queue" = "Transparent"
		  	  "RenderType" = "Opaque"
		}
			LOD 200

			CGPROGRAM
	#pragma surface surf Standard alpha:fade 
	#pragma target 3.0

			float _Value;
		sampler2D _MainTex;

		struct Input {
		float2 uv_MainTex;
	};

	void surf(Input IN, inout SurfaceOutputStandard o) {
		fixed2 p = IN.uv_MainTex;
		o.Albedo = tex2D(_MainTex, p);
		o.Alpha = _Value;
	}
	ENDCG
	}
		FallBack "Diffuse"
}