Shader "CurissShader/EmissionScroll/Flat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _EmissionStrength ("EmissionStranth", float) = 0
        _EmissiveScroll_Direction ("Direction", Vector) = (0,0,0,0)
        _EmissiveScroll_Velocity ("Velocity", float) = 0
        _EmissiveScroll_Interval ("Interval", float) = 0
        _EmissiveScroll_Width ("Width", float) = 0
        _Tilt ("Tilt", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _EmissionStrength;
            float4 _EmissiveScroll_Direction;
            float _EmissiveScroll_Velocity;
            float _EmissiveScroll_Interval;
            float _EmissiveScroll_Width;
            float _Tilt;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float emission = dot(i.uv, _EmissiveScroll_Direction);
                emission -= _Time.y * _EmissiveScroll_Velocity;
                emission /= _EmissiveScroll_Interval;
                emission -= floor(emission);

                float width = _EmissiveScroll_Width;
                //emission = (pow(emission, width) + pow(1 - emission, width * _Tilt)) * 0.5;
                emission = pow(emission, width);
                emission = step(0.5f, emission);
                saturate (emission);
                
                fixed4 col = tex2D(_MainTex, i.uv) + (emission * _EmissionStrength);
                return col;
            }
            ENDCG
        }
    }
}
