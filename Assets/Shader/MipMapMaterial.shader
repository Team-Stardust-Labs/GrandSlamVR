Shader "Custom/MapMapMaterial"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _MinTiling("Min Tiling", Float) = 1.0
        _MaxTiling("Max Tiling", Float) = 4.0
        _StartDistance("Start Distance", Float) = 5.0
        _FadeDistance("Fade Distance", Float) = 25.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _MinTiling;
            float _MaxTiling;
            float _StartDistance;
            float _FadeDistance;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float3 GetObjectCenter()
            {
                return mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 camPos = _WorldSpaceCameraPos;
                float3 objPos = GetObjectCenter();

                float dist = distance(camPos, objPos);
                float t = saturate((dist - _StartDistance) / _FadeDistance);
                float tiling = lerp(_MaxTiling, _MinTiling, round(t*5) / 5);

                float2 uv = i.uv * tiling;

                return tex2D(_MainTex, uv);
            }
            ENDCG
        }
    }
}