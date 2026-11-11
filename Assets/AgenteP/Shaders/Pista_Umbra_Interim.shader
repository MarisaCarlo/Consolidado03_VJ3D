Shader "AgenteP/PistaUmbraInterim"
{
    Properties { _BaseColor("Color de pista", Color) = (0.08,0.75,0.96,1) _PulseSpeed("Velocidad", Range(0.1,5)) = 2 _EmissionStrength("Brillo", Range(0,6)) = 1.8 }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            Name "Unlit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _PulseSpeed;
                half _EmissionStrength;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; };
            struct Varyings { float4 positionCS:SV_POSITION; };
            Varyings vert(Attributes v) { Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); return o; }
            half4 frag(Varyings i):SV_Target
            {
                half pulse = (sin(_Time.y*_PulseSpeed)*.5h+.5h);
                half intensity = .3h + pulse * _EmissionStrength;
                return half4(_BaseColor.rgb * intensity,1);
            }
            ENDHLSL
        }
    }
}
