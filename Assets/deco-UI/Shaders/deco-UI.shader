// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/deco-UI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [HideInInspector]_Color ("Tint", Color) = (1,1,1,1)

        [HideInInspector]_StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector]_Stencil ("Stencil ID", Float) = 0
        [HideInInspector]_StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector]_StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector]_StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector][Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
        [HideInInspector]_ColorMask ("Color Mask", Float) = 15
        
        [Header(Color)]
        [Space(25)]
        [Toggle(UNITY_UI_ALPHACLIP)] _IsSubColor("isSubColor",int) = 0
        _SubColor("SubColor",Color) = (1.0,1.0,1.0,1.0)
        _ContrastWeight("ContrastWeight",Float) = 0.02
        _ColorWeight("ColorWeight",Float) = 0.025
        
        [Header(Pattern)]
        [Space(25)]
        [KeywordEnum(Stripe,Dot,Square,Diamond,None)] _PatternType("PatternType",Int) = 0
        _PatternDensity("PatternDensity",Float) = 0.25
        _PatternRotate("PatternRotation",Range(-3.14,3.14)) = 0.0
        _PatternMoveDir("PatternMoveDir",Range(-3.14,3.14)) = 0.0
        [KeywordEnum(Gradation,Const)]_GradationMode("GradationMode",Int)=0
        _DotSize("DotSize",Float) = 0.5
        
        [Header(Edge)]
        [Space(25)]
        _EdgeWidth("EdgeWidth",Float) = 0.1
        [Toggle(UNITY_UI_ALPHACLIP)] _IsEdgeShining("isEdgeShining",int) = 0
        _EdgeShining("EdgeShining",Float) = 1.5
        _ShiningSpeed("ShiningSpeed",Float) = 1.0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                half4  mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            float _PatternDensity;
            float _PatternMoveDir;
            float _EdgeWidth;
            int _PatternType;
            int _GradationMode;
            float _ContrastWeight;
            float _ColorWeight;
            float _IsEdgeShining;
            float _ShiningSpeed;
            float _EdgeShining;
            float _IsSubColor;
            fixed4 _SubColor;
            float _PatternRotate;
            float _DotSize;

            fixed3 hsv2rgb(fixed3 hsv)
            {
                float h = hsv.x;
                float s = hsv.y;
                float v = hsv.z;
                return ((clamp(abs(frac(h+float3(0,2,1)/3.)*6.-3.)-1.,0.,1.)-1.)*s+1.)*v;

            }

            fixed3 rgb2hsv(fixed3 col)
            {
                float r = col.x;float g = col.y;float b = col.z;
                float max = r > g ? r : g;
                max = max > b ? max : b;
                float min = r < g ? r : g;
                min = min < b ? min : b;
                float h = max - min;
                if (h > 0.0f) {
                    if (max == r) {
                        h = (g - b) / h;
                        if (h < 0.0f) {
                            h += 6.0f;
                        }
                    } else if (max == g) {
                    h = 2.0f + (b - r) / h;
                    } else {
                    h = 4.0f + (r - g) / h;
                    }
                }
                h /= 6.0f;
                float s = (max - min);
                if (max != 0.0f)
                    s /= max;
                float v = max;
                return fixed3(h,s,v);
            }

            float stripe(fixed2 uv)
            {
                fixed2x2 rot = fixed2x2(cos(_PatternMoveDir),-sin(_PatternMoveDir),sin(_PatternMoveDir),cos(_PatternMoveDir));
                float t = (uv.x+uv.y)*_PatternDensity + mul(rot,_Time.y);
                return sin(t+_Time.y*3.14);
            }

            float dotted(fixed2 uv,float r)
            {
                fixed2x2 rot = fixed2x2(cos(_PatternMoveDir),-sin(_PatternMoveDir),sin(_PatternMoveDir),cos(_PatternMoveDir));
                fixed2 grid = floor(uv * _PatternDensity/4.0+mul(rot,_Time.y*fixed2(1.0,-1.0)));
                fixed2 st = frac(uv * _PatternDensity/4.0+mul(rot,_Time.y*fixed2(1.0,-1.0)));
                return (length(st-0.5) - 0.5*r);
                //return ((frac(grid.x/2.0) < 0.5 ^ frac(grid.y/2.0) < 0.5) ? 1.0 : 0.0) + (length(st-0.5) - 0.5*r);
            }

            float diamond(fixed2 uv,float r)
            {
                fixed2x2 rot = fixed2x2(cos(_PatternMoveDir),-sin(_PatternMoveDir),sin(_PatternMoveDir),cos(_PatternMoveDir));
                fixed2 grid = floor(uv * _PatternDensity/4.0+mul(rot,_Time.y*fixed2(1.0,-1.0)));
                fixed2 st = frac(uv * _PatternDensity/4.0+mul(rot,_Time.y*fixed2(1.0,-1.0)));
                fixed val = abs(st.x-0.5) + abs(st.y-0.5);
                return (length(val/1.414) - 0.5*r);
                //return ((frac(grid.x/2.0) < 0.5 ^ frac(grid.y/2.0) < 0.5) ? 1.0 : 0.0) + (length(st-0.5) - 0.5*r);
            }
            float squared(fixed2 uv,float size)
            {
                fixed2x2 rot = fixed2x2(cos(_PatternMoveDir),-sin(_PatternMoveDir),sin(_PatternMoveDir),cos(_PatternMoveDir));
                fixed2 grid = floor(uv * _PatternDensity/4.0 + mul(rot,_Time.y*fixed2(1.0,1.0)));
                fixed2 st = frac(uv * _PatternDensity/4.0 + mul(rot,_Time.y*fixed2(1.0,1.0))) - 0.5;
                return max(abs(st.x),abs(st.y)) - size*0.5;
            }

            float edged(fixed2 uv,float width)
            {
                float e = 1.0;
                float theta = 30.0;
                for(int i = 0; i < 12; i++)
                {
                    e = min(e,tex2D(_MainTex,uv + width * float2(cos(theta * i),sin(theta * i))).w);
                }
                return e;
            }
        
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 vPosition = UnityObjectToClipPos(v.vertex);
                OUT.worldPosition = v.vertex;
                OUT.vertex = vPosition;

                float2 pixelSize = vPosition.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
                float2 maskUV = (v.vertex.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = half4(v.vertex.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixelSize.xy)));

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;
                fixed4 subColor = color;
                subColor.rgb *= (1.0-_ContrastWeight);
                fixed3 hsv = rgb2hsv(subColor.rgb);
                hsv.x = frac(hsv.x - _ColorWeight);
                subColor.rgb = hsv2rgb(hsv);
                subColor.rgb = _IsSubColor ? _SubColor.rgb : subColor.rgb;
                


                fixed2 uv = IN.vertex.xy;
                fixed2x2 rot = fixed2x2(cos(_PatternRotate),-sin(_PatternRotate),sin(_PatternRotate),cos(_PatternRotate));
                fixed2 pos = fixed2(IN.texcoord.x,1.0 - IN.texcoord.y);
                uv = mul(rot,uv - 0.5) + 0.5;
                pos = mul(rot,pos - 0.5) + 0.5;
                float gradVal = clamp(pow(pos.x+pos.y-0.25,2.0),0.0,1.0)*sqrt(2.0);
                gradVal = _GradationMode == 1 ? _DotSize : gradVal;
                float val = _PatternType == 1 ? dotted(uv,gradVal) : 1.0;
                val = _PatternType == 0 ? stripe(uv) : val;
                val = _PatternType == 2 ? squared(uv,length(pos)/1.0) : val;
                val = _PatternType == 3 ? diamond(uv,gradVal) : val;
                float isEdge = edged(IN.texcoord,_EdgeWidth) < 0.3;
                val = min(isEdge ? -1.0 : 1.0,val);
                color.rgb = val < 0.0 ? color.rgb : subColor.rgb;
                color.rgb *= _IsEdgeShining && isEdge && frac(_Time.y*_ShiningSpeed+atan2(IN.texcoord.y - 0.5,IN.texcoord.x - 0.5)/3.14) < 0.2 ? _EdgeShining : 1.0;
                return color;
            }
        ENDCG
        }
    }
}
