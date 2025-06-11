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
            StructuredBuffer<float> Textures;

            uniform float ShouldRenderTextures;

            struct attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct varyings
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR0;
                float3 uv : TEXCOORD0;
            };

            UNITY_DECLARE_TEX2DARRAY(TextureArray);

            varyings vert(attributes v, const uint instance_id : SV_InstanceID)
            {
                const float4 pos = mul(Matrixes[instance_id], v.vertex);
                float4 color = Colors[instance_id];
                float z = (float)instance_id;
                varyings o;
                o.vertex = mul(UNITY_MATRIX_VP, pos);
                o.color = color;
                o.uv = float3(v.uv, Textures[instance_id]);
                return o;
            }

            half4 frag(varyings i) : SV_Target
            {
                if(ShouldRenderTextures == 1){
                    return UNITY_SAMPLE_TEX2DARRAY(TextureArray, i.uv);
                }
                return i.color;
            }
            ENDHLSL
        }
    }
}