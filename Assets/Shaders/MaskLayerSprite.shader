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
                // --- [原有逻辑] 全局禁用处理 ---
                if (_GlobalMaskEnabled < 0.5)
                {
                    #if _LAYER_LAYERA
                        if (_GlobalMaskShowLayerA < 0.5) col.a = 0;
                    #elif _LAYER_LAYERB
                        if (_GlobalMaskShowLayerB < 0.5) col.a = 0;
                    #endif
                    return col;
                }

                // 计算边界判定数据
                float2 halfSize = _GlobalMaskSize * 0.5;
                // 计算当前像素点距离遮罩边缘的距离 (负数表示在内部，正数表示在外部)
                float2 d = abs(i.worldPos.xy - _GlobalMaskCenter) - halfSize;
                // 找到 X 和 Y 方向最靠近边缘的那一个距离
                float distToEdge = max(d.x, d.y);

                // 判定是否在矩形内（包含边框）
                float borderThickness = 0.04;
                bool insideMask = distToEdge <= 0;
                bool isBorder = distToEdge > -borderThickness && distToEdge <= 0;

                #if _LAYER_LAYERA
                    // A 层：遮罩内部完全掏空
                    if (insideMask) col.a = 0;
                    else col.a *= _OutsideAlpha;
                #elif _LAYER_LAYERB
                    // B 层：遮罩内部显示
                    if (insideMask)
                    {
                        if (isBorder)
                        {
                            // --- 边框绘制逻辑 ---
                            if (_GlobalMaskActive > 0.5)
                            {
                                // 1. 激活状态：粉色
                                col = fixed4(0.94,0.48,0.54,1); 
                            }
                            else
                            {
                                // 2. 未激活状态：粉色虚线
                                // 使用世界坐标计算虚线，5.0 是频率，0.5 是断开比例
                                float dash = step(0.5, frac((i.worldPos.x + i.worldPos.y) * 5.0));
                               if (dash > 0.5) 
                                {
                                     col = fixed4(0.94,0.48,0.54,1);  
                                }
                                else
                                {
                                    float targetAlpha = lerp(_GlobalMaskPreviewAlpha, _GlobalMaskActiveAlpha, _GlobalMaskActive);
                                    col.a *= targetAlpha;
                                }
                            }
                        }
                        else
                        {
                            // 遮罩内部图像像素
                            float targetAlpha = lerp(_GlobalMaskPreviewAlpha, _GlobalMaskActiveAlpha, _GlobalMaskActive);
                            col.a *= targetAlpha;
                        }
                    }
                    else
                    {
                        // Mask 范围外完全隐藏
                        col.a = 0; 
                    }
                #endif

                return col;
            }
            ENDCG
        }
    }
}
