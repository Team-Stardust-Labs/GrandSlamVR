Shader "Custom/TransparentGridDistanceFade"
{
    Properties
    {
        _MainTex ("Grid Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,0.5)
        _FadeStart ("Fade Start Distance", Float) = 0.0
        _FadeEnd ("Fade End Distance", Float) = 2.0
        _Tiling ("Texture Tiling", Vector) = (1, 1, 0, 0)  // Tiling control (X for X axis, Y for Y axis)
        _EyeLevel ("Eye Level Y", Float) = 1.5 // Default value for eye level height (adjustable)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Lighting Off
        Cull Back

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Color;
            float _FadeStart;
            float _FadeEnd;
            float4 _Tiling;  // Tiling parameters (X: X-axis, Y: Y-axis)
            float _EyeLevel; // Eye level height

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Apply tiling to UVs without using world-space UVs
                o.uv = v.uv * _Tiling.xy;  // Apply tiling based on the object UVs
                o.worldPos = worldPos.xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Distance-based fade calculation
                float dist = distance(_WorldSpaceCameraPos, i.worldPos);
                float fade = saturate((_FadeEnd - dist) / (_FadeEnd - _FadeStart));  // Linear fade

                // Fade out everything above eye level
                float heightFade = (i.worldPos.y < _EyeLevel) ? 1.0 : 0.0; // Fade out if above eye level

                fixed4 tex = tex2D(_MainTex, i.uv);  // Get texture color
                return tex * float4(_Color.rgb, _Color.a * fade * heightFade);  // Apply both fades
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
