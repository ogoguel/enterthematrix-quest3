
// Copyright (c) Olivier Goguel 2024
// Licensed under the MIT License.
 
// Based on code from https://de.linkedin.com/in/shahriar-shahrabi


Shader "MatrixShader"
{
    Properties
    {
        _Font ("Font", 2D) = "black" {}
        _White_Noise ("WhiteNoise", 2D) = "black" {}
        _Transparency ("Transparency", Float) = 0.4
    }
    
    SubShader
    {
       Tags {
           "Queue" = "Transparent"
           "RenderType" = "Transparent"
       }
        LOD 100
      //  No Additive Blending on QUEST :(
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
       
             #pragma multi_compile __ UNITY_EDITOR

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f
            {
                float4 vertex    : SV_POSITION;
                float3 worldPos  : TEXCOORD0;
                float3 normal    : NORMAL;
                float4 screenPosition : TEXCOORD2;
            };
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex    = TransformObjectToHClip(v.vertex);
                o.worldPos  = mul(unity_ObjectToWorld, v.vertex);
                o.normal    = TransformObjectToWorldNormal(v.normal);
                o.screenPosition = ComputeScreenPos(o.vertex);
                return o;
            }

            sampler2D _Font;
            sampler2D _White_Noise;
          
            float _Rain_Speed = 2;
            float _Rain_Scale = 1;
            float _Rain_Fade = 5;
            float _Transparency;
            float _World_Angle;
            
            uint _session_rand_seed; 
            #define rnd(seed, constant)  wang_rnd(seed +triple32(_session_rand_seed) * constant)
            #define sharpness 10.
            #define dropLength 512

            inline float text(float2 coord, sampler2D fontTexture, sampler2D whiteNoise)
            {
                float2 uv    = frac (coord.xy/ 16.);         
                float2 block = floor(coord.xy/ 16.);               
                uv    = uv * 0.7 + .1;                  
                
                float2 rand  = tex2D(whiteNoise,      
                block.xy/float2(512.,512.)).xy;  
                
                rand  = floor(rand*16.);                    
                uv   += rand;                               
                uv   *= 0.0625;                            
                uv.x  = -uv.x;
                return tex2D(fontTexture, uv).r;
            }
            
            inline float3 rain(float2 fragCoord, float rainSpeed, float rainFade)
            {
                fragCoord.x  = floor(fragCoord.x/ 16.);         
                
                float offset = sin (fragCoord.x*15.);            
                float speed  = cos (fragCoord.x*3.)*.15 + .35;
                speed *= rainSpeed;
                float y      =  frac((fragCoord.y / dropLength)  + _Time.y * speed + offset);                  
                return float3(.1, 1., .35) / (y*rainFade);              
            }
            
            inline float3 MatrixEffect(float2 coord, float rainScale, float rainSpeed, float rainFade,  sampler2D fontTexture, sampler2D whiteNoise)
            {
                float3      col = float3(0., 0., 0.);
                float3 rain_col = rain(coord* float2(dropLength, dropLength)*rainScale, rainSpeed, rainFade);
                return  text(coord * float2(dropLength, dropLength)*rainScale, fontTexture, whiteNoise) *rain_col;
            }

            inline float3 RotateAroundYInDegrees (float3 vertex, float degrees)
            {
                float alpha = degrees * 3.14159265359 / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float3x3 m = float3x3(cosa, 0, sina, 0, 1, 0, -sina, 0, cosa); 
                return mul(m, vertex);
            }

            real4 frag (v2f i) : SV_Target
            {
                real4 col = real4(0.,1.,0.,1.);
                float2 front = i.worldPos.xy ;
                float2 side = i.worldPos.zy ;
                float3 w = RotateAroundYInDegrees(i.worldPos,_World_Angle);
                float2 top = w.xz ;
              
                float3 colFront = MatrixEffect(front,_Rain_Scale, _Rain_Speed, _Rain_Fade,_Font,_White_Noise);
                float3 colSide  = MatrixEffect(side,_Rain_Scale, _Rain_Speed, _Rain_Fade,_Font,_White_Noise);
                float3 colTop   = MatrixEffect(top,_Rain_Scale, _Rain_Speed, _Rain_Fade,_Font,_White_Noise);

                float3 blendWeight  = pow(normalize(abs(i.normal)), sharpness);
                blendWeight /= (blendWeight.x+ blendWeight.y+ blendWeight.z);
                col.xyz      = colFront * blendWeight.z +  colSide  * blendWeight.x + colTop   * blendWeight.y;

                col = clamp(col,0,1);
                col.a = _Transparency;
                return col;
            }
            ENDHLSL
        }
    }
}