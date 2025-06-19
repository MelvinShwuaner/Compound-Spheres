Shader "CompoundSphere"
{
    Properties{
        TextureArray("TextureArray", 2DArray) = "" {}
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "RenderType"="Opaque"
                "RenderPipeline" = "UniversalRenderPipeline"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma require 2darray

            #include "UnityCG.cginc"

            StructuredBuffer<float4x4> Matrixes;
            StructuredBuffer<float4> Colors;
            StructuredBuffer<int> Textures;

            uniform float ShouldRenderTextures;

            struct Input
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Output
            {
                float4 vertex : SV_POSITION;
                float3 uv : TEXCOORD0;
                uint instance_id : SV_InstanceID ;
            };

            UNITY_DECLARE_TEX2DARRAY(TextureArray);
            Output vert(Input v, const uint instance_id : SV_InstanceID)
            {
                Output o;
                const float4 pos = mul(Matrixes[instance_id], v.vertex);
                o.vertex = mul(UNITY_MATRIX_VP, pos);
                o.instance_id = instance_id;
                o.uv = float3(v.uv, Textures[instance_id]);
                return o;
            }
            half4 GetColor(Output i){
                if(ShouldRenderTextures == 1){
                    return UNITY_SAMPLE_TEX2DARRAY(TextureArray, i.uv);
                }
                if(ShouldRenderTextures == 2){
                    return UNITY_SAMPLE_TEX2DARRAY(TextureArray, i.uv) * Colors[i.instance_id];
                }
                if(ShouldRenderTextures == 3){
                    return UNITY_SAMPLE_TEX2DARRAY(TextureArray, i.uv) + Colors[i.instance_id];
                }
                return Colors[i.instance_id];
            }
            half4 frag(Output i) : SV_Target
            {
                return GetColor(i);
            }
            ENDHLSL
        }
    }
}