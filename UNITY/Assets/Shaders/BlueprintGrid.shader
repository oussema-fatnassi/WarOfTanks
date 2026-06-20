Shader "WarOfTanks/BlueprintGrid"
{
    Properties
    {
        [Header(Colors)]
        _BgColor        ("Background Color", Color)      = (0.043, 0.082, 0.133, 1)   // deep navy
        _BgColorEdge    ("Background Edge (vignette)", Color) = (0.016, 0.035, 0.063, 1)
        _LineColor      ("Grid Line Color", Color)       = (0.45, 0.62, 0.78, 0.30)
        _CrossColor     ("Intersection Cross Color", Color) = (0.70, 0.85, 1.0, 0.65)

        [Header(Grid Layout (world units))]
        _GridSize       ("Cell Size (world units)", Float)   = 4.0    // size of one big square
        _LineThickness  ("Line Thickness (units)", Float)    = 0.035
        _CrossSize      ("Cross Arm Length (units)", Float)   = 0.18
        _CrossThickness ("Cross Thickness (units)", Float)    = 0.045

        [Header(Effects)]
        _ScanlineStrength ("Scanline Strength", Range(0,1)) = 0.10
        _ScanlineDensity  ("Scanline Density", Float)      = 240.0
        _Vignette         ("Vignette Strength", Range(0,1)) = 0.6
    }

    SubShader
    {
        // Built-in Render Pipeline. Opaque so it always draws behind transparent sprites.
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos     : SV_POSITION;
                float2 worldXY : TEXCOORD0;
                float2 uv      : TEXCOORD1;
            };

            float4 _BgColor;
            float4 _BgColorEdge;
            float4 _LineColor;
            float4 _CrossColor;
            float  _GridSize;
            float  _LineThickness;
            float  _CrossSize;
            float  _CrossThickness;
            float  _ScanlineStrength;
            float  _ScanlineDensity;
            float  _Vignette;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldXY = mul(unity_ObjectToWorld, v.vertex).xy;
                o.uv = v.uv;
                return o;
            }

            // Antialiased line mask: 1 on the line, 0 off it. distToLine is in world units.
            float LineMask(float distToLine, float halfThickness)
            {
                float aa = fwidth(distToLine) * 1.25 + 1e-5;
                return 1.0 - smoothstep(halfThickness, halfThickness + aa, distToLine);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float gs = max(_GridSize, 0.0001);

                // Distance (in world units) to the nearest grid line on each axis.
                float2 g = i.worldXY / gs;
                float2 cellFrac = abs(frac(g) - 0.5);          // 0 at line, 0.5 at cell center
                float2 distToLine = cellFrac * gs;             // back to world units

                // --- grid lines ---
                float halfLine = _LineThickness * 0.5;
                float lineX = LineMask(distToLine.x, halfLine);
                float lineY = LineMask(distToLine.y, halfLine);
                float grid = saturate(lineX + lineY);

                // --- intersection cross marks (a small plus at every node) ---
                float halfCross = _CrossThickness * 0.5;
                float armH = LineMask(distToLine.y, halfCross) * step(distToLine.x, _CrossSize);
                float armV = LineMask(distToLine.x, halfCross) * step(distToLine.y, _CrossSize);
                float cross = saturate(armH + armV);

                // --- vignette / radial darkening from quad center ---
                float2 c = i.uv - 0.5;
                float vig = 1.0 - saturate(dot(c, c) * 2.0) * _Vignette;
                float3 baseCol = lerp(_BgColorEdge.rgb, _BgColor.rgb, vig);

                // --- compose ---
                float3 col = baseCol;
                col = lerp(col, _LineColor.rgb,  grid  * _LineColor.a);
                col = lerp(col, _CrossColor.rgb, cross * _CrossColor.a);

                // --- subtle horizontal scanlines (CRT/blueprint texture) ---
                float scan = sin(i.worldXY.y * _ScanlineDensity) * 0.5 + 0.5;
                col *= 1.0 - _ScanlineStrength * scan;

                return fixed4(col, 1.0);
            }
            ENDCG
        }
    }
    Fallback Off
}
