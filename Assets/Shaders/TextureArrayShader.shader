// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TextureArrayShader"
{
    Properties
    {
        _MyArr ("Tex", 2DArray) = "" {}
        _SliceRange ("Slices", Range(0,16)) = 6
        _UVScale ("UVScale", Float) = 1.0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // texture arrays are not available everywhere,
            // only compile shader on platforms where they are
            #pragma require 2darray
            
            #include "UnityCG.cginc"
            
            struct input
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };
                        
            struct v2f
            {
                //float3 uv : TEXCOORD0;
                float4 uv : TEXCOORD0;
                float4 vertex : POSITION;
            };

            float _SliceRange;
            float _UVScale;
            //sampler2D _MyArr;
            UNITY_DECLARE_TEX2DARRAY(_MyArr);

            v2f vert (input inp)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(inp.vertex);
                //o.uv.xy = (vertex.xy + 0.5) * _UVScale;
                //o.uv.z = (vertex.z + 0.5) * _SliceRange;
                o.uv = inp.uv;
                return o;
            }
            

            float4 frag (v2f i) : SV_Target
            {
                //return UNITY_SAMPLE_TEX2DARRAY(_MyArr, i.uv);
                
                float4 uv_low = float4(i.uv.x, i.uv.y, floor(i.uv.z), 0);
                float4 low = UNITY_SAMPLE_TEX2DARRAY(_MyArr, uv_low);
                
                float low_weight = 1 - i.uv.z; // i.uv.z
                
                float4 uv_high = float4(i.uv.x, i.uv.y, ceil(i.uv.z), 0);
                float4 high = UNITY_SAMPLE_TEX2DARRAY(_MyArr, uv_high);
                
                float high_weight = 1 - low_weight;
                
                float4 avg = (low_weight * low) + (high_weight * high);
                
                return avg;
                //return tex2D(_MyArr, i.uv);
            }
            ENDCG
        }
    }
}