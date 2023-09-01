Shader "Shader Graphs/SeethroughWall"
{
    Properties
    {
        [NoScaleOffset]_Main_Texture("Main Texture", 2D) = "white" {}
        _Tint("Tint", Color) = (0, 0, 0, 0)
        _Player_Position("Player Position", Vector) = (0.5, 0.5, 0, 0)
        _Size("Size", Float) = 1
        _Smoothness("Smoothness", Float) = 1
        _Opacity("Opacity", Float) = 2
        [HideInInspector]_QueueOffset("_QueueOffset", Float) = 0
        [HideInInspector]_QueueControl("_QueueControl", Float) = -1
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalUnlitSubTarget"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                // LightMode: <None>
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma instancing_options renderinglayer
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma shader_feature _ _SAMPLE_GI
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        #pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.texCoord0 = input.interp2.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_half4_half4(half4 A, half4 B, out half4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half3 BaseColor;
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_928571c7b2f04038bc24dcf84014f005_Out_0 = UnityBuildTexture2DStructNoScale(_Main_Texture);
            half4 _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0 = SAMPLE_TEXTURE2D(_Property_928571c7b2f04038bc24dcf84014f005_Out_0.tex, _Property_928571c7b2f04038bc24dcf84014f005_Out_0.samplerstate, _Property_928571c7b2f04038bc24dcf84014f005_Out_0.GetTransformedUV(IN.uv0.xy) );
            half _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_R_4 = _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0.r;
            half _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_G_5 = _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0.g;
            half _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_B_6 = _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0.b;
            half _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_A_7 = _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0.a;
            half4 _Property_5a46014f9fc14b66b0fb7a64805f7511_Out_0 = _Tint;
            half4 _Multiply_ba1911f997914e1b85ae437a7493499b_Out_2;
            Unity_Multiply_half4_half4(_SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0, _Property_5a46014f9fc14b66b0fb7a64805f7511_Out_0, _Multiply_ba1911f997914e1b85ae437a7493499b_Out_2);
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.BaseColor = (_Multiply_ba1911f997914e1b85ae437a7493499b_Out_2.xyz);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormalsOnly"
            Tags
            {
                "LightMode" = "DepthNormalsOnly"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _WRITE_RENDERING_LAYERS
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_NORMAL_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        #define _SURFACE_TYPE_TRANSPARENT 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_NORMAL_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 4.5
        #pragma exclude_renderers gles gles3 glcore
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"="UniversalUnlitSubTarget"
        }
        Pass
        {
            Name "Universal Forward"
            Tags
            {
                // LightMode: <None>
            }
        
        // Render State
        Cull Back
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma target 3.5 DOTS_INSTANCING_ON
        #pragma instancing_options renderinglayer
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma shader_feature _ _SAMPLE_GI
        #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
        #pragma multi_compile_fragment _ DEBUG_DISPLAY
        #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD0
        #define VARYINGS_NEED_POSITION_WS
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TEXCOORD0
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_UNLIT
        #define _FOG_FRAGMENT 1
        #define _SURFACE_TYPE_TRANSPARENT 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
             float4 uv0;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.texCoord0;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.texCoord0 = input.interp2.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Multiply_half4_half4(half4 A, half4 B, out half4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half3 BaseColor;
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            UnityTexture2D _Property_928571c7b2f04038bc24dcf84014f005_Out_0 = UnityBuildTexture2DStructNoScale(_Main_Texture);
            half4 _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0 = SAMPLE_TEXTURE2D(_Property_928571c7b2f04038bc24dcf84014f005_Out_0.tex, _Property_928571c7b2f04038bc24dcf84014f005_Out_0.samplerstate, _Property_928571c7b2f04038bc24dcf84014f005_Out_0.GetTransformedUV(IN.uv0.xy) );
            half _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_R_4 = _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0.r;
            half _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_G_5 = _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0.g;
            half _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_B_6 = _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0.b;
            half _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_A_7 = _SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0.a;
            half4 _Property_5a46014f9fc14b66b0fb7a64805f7511_Out_0 = _Tint;
            half4 _Multiply_ba1911f997914e1b85ae437a7493499b_Out_2;
            Unity_Multiply_half4_half4(_SampleTexture2D_2ff8e9e9d5e842f0abd52d8fa06daaf7_RGBA_0, _Property_5a46014f9fc14b66b0fb7a64805f7511_Out_0, _Multiply_ba1911f997914e1b85ae437a7493499b_Out_2);
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.BaseColor = (_Multiply_ba1911f997914e1b85ae437a7493499b_Out_2.xyz);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
            output.uv0 = input.texCoord0;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "DepthNormalsOnly"
            Tags
            {
                "LightMode" = "DepthNormalsOnly"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma target 3.5 DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define ATTRIBUTES_NEED_TEXCOORD1
        #define VARYINGS_NEED_NORMAL_WS
        #define VARYINGS_NEED_TANGENT_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHNORMALSONLY
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv1 : TEXCOORD1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
             float4 tangentWS;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float4 interp1 : INTERP1;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            output.interp1.xyzw =  input.tangentWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            output.tangentWS = input.interp1.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthNormalsOnlyPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }
        
        // Render State
        Cull Back
        ZTest LEqual
        ZWrite On
        ColorMask 0
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma target 3.5 DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define VARYINGS_NEED_NORMAL_WS
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_SHADOWCASTER
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.normalWS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.normalWS = input.interp0.xyz;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "SceneSelectionPass"
            Tags
            {
                "LightMode" = "SceneSelectionPass"
            }
        
        // Render State
        Cull Off
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma target 3.5 DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENESELECTIONPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
        Pass
        {
            Name "ScenePickingPass"
            Tags
            {
                "LightMode" = "Picking"
            }
        
        // Render State
        Cull Back
        
        // Debug
        // <None>
        
        // --------------------------------------------------
        // Pass
        
        HLSLPROGRAM
        
        // Pragmas
        #pragma target 2.0
        #pragma only_renderers gles gles3 glcore d3d11
        #pragma multi_compile_instancing
        #pragma multi_compile _ DOTS_INSTANCING_ON
        #pragma target 3.5 DOTS_INSTANCING_ON
        #pragma vertex vert
        #pragma fragment frag
        
        // Keywords
        // PassKeywords: <None>
        // GraphKeywords: <None>
        
        // Defines
        
        #define ATTRIBUTES_NEED_NORMAL
        #define ATTRIBUTES_NEED_TANGENT
        #define FEATURES_GRAPH_VERTEX
        /* WARNING: $splice Could not find named fragment 'PassInstancing' */
        #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        #define ALPHA_CLIP_THRESHOLD 1
        /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
        
        
        // custom interpolator pre-include
        /* WARNING: $splice Could not find named fragment 'sgci_CustomInterpolatorPreInclude' */
        
        // Includes
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        
        // --------------------------------------------------
        // Structs and Packing
        
        // custom interpolators pre packing
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPrePacking' */
        
        struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float2 NDCPosition;
             float2 PixelPosition;
        };
        struct VertexDescriptionInputs
        {
             float3 ObjectSpaceNormal;
             float3 ObjectSpaceTangent;
             float3 ObjectSpacePosition;
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
        PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
        // --------------------------------------------------
        // Graph
        
        // Graph Properties
        CBUFFER_START(UnityPerMaterial)
        float4 _Main_Texture_TexelSize;
        half4 _Tint;
        half2 _Player_Position;
        half _Size;
        half _Smoothness;
        half _Opacity;
        CBUFFER_END
        
        
        // Object and Global properties
        SAMPLER(SamplerState_Linear_Repeat);
        TEXTURE2D(_Main_Texture);
        SAMPLER(sampler_Main_Texture);
        
        // Graph Includes
        // GraphIncludes: <None>
        
        // -- Property used by ScenePickingPass
        #ifdef SCENEPICKINGPASS
        float4 _SelectionID;
        #endif
        
        // -- Properties used by SceneSelectionPass
        #ifdef SCENESELECTIONPASS
        int _ObjectId;
        int _PassValue;
        #endif
        
        // Graph Functions
        
        void Unity_Remap_half2(half2 In, half2 InMinMax, half2 OutMinMax, out half2 Out)
        {
            Out = OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
        }
        
        void Unity_Add_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A + B;
        }
        
        void Unity_TilingAndOffset_half(half2 UV, half2 Tiling, half2 Offset, out half2 Out)
        {
            Out = UV * Tiling + Offset;
        }
        
        void Unity_Multiply_half2_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A * B;
        }
        
        void Unity_Subtract_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A - B;
        }
        
        void Unity_Divide_half(half A, half B, out half Out)
        {
            Out = A / B;
        }
        
        void Unity_Multiply_half_half(half A, half B, out half Out)
        {
            Out = A * B;
        }
        
        void Unity_Divide_half2(half2 A, half2 B, out half2 Out)
        {
            Out = A / B;
        }
        
        void Unity_Length_half2(half2 In, out half Out)
        {
            Out = length(In);
        }
        
        void Unity_OneMinus_half(half In, out half Out)
        {
            Out = 1 - In;
        }
        
        void Unity_Saturate_half(half In, out half Out)
        {
            Out = saturate(In);
        }
        
        void Unity_Smoothstep_half(half Edge1, half Edge2, half In, out half Out)
        {
            Out = smoothstep(Edge1, Edge2, In);
        }
        
        // Custom interpolators pre vertex
        /* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */
        
        // Graph Vertex
        struct VertexDescription
        {
            half3 Position;
            half3 Normal;
            half3 Tangent;
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            description.Position = IN.ObjectSpacePosition;
            description.Normal = IN.ObjectSpaceNormal;
            description.Tangent = IN.ObjectSpaceTangent;
            return description;
        }
        
        // Custom interpolators, pre surface
        #ifdef FEATURES_GRAPH_VERTEX
        Varyings CustomInterpolatorPassThroughFunc(inout Varyings output, VertexDescription input)
        {
        return output;
        }
        #define CUSTOMINTERPOLATOR_VARYPASSTHROUGH_FUNC
        #endif
        
        // Graph Pixel
        struct SurfaceDescription
        {
            half Alpha;
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            half _Property_c363617742384058bc315a9a7cef756c_Out_0 = _Smoothness;
            half4 _ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0 = half4(IN.NDCPosition.xy, 0, 0);
            half2 _Property_61f815ccf7ec47d6a02cf157fa359945_Out_0 = _Player_Position;
            half2 _Remap_53c782c719014af89c3c3453aa5982d6_Out_3;
            Unity_Remap_half2(_Property_61f815ccf7ec47d6a02cf157fa359945_Out_0, half2 (0, 1), half2 (0.5, -1.5), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3);
            half2 _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2;
            Unity_Add_half2((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), _Remap_53c782c719014af89c3c3453aa5982d6_Out_3, _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2);
            half2 _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3;
            Unity_TilingAndOffset_half((_ScreenPosition_3ef6038f9dd74c4db9e177f345bd1c10_Out_0.xy), half2 (1, 1), _Add_d7ac6ce6b5154798964b9544e05645a3_Out_2, _TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3);
            half2 _Multiply_821177bb105d4cffad473d3992f03b13_Out_2;
            Unity_Multiply_half2_half2(_TilingAndOffset_6c2db9ff46aa4622a187c227e32343eb_Out_3, half2(2, 2), _Multiply_821177bb105d4cffad473d3992f03b13_Out_2);
            half2 _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2;
            Unity_Subtract_half2(_Multiply_821177bb105d4cffad473d3992f03b13_Out_2, half2(1, 1), _Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2);
            half _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2;
            Unity_Divide_half(unity_OrthoParams.y, unity_OrthoParams.x, _Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2);
            half _Property_0184f33afced43878b25895e51fcd1ed_Out_0 = _Size;
            half _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2;
            Unity_Multiply_half_half(_Divide_72f21ac61bd8418c84d59fc4f9011c44_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0, _Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2);
            half2 _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0 = half2(_Multiply_6b48402d92634e7992bfc38fed04ed6d_Out_2, _Property_0184f33afced43878b25895e51fcd1ed_Out_0);
            half2 _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2;
            Unity_Divide_half2(_Subtract_473f9fc0a6dd4a069cfab3b2fed3d950_Out_2, _Vector2_4b664651085441a0bcac8303e7c30c46_Out_0, _Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2);
            half _Length_9144a7737c334b16a6761c00810f7d69_Out_1;
            Unity_Length_half2(_Divide_62b608e8ae134fb18c29a44dad4cd1a1_Out_2, _Length_9144a7737c334b16a6761c00810f7d69_Out_1);
            half _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1;
            Unity_OneMinus_half(_Length_9144a7737c334b16a6761c00810f7d69_Out_1, _OneMinus_5f487bf762754922a319ddbe396e7008_Out_1);
            half _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1;
            Unity_Saturate_half(_OneMinus_5f487bf762754922a319ddbe396e7008_Out_1, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1);
            half _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3;
            Unity_Smoothstep_half(0, _Property_c363617742384058bc315a9a7cef756c_Out_0, _Saturate_817ba454e59d4f728d13272311ffc6c5_Out_1, _Smoothstep_be11557c0bae490286e613acdedc1092_Out_3);
            half _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0 = _Opacity;
            half _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2;
            Unity_Multiply_half_half(_Smoothstep_be11557c0bae490286e613acdedc1092_Out_3, _Property_b7e2908ab6f7499cbb1d1f456b3f071e_Out_0, _Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2);
            half _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            Unity_OneMinus_half(_Multiply_a6d992c0b43c4ce2b65ed366640b0b94_Out_2, _OneMinus_ca1b709128044741ad67ed361456905d_Out_1);
            surface.Alpha = _OneMinus_ca1b709128044741ad67ed361456905d_Out_1;
            return surface;
        }
        
        // --------------------------------------------------
        // Build Graph Inputs
        #ifdef HAVE_VFX_MODIFICATION
        #define VFX_SRP_ATTRIBUTES Attributes
        #define VFX_SRP_VARYINGS Varyings
        #define VFX_SRP_SURFACE_INPUTS SurfaceDescriptionInputs
        #endif
        VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
        {
            VertexDescriptionInputs output;
            ZERO_INITIALIZE(VertexDescriptionInputs, output);
        
            output.ObjectSpaceNormal =                          input.normalOS;
            output.ObjectSpaceTangent =                         input.tangentOS.xyz;
            output.ObjectSpacePosition =                        input.positionOS;
        
            return output;
        }
        SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
        #ifdef HAVE_VFX_MODIFICATION
            // FragInputs from VFX come from two places: Interpolator or CBuffer.
            /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */
        
        #endif
        
            
        
        
        
        
        
        
            #if UNITY_UV_STARTS_AT_TOP
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x < 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #else
            output.PixelPosition = float2(input.positionCS.x, (_ProjectionParams.x > 0) ? (_ScaledScreenParams.y - input.positionCS.y) : input.positionCS.y);
            #endif
        
            output.NDCPosition = output.PixelPosition.xy / _ScaledScreenParams.xy;
            output.NDCPosition.y = 1.0f - output.NDCPosition.y;
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
                return output;
        }
        
        // --------------------------------------------------
        // Main
        
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/SelectionPickingPass.hlsl"
        
        // --------------------------------------------------
        // Visual Effect Vertex Invocations
        #ifdef HAVE_VFX_MODIFICATION
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
        #endif
        
        ENDHLSL
        }
    }
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    CustomEditorForRenderPipeline "UnityEditor.ShaderGraphUnlitGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    FallBack "Hidden/Shader Graph/FallbackError"
}