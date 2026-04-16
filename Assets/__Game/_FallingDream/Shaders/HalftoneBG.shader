Shader "Custom/HalftoneBG_CharacterOnly"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {}
        _DotColor ("Dot Color", Color) = (0.5, 0.4, 0.2, 1) // 점 색상 (레퍼런스의 금갈색)
        _BgColor ("Background Color", Color) = (0.74, 0.78, 0.78, 1) // 배경 색상 (민트그레이)
        _DotDensity ("Dot Density", Float) = 40.0 // 점의 개수 (낮을수록 점이 커짐)
    }
    SubShader
    {
        // 투명도를 지원하도록 Tags와 Blend 설정 수정
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha // 알파 블렌딩 활성화

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _DotColor;
            float4 _BgColor;
            float _DotDensity;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 1. 원본 샘플링 (현재 픽셀의 색상과 알파값 가져오기)
                fixed4 col = tex2D(_MainTex, i.uv);
                float alpha = col.a; // 캐릭터의 투명도 (있으면 1, 없으면 0에 가까움)

                // 2. 그리드 및 도트 계산 (캐릭터 영역 내에서만 의미 있음)
                float2 uv = i.uv * _DotDensity;
                float2 grid = frac(uv) - 0.5;
                
                // 캐릭터 영역의 밝기 계산 (밝기에 따라 점 크기 조절)
                float brightness = dot(col.rgb, float3(0.3, 0.59, 0.11));
                float radius = (1.0 - brightness) * 0.5; // 밝은 부분이 배경, 어두운 부분이 점
                float dist = length(grid);
                
                // 원 그리기 (부드러운 경계)
                float dotMask = smoothstep(radius, radius - 0.05, dist);

                // 3. 최종 색상 조합 ★핵심 수정 부분
                // 캐릭터가 있는 곳(alpha > 0.5)은 도트 효과 적용, 나머지는 배경색
                fixed4 finalColor = lerp(_BgColor, _DotColor, dotMask);
                
                // 알파 값이 낮은(배경) 부분은 배경색으로 강제 치환
                return lerp(_BgColor, finalColor, step(0.1, alpha)); 
            }
            ENDCG
        }
    }
}