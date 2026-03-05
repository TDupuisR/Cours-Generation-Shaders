Shader "Unlit/FeurShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AltTex ("Texture", 2D) = "white" {}
        progress ("Progress", Range(0, 1)) = 0.5
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        Tags { "Queue" = "Transparent" }
        Tags { "RenderType" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual

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
            sampler2D _AltTex;
            float4 _MainTex_ST;
            float progress;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(1, 0, 0, 0.3);
                
                float4 col = tex2D(_MainTex, i.uv);
                float4 col2 = tex2D(_AltTex, i.uv);
                //col.rgb *= col2.rgb;
                
                float anim = (sin(_Time.y) + 1) / 2;
                //float t = step(anim, i.uv.x);
                //col = col * t + float4(0, 1, 0, 0) * (1 - t);
                
                //return col;
                if (col2.r < anim)
                {
                    col.a = 0;
                }
                return col;
            }
            ENDCG
        }
    }
}
