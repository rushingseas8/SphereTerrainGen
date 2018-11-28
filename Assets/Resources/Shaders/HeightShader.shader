Shader "Custom/HeightShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
        
        _HeightMin ("Minimum height", Float) = 0
        _HeightMax ("Maximum height", Float) = 1
        
        _ColorMin ("Color at Min", Color) = (0, 0, 0, 1)
        _ColorMax ("Color at Max", Color) = (1, 1, 1, 1)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
        
        float _HeightMin;
        float _HeightMax;
        
        fixed4 _ColorMin;
        fixed4 _ColorMax;

		struct Input {
			float2 uv_MainTex;
            float3 worldPos;
		};  
        
        void surf(Input IN, inout SurfaceOutput o) {
            half4 c = tex2D(_MainTex, IN.uv_MainTex);
            float height = (_HeightMax - IN.worldPos.y) / (_HeightMax - _HeightMin);
            //float height = 0;
            fixed4 tintColor = lerp(_ColorMax.rgba, _ColorMin.rgba, height);
            
            o.Albedo = c.rgb * tintColor.rgb;
            o.Alpha = c.a * tintColor.a;
        }
		ENDCG
	}
	FallBack "Diffuse"
}
