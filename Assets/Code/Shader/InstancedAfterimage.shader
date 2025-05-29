Shader "Custom/InstancedAfterimage"
{
    Properties
    {
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Fresnel Power", Range(0.1,10)) = 3
        _GlowStrength ("Glow Strength", Range(0,3)) = 1.5
        _LifeTime ("Lifetime", Float) = 1
        _SpawnTime ("Spawn Time", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Lighting Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            float _Time;
            float _LifeTime;
            float _SpawnTime;
            float4 _RimColor;
            float _RimPower;
            float _GlowStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
                float rim : TEXCOORD2;
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
                float rim = 1 - saturate(dot(normalize(o.viewDir), normalize(o.worldNormal)));
                o.rim = pow(rim, _RimPower);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float age = _Time - _SpawnTime;
                float fade = saturate(1 - (age / _LifeTime));
                float3 glow = _RimColor.rgb * i.rim * _GlowStrength;
                return fixed4(glow * fade, fade);
            }
            ENDCG
        }
    }
}
