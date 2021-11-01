Shader "CustomSprites/MapTile"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_OverlayTex ("Liquid Texture", 2D) = "white" {}
		_LiquidLevel("Liquid Level", Range(0,1)) = 0
		_LiquidStartHeight("Start Height", Range(0,3)) = 0.667
		_LiquidOpacity("Liquid Opacity", Range(0,1)) = 0.5
		_WaveIntensity("Wave Intensity", Range(0,0.5)) = 0.1
		_NoiseTex("Noise Texture", 2D) = "white" {}
		_SecondaryNoiseTex("Secondary Noise Texture", 2D) = "white" {}


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
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			sampler2D _OverlayTex;
			sampler2D _NoiseTex;
			sampler2D _SecondaryNoiseTex;

			float _AlphaSplitEnabled;
			float _LiquidLevel;
			float _LiquidOpacity;
			float _WaveIntensity;
			float _LiquidStartHeight;

			fixed4 SampleSpriteTexture (float2 uv)
			{
				fixed4 mainColor = tex2D (_MainTex, uv);
				float2 reverseU = float2(uv[0] * -1 + 1, uv[1]);
				float2 reverseV = float2(uv[0], uv[1]* -1 * _LiquidStartHeight + 1);
				fixed4 liquidColor = tex2D(_OverlayTex, reverseV + _Time[0]);
				fixed noise = tex2D(_NoiseTex, uv+_Time[0]).r;
				fixed secondNoise =  tex2D(_SecondaryNoiseTex, reverseU+_Time[0]).r;
				fixed liquidAlpha = liquidColor.a * _LiquidOpacity * _LiquidLevel * noise * secondNoise;



				fixed3 col = fixed3(liquidColor.rgb * liquidAlpha + mainColor.rgb * mainColor.a * (1 - liquidAlpha));
				fixed4 color = fixed4(col, liquidAlpha + mainColor.a * (1 - liquidAlpha));

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}


			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
				c.rgb *= c.a;
				return c;
			}
		ENDCG
		}
	}
}