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

            uniform StructuredBuffer<float4x4> Matrixes;
            uniform StructuredBuffer<float3> Scales;
            uniform StructuredBuffer<float3> Colors;
            uniform StructuredBuffer<float> Textures;
            uint Row;
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
                uint ID = instance_id + Row;
                float3 vertex = v.vertex.xyz * Scales[ID];
                const float4 pos = mul(Matrixes[ID], float4(vertex, v.vertex.w));
                o.vertex = mul(UNITY_MATRIX_VP, pos);
                o.instance_id = ID;
                o.uv = float3(v.uv, Textures[ID]);
                return o;
            }
            half4 frag(Output i) : SV_Target
            {
                float4 color = float4(Colors[i.instance_id], 1);
                if(ShouldRenderTextures == 1){
                    return UNITY_SAMPLE_TEX2DARRAY(TextureArray, i.uv);
                }
                if(ShouldRenderTextures == 2){
                    return (UNITY_SAMPLE_TEX2DARRAY(TextureArray, i.uv) * color);
                }
                if(ShouldRenderTextures == 3){
                    return (UNITY_SAMPLE_TEX2DARRAY(TextureArray, i.uv) + color);
                }
                return color;
            }
            ENDHLSL
        }
    }
}
