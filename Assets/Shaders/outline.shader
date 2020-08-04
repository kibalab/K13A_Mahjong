Shader "Mingyu/Outline"
{
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Main Texture", 2D) = "white" {}
		_Outline("Outline", Float) = 0.1
		_OutlineColor("Outline Color", Color) = (1,1,1,1)
	}

		SubShader
		{
			Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

			// 외곽선 그리기
			Pass
			{
				Blend SrcAlpha OneMinusSrcAlpha
				Cull Front // 뒷면만 그리기
				ZWrite Off

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				half _Outline;
				half4 _OutlineColor;

				struct vertexInput
				{
					float4 vertex: POSITION;
				};

				struct vertexOutput
				{
					float4 pos: SV_POSITION;
				};

				float4 CreateOutline(float4 vertPos, float Outline)
				{
					float4x4 scaleMat;
					scaleMat[0][0] = 1.0f + Outline;
					scaleMat[0][1] = 0.0f;
					scaleMat[0][2] = 0.0f;
					scaleMat[0][3] = 0.0f;
					scaleMat[1][0] = 0.0f;
					scaleMat[1][1] = 1.0f + Outline;
					scaleMat[1][2] = 0.0f;
					scaleMat[1][3] = 0.0f;
					scaleMat[2][0] = 0.0f;
					scaleMat[2][1] = 0.0f;
					scaleMat[2][2] = 1.0f + Outline;
					scaleMat[2][3] = 0.0f;
					scaleMat[3][0] = 0.0f;
					scaleMat[3][1] = 0.0f;
					scaleMat[3][2] = 0.0f;
					scaleMat[3][3] = 1.0f;

					return mul(scaleMat, vertPos);
				}

				vertexOutput vert(vertexInput v)
				{
					vertexOutput o;

					o.pos = UnityObjectToClipPos(CreateOutline(v.vertex, _Outline));

					return o;
				}

				half4 frag(vertexOutput i) : COLOR
				{
					return _OutlineColor;
				}

				ENDCG
			}

			// 정상적으로 그리기
			Pass
			{
				Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag

				half4 _Color;
				sampler2D _MainTex;
				float4 _MainTex_ST;



				struct vertexInput
				{
					float4 vertex: POSITION;
					float4 texcoord: TEXCOORD0;
				};

				struct vertexOutput
				{
					float4 pos: SV_POSITION;
					float4 texcoord: TEXCOORD0;
				};

				vertexOutput vert(vertexInput v)
				{

					vertexOutput o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.texcoord.xy = (v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw);
					return o;
				}

				half4 frag(vertexOutput i) : COLOR
				{
					return tex2D(_MainTex, i.texcoord) * _Color;
				}

				ENDCG
			}

		}
}