Shader "Hidden/Lighting"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LightMap ("Light Texture", 2D) = "black" {}
        _DisplacementMap ("Displacement Map", 2d) = "black" {}
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+1"
        }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _LightMap;
            sampler2D _DisplacementMap;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 disp = tex2D(_DisplacementMap, i.uv);
                fixed4 col = tex2D(_LightMap, i.uv - float2(disp.r, disp.g)); //I want to subtract the value in the displacement map from the UV position when evaluating, but don't know how to do that
                
                
                
                return fixed4(0, 0, 0, 1 - col.r);
            }
            ENDCG
        }
    }
}
