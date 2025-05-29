Shader "Custom/UnlitFresnelGlowOnly"
{
    Properties
    {
        _RimColor ("Glow Color", Color) = (1,1,1,1)
        _RimPower ("Fresnel Power", Range(0.1,10)) = 3
        _GlowStrength ("Glow Strength", Range(0,3)) = 1.5
        _Alpha ("Fade Alpha", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            fixed4 _RimColor;
            float _RimPower;
            float _GlowStrength;
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float rim = 1.0 - saturate(dot(normalize(i.viewDir), normalize(i.worldNormal)));
                rim = pow(rim, _RimPower);
                float glow = rim * _GlowStrength;

                float3 glowColor = _RimColor.rgb * glow * _Alpha;
                float alpha = glow * _Alpha;

                return fixed4(glowColor, alpha);
            }
            ENDCG
        }
    }
}
