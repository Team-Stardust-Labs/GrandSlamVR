Shader "Custom/NeonWireframe"
{
    Properties
    {
        _WireColor ("Wire Color", Color) = (0,1,0,1)
        _WireThickness ("Wire Thickness", Range(0.001, 0.15)) = 0.03
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 2.0
       _SilhouettePower ("Silhouette Power", Range(1, 10)) = 2.0 // Added for outline effect
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        Cull Off // Show wireframe from all angles
        ZWrite On // Wireframe should still depth test
        Blend SrcAlpha OneMinusSrcAlpha // Standard alpha blending

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma geometry geom
            #pragma require geometry // Ensure geometry shader compilation
            #pragma target 4.0 // Geometry shaders require SM4.0 or higher

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
               float3 normal : NORMAL; // Added
            };

            // Vertex to Geometry shader structure
            struct v2g
            {
                float4 pos_clip : SV_POSITION; // Clip space position
                // We don't strictly need world position for this effect,
                // but it can be useful for other effects.
                float3 pos_world : TEXCOORD0; // Enabled and changed to float3
                float3 normal_world : TEXCOORD1; // Added
            };

            // Geometry to Fragment shader structure
            struct g2f
            {
                float4 pos_clip : SV_POSITION; // Clip space position
                float3 barycentric_coord : TEXCOORD0; // Barycentric coordinates
               float3 face_normal_world : TEXCOORD1; // Added: normal of the triangle
               float3 center_pos_world : TEXCOORD2; // Added: world position of triangle center (for view vector)
            };

            // Properties
            fixed4 _WireColor;
            float _WireThickness;
            float _EmissionIntensity;
           float _SilhouettePower; // Added

            v2g vert(appdata v)
            {
                v2g o;
                o.pos_clip = UnityObjectToClipPos(v.vertex);
                o.pos_world = mul(unity_ObjectToWorld, v.vertex).xyz; // Enabled and populated
                o.normal_world = UnityObjectToWorldNormal(v.normal); // Added
                return o;
            }

            [maxvertexcount(3)] // Output one triangle (3 vertices)
            void geom(triangle v2g input[3], inout TriangleStream<g2f> triStream)
            {
                g2f o;

               // Calculate world positions (already in input from v2g)
               float3 p0_world = input[0].pos_world;
               float3 p1_world = input[1].pos_world;
               float3 p2_world = input[2].pos_world;

               // Calculate face normal in world space
               // Ensure consistent winding order for normal calculation if not guaranteed by mesh
               float3 face_normal = normalize(cross(p1_world - p0_world, p2_world - p0_world));
               
               // Calculate triangle center for a representative view direction
               float3 triangle_center_world = (p0_world + p1_world + p2_world) / 3.0f;

               // Assign to output, same for all 3 vertices of this triangle
               o.face_normal_world = face_normal;
               o.center_pos_world = triangle_center_world;

                // Pass through vertices and assign barycentric coordinates
                // Vertex 0
                o.pos_clip = input[0].pos_clip;
                o.barycentric_coord = float3(1,0,0);
                triStream.Append(o);

                // Vertex 1
                o.pos_clip = input[1].pos_clip;
                o.barycentric_coord = float3(0,1,0);
                triStream.Append(o);

                // Vertex 2
                o.pos_clip = input[2].pos_clip;
                o.barycentric_coord = float3(0,0,1);
                triStream.Append(o);

                triStream.RestartStrip();
            }

            fixed4 frag(g2f i) : SV_Target
            {
                float3 barycentrics = i.barycentric_coord;
                
                // Calculate screen-space derivatives for anti-aliasing
                float3 screen_derived_bary = fwidth(barycentrics);
                
                // Determine how close we are to an edge
                // 'wires' will be close to 0 near an edge, close to 1 in the middle of a face
                float3 wires = smoothstep(_WireThickness - screen_derived_bary, 
                                          _WireThickness + screen_derived_bary, 
                                          barycentrics);
                
                // 'wire_alpha' will be 1.0 for lines, 0.0 for face centers
                float wire_alpha_base = 1.0 - min(min(wires.x, wires.y), wires.z);

                if (wire_alpha_base < 0.01) // Threshold to discard non-wire pixels
                {
                    discard;
                }

               // Silhouette enhancement:
               // Calculate view direction from triangle center (approx for pixel)
               float3 view_dir_world = normalize(_WorldSpaceCameraPos - i.center_pos_world);
               float NdotV = dot(i.face_normal_world, view_dir_world);

               // Attenuate lines on faces directly facing/away from camera, enhance lines on glancing faces
               // (1.0 - abs(NdotV)) is 0 for direct view, 1 for edge-on.
               float silhouette_factor = pow(1.0 - abs(NdotV), _SilhouettePower);
               
               float final_wire_alpha = wire_alpha_base * silhouette_factor;

               if (final_wire_alpha < 0.001) // Discard if too faint after modulation (adjusted threshold)
               {
                   discard;
               }

                fixed4 final_color = _WireColor;
                final_color.rgb *= _EmissionIntensity; // Apply emission
                final_color.a *= final_wire_alpha; // Apply modulated wire alpha

                return final_color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse" // Fallback for hardware that doesn't support geometry shaders
}
