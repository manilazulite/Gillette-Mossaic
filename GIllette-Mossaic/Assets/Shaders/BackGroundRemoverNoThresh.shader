Shader "Custom/BackGroundRemoverNoThresh"
{
    Properties
    {
        _Background("", 2D) = ""{}
        _CameraFeed("", 2D) = ""{}
        _Mask("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _Background;
    sampler2D _CameraFeed;
    sampler2D _Mask;
    float _Threshold;

    void Vertex(float4 position : POSITION,
                float2 uv : TEXCOORD0,
                out float4 outPosition : SV_Position,
                out float2 outUV : TEXCOORD0)
    {
        outPosition = UnityObjectToClipPos(position);
        outUV = uv;
    }

    float4 Fragment(float4 position : SV_Position,
                    float2 uv : TEXCOORD0) : SV_Target
    {
        float3 bg = tex2D(_Background, uv).rgb;
        float3 fg = tex2D(_CameraFeed, uv).rgb;
        float mask = tex2D(_Mask, uv).r;
        return float4(lerp(bg, fg, mask), 1);
    }

    ENDCG

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDCG
        }
    }
}
