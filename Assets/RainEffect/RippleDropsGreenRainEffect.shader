Shader "Custom/RippleDropsGreenRainEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaterTex ("Water Texture", 2D) = "black" {}
        _DistortionAmount ("Distortion Amount", Range(0, 0.2)) = 0.08
        _RippleSpeed ("Ripple Speed", Range(0.1, 10)) = 0.8
        _GreenTint ("Green Tint", Range(0, 1)) = 0.5
        _Darkness ("Darkness", Range(0, 0.5)) = 0.2
        _WaterBlend ("Water Blend", Range(0, 1)) = 0.7
        _HorizontalMovement ("Horizontal Movement", Range(0, 1)) = 0.2
        _DropSize ("Drop Size", Range(0.001, 0.1)) = 0.035
        _DropBrightness ("Drop Brightness", Range(0, 0.5)) = 0.15
        _RippleWidth ("Ripple Width", Range(0.001, 0.05)) = 0.01
        _RippleIntensity ("Ripple Intensity", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _WaterTex;
            float4 _MainTex_ST;
            float _DistortionAmount;
            float _RippleSpeed;
            float _GreenTint;
            float _Darkness;
            float _WaterBlend;
            float _HorizontalMovement;
            float _DropSize;
            float _DropBrightness;
            float _RippleWidth;
            float _RippleIntensity;

            // Função de ruído simples
            float noise(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Função para criar ondulações realistas
            float createRipple(float2 uv, float time, float density, float dropSize, float rippleWidth)
            {
                float2 p = frac(uv * density) - 0.5;
                float2 id = floor(uv * density);
                
                float timeOffset = noise(id);
                float dropTime = frac(time * 0.2 + timeOffset);
                
                float2 dropPos = frac(id * 0.1) * 0.6 - 0.3; // Posição mais aleatória
                
                float dist = length(p - dropPos);
                
                // Ondulação que se expande
                float currentRippleRadius = dropTime * 0.5; // Raio da ondulação aumenta com o tempo
                float rippleEffect = smoothstep(currentRippleRadius - rippleWidth, currentRippleRadius, dist) - 
                                     smoothstep(currentRippleRadius, currentRippleRadius + rippleWidth, dist);
                
                // Intensidade diminui com o tempo
                rippleEffect *= (1.0 - dropTime) * (1.0 - dropTime);
                
                // Impacto inicial (pequeno círculo)
                float impact = smoothstep(dropSize, 0.0, dist) * (1.0 - dropTime * 2.0);
                
                return saturate(max(impact * 0.5, rippleEffect * _RippleIntensity));
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _RippleSpeed;
                
                // Criar distorção simples baseada em ondas senoidais (movimento horizontal reduzido)
                float2 distortion;
                distortion.x = sin(uv.y * 10.0 + time) * 0.005 * _DistortionAmount * _HorizontalMovement;
                distortion.y = cos(uv.x * 15.0 + time * 0.5) * 0.015 * _DistortionAmount;
                
                // Adicionar mais camadas de ondas para maior complexidade (com movimento horizontal reduzido)
                distortion.x += sin(uv.y * 20.0 + time * 0.4) * 0.003 * _DistortionAmount * _HorizontalMovement;
                distortion.y += cos(uv.x * 25.0 + time * 0.7) * 0.01 * _DistortionAmount;
                
                // Adicionar gotas com efeito de ondulação
                float ripple = 0.0;
                ripple += createRipple(uv, time, 70.0, _DropSize, _RippleWidth);
                ripple += createRipple(uv * 1.3 + 0.3, time * 1.1, 90.0, _DropSize * 0.7, _RippleWidth * 0.8) * 0.7;
                ripple += createRipple(uv * 1.6 + 0.7, time * 0.9, 120.0, _DropSize * 0.5, _RippleWidth * 0.6) * 0.5;
                ripple = saturate(ripple);
                
                // Adicionar distorção das ondulações (com movimento horizontal reduzido)
                distortion.x += ripple * 0.005 * _DistortionAmount * _HorizontalMovement;
                distortion.y += ripple * 0.015 * _DistortionAmount;
                
                // Amostra da textura com distorção
                fixed4 col = tex2D(_MainTex, uv + distortion);
                
                // Amostra da textura de água verde (com movimento mais suave)
                fixed4 waterCol = tex2D(_WaterTex, uv * 2.0 + float2(distortion.x * 1.0, distortion.y * 2.5) + float2(time * 0.02, time * 0.03));
                
                // Misturar a textura original com a textura de água
                col = lerp(col, waterCol, _WaterBlend);
                
                // Aplicar cor original
                col *= i.color;
                
                // Aplicar tint verde (mais intenso)
                float3 greenColor = float3(0.2, 0.9, 0.3);
                col.rgb = lerp(col.rgb, col.rgb * greenColor, _GreenTint);
                
                // Escurecer levemente
                col.rgb *= (1.0 - _Darkness);
                
                // Adicionar brilho nas ondulações para destacar
                col.rgb += ripple * _DropBrightness * greenColor;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}
