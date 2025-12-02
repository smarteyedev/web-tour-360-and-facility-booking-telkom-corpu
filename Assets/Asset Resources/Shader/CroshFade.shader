Shader "Custom/CrossFadeShader_NoMirror"
{
    Properties
    {
        _Texture_A ("Texture A", 2D) = "white" {}
        _Texture_B ("Texture B", 2D) = "white" {}
        _Blend ("Blend", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uvA : TEXCOORD0;
                float2 uvB : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _Texture_A;
            sampler2D _Texture_B;
            float _Blend;

            float4 _Texture_A_ST;
            float4 _Texture_B_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Transformasi UV seperti biasa
                o.uvA = TRANSFORM_TEX(v.uv, _Texture_A);
                o.uvB = TRANSFORM_TEX(v.uv, _Texture_B);

                // 💡 FIX: Balik sumbu X supaya tidak mirror
                o.uvA.x = 1.0 - o.uvA.x;
                o.uvB.x = 1.0 - o.uvB.x;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float4 colA = tex2D(_Texture_A, i.uvA);
                float4 colB = tex2D(_Texture_B, i.uvB);

                return lerp(colA, colB, _Blend);
            }
            ENDCG
        }
    }
}
