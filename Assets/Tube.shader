﻿Shader "Custom/Tube"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert addshadow nolightmap
        #pragma target 5.0

        struct Input {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        half4 _Color;
        half _Radius;

        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        #ifdef SHADER_API_D3D11
            StructuredBuffer<float3> _PositionBuffer;
            StructuredBuffer<float3> _TangentBuffer;
            StructuredBuffer<float3> _NormalBuffer;
        #endif

        void vert(inout appdata_full v, out Input data) {
            UNITY_INITIALIZE_OUTPUT(Input, data);

            #ifdef SHADER_API_D3D11
                float phi = v.vertex.x; // Angle in slice
                float cap = v.vertex.y; // -1:head, +1:tail
                float seg = v.vertex.z; // Segment index

                uint idx = (uint)seg;
                float3 p = _PositionBuffer[idx];
                float3 t = _TangentBuffer[idx];
                float3 n = _NormalBuffer[idx];
                float3 b = cross(t, n);

                float3 normal = n * cos(phi) + b * sin(phi);
                float4 position = float4(p + normal * _Radius * (1 - abs(cap)), 1);

                v.vertex = position;
                v.normal = normal * (1 - abs(cap));
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o) {
            o.Albedo = _Color.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
