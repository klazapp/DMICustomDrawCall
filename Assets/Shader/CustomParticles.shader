Shader "Custom/CustomParticles"
{    
    Properties
    { 
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {        
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //GPU Instancing
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct Attributes
            {
                float4 positionOS   : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            //Declaring properties in CBUFFER
            //to make shader SRP batcher compatible
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;            
            CBUFFER_END

             //Max instanced batch size.
            float4 _Colors[1023];  

            Varyings vert(Attributes IN, uint instanceID: SV_InstanceID) 
            {
                Varyings OUT;
                
                UNITY_SETUP_INSTANCE_ID(IN);
                //Necessary only if you want to access instanced properties in the fragment Shader.
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT); 
             
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }
        
            half4 frag(Attributes IN, uint instanceID: SV_InstanceID) : SV_Target
            {
                //Returning the _BaseColor value.
                #ifdef UNITY_INSTANCING_ENABLED
                _BaseColor *= _Colors[instanceID];
                #endif
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}