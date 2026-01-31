Shader "Custom/MaskLayerSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Mask Settings)]
        [KeywordEnum(LayerA, LayerB)] _Layer ("Layer Type", Float) = 0
        _OutsideAlpha ("Outside Mask Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _LAYER_LAYERA _LAYER_LAYERB

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _OutsideAlpha;

            // 全局参数（由 MaskGlobalManager 设置）
            float _GlobalMaskEnabled;     // 0 = 禁用效果（编辑器模式），1 = 启用
            float _GlobalMaskShowLayerA;  // 编辑器中是否显示 A 层
            float _GlobalMaskShowLayerB;  // 编辑器中是否显示 B 层
            float2 _GlobalMaskCenter;
            float2 _GlobalMaskSize;
            float _GlobalMaskActive;      // 0 = 未激活（预览），1 = 激活
            float _GlobalMaskPreviewAlpha; // 预览时 B 层透明度
            float _GlobalMaskActiveAlpha;  // 激活时 B 层透明度

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // 如果效果被禁用，根据层级和显示设置决定是否显示
                if (_GlobalMaskEnabled < 0.5)
                {
                    #if _LAYER_LAYERA
                        if (_GlobalMaskShowLayerA < 0.5)
                            col.a = 0;
                    #elif _LAYER_LAYERB
                        if (_GlobalMaskShowLayerB < 0.5)
                            col.a = 0;
                    #endif
                    return col;
                }

                // 检查是否在 mask 范围内
                float2 halfSize = _GlobalMaskSize * 0.5;
                float2 minBound = _GlobalMaskCenter - halfSize;
                float2 maxBound = _GlobalMaskCenter + halfSize;

                bool insideMask = i.worldPos.x >= minBound.x && i.worldPos.x <= maxBound.x &&
                                  i.worldPos.y >= minBound.y && i.worldPos.y <= maxBound.y;

                #if _LAYER_LAYERA
                    // A 层：mask 内隐藏
                    if (insideMask)
                    {
                        col.a = 0;
                    }
                    else
                    {
                        col.a *= _OutsideAlpha;
                    }
                #elif _LAYER_LAYERB
                    // B 层：mask 内根据激活状态显示
                    if (insideMask)
                    {
                        float targetAlpha = lerp(_GlobalMaskPreviewAlpha, _GlobalMaskActiveAlpha, _GlobalMaskActive);
                        col.a *= targetAlpha;
                    }
                    else
                    {
                        // mask 外隐藏
                        col.a = 0;
                    }
                #else
                    // 默认当作 A 层
                    if (insideMask)
                    {
                        col.a = 0;
                    }
                #endif

                return col;
            }
            ENDCG
        }
    }
}
