Shader "Custom/TriplanarMappingShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Triplanar mapping
            float3 blendWeights = abs(IN.worldNormal);
            blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z); // Normalize weights

            // Sample textures from 3 planes
            float2 uvX = IN.worldPos.yz; // UV for X-plane
            float2 uvY = IN.worldPos.xz; // UV for Y-plane
            float2 uvZ = IN.worldPos.xy; // UV for Z-plane

            fixed4 colX = tex2D(_MainTex, uvX);
            fixed4 colY = tex2D(_MainTex, uvY);
            fixed4 colZ = tex2D(_MainTex, uvZ);

            // Blend textures based on normal
            fixed4 c = colX * blendWeights.x + colY * blendWeights.y + colZ * blendWeights.z;
            c *= _Color; // Apply tint

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
