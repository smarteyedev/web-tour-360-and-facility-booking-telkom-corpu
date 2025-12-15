// Stencil Injection by ShaderGraphStencilInjector

Shader "Stencil Shader Graph/Blur7"
{
Properties
{
_StencilComp("_StencilComp", Float) = 0
_Stencil("_Stencil", Float) = 0
_StencilOp("_StencilOp", Float) = 0
_StencilWriteMask("_StencilWriteMask", Float) = 0
_StencilReadMask("_StencilReadMask", Float) = 0
_ColorMask("_ColorMask", Float) = 0
_RadiusTL("RadiusTL", Float) = 0
_RadiusTR("RadiusTR", Float) = 0
_RadiusBL("RadiusBL", Float) = 0
_RadiusBR("RadiusBR", Float) = 0
_height("height", Float) = 1
_widht("widht", Float) = 0.05
_PositionOffset("PositionOffset", Vector) = (0, 0, 0, 0)
_AspectRatio("AspectRatio", Float) = 0
_BlurX("BlurX", Range(0, 0.015)) = 0
_BlurY("BlurY", Range(0, 0.015)) = 0

        // Stencil Properties
        [IntRange] _StencilRef ("Stencil Reference Value", Range(0, 255)) = 0
        [IntRange] _StencilReadMask ("Stencil ReadMask Value", Range(0, 255)) = 255
        [IntRange] _StencilWriteMask ("Stencil WriteMask Value", Range(0, 255)) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Stencil Pass Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail Op", Float) = 0
        [Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail Op", Float) = 0
        [Enum(Off,0,On,1)] _StencilEnabled ("Stencil Enabled", Float) = 0
}
SubShader
{
Tags
{
"RenderPipeline"="UniversalPipeline"
"RenderType"="Transparent"
"UniversalMaterialType" = "Unlit"
"Queue"="Transparent"
// DisableBatching: <None>
"ShaderGraphShader"="true"
"ShaderGraphTargetId"="UniversalSpriteUnlitSubTarget"
}
Pass
{
    Name "Sprite Unlit"
    Tags
    {
        "LightMode" = "Universal2D"
    }

// Render State
Cull Off
Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
ZTest LEqual
ZWrite Off

        // Stencil Buffer Setup
        Stencil
        {
            Ref [_StencilRef]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            Comp [_StencilComp]
            Pass [_StencilPass]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
        }

// Debug
// <None>

// --------------------------------------------------
// Pass

HLSLPROGRAM

// Pragmas
#pragma target 2.0
#pragma exclude_renderers d3d11_9x
#pragma vertex vert
#pragma fragment frag

// Keywords
#pragma multi_compile_fragment _ DEBUG_DISPLAY
// GraphKeywords: <None>

// Defines

#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define ATTRIBUTES_NEED_COLOR
#define VARYINGS_NEED_POSITION_WS
#define VARYINGS_NEED_TEXCOORD0
#define VARYINGS_NEED_COLOR
#define FEATURES_GRAPH_VERTEX
/* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_SPRITEUNLIT
#define ALPHA_CLIP_THRESHOLD 1
#define REQUIRE_OPAQUE_TEXTURE


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
 float4 uv0 : TEXCOORD0;
 float4 color : COLOR;
#if UNITY_ANY_INSTANCING_ENABLED
 uint instanceID : INSTANCEID_SEMANTIC;
#endif
};
struct Varyings
{
 float4 positionCS : SV_POSITION;
 float3 positionWS;
 float4 texCoord0;
 float4 color;
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
 float4 texCoord0 : INTERP0;
 float4 color : INTERP1;
 float3 positionWS : INTERP2;
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
output.texCoord0.xyzw = input.texCoord0;
output.color.xyzw = input.color;
output.positionWS.xyz = input.positionWS;
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
output.texCoord0 = input.texCoord0.xyzw;
output.color = input.color.xyzw;
output.positionWS = input.positionWS.xyz;
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
float _StencilComp;
float _Stencil;
float _StencilOp;
float _StencilWriteMask;
float _StencilReadMask;
float _ColorMask;
float _RadiusTL;
float _RadiusTR;
float _RadiusBL;
float _RadiusBR;
float _height;
float _widht;
float2 _PositionOffset;
float _AspectRatio;
float _BlurX;
float _BlurY;
CBUFFER_END


// Object and Global properties

// Graph Includes
#include "Assets/Asset Resources/Shader/BlurColor.hlsl"

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

void Unity_SceneColor_float(float4 UV, out float3 Out)
{
    Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
}

void Unity_Add_float3(float3 A, float3 B, out float3 Out)
{
    Out = A + B;
}

void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
{
    Out = A / B;
}

void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
{
    Out = A - B;
}

void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
{
Out = A * B;
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

void Unity_Multiply_float_float(float A, float B, out float Out)
{
Out = A * B;
}

void Unity_RoundedRectangle_float(float2 UV, float Width, float Height, float Radius, out float Out)
{
    Radius = max(min(min(abs(Radius * 2), abs(Width)), abs(Height)), 1e-5);
    float2 uv = abs(UV * 2 - 1) - float2(Width, Height) + Radius;
    float d = length(max(0, uv)) / Radius;
#if defined(SHADER_STAGE_RAY_TRACING)
    Out = saturate((1 - d) * 1e7);
#else
    float fwd = max(fwidth(d), 1e-5);
    Out = saturate((1 - d) / fwd);
#endif
}

// Custom interpolators pre vertex
/* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

// Graph Vertex
struct VertexDescription
{
float3 Position;
float3 Normal;
float3 Tangent;
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
float3 BaseColor;
float Alpha;
float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
SurfaceDescription surface = (SurfaceDescription)0;
float4 _ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4 = float4(IN.NDCPosition.xy, 0, 0);
float _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float = _BlurX;
float _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float = _BlurY;
float4 _BlurColorCustomFunction_95b3c231029f452eac6f36dcd9ea2cff_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 0, 1, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_95b3c231029f452eac6f36dcd9ea2cff_New_5_Vector4);
float3 _SceneColor_24637a447e0b43e684b93da232333e1f_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_95b3c231029f452eac6f36dcd9ea2cff_New_5_Vector4, _SceneColor_24637a447e0b43e684b93da232333e1f_Out_1_Vector3);
float4 _BlurColorCustomFunction_735154d1ca8a429d96253bbfa2ce1776_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 1, 2, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_735154d1ca8a429d96253bbfa2ce1776_New_5_Vector4);
float3 _SceneColor_650174354c11437fbbb4c54960a31ab5_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_735154d1ca8a429d96253bbfa2ce1776_New_5_Vector4, _SceneColor_650174354c11437fbbb4c54960a31ab5_Out_1_Vector3);
float3 _Add_4ff1aab325ae4a1390061c7820c4c33e_Out_2_Vector3;
Unity_Add_float3(_SceneColor_24637a447e0b43e684b93da232333e1f_Out_1_Vector3, _SceneColor_650174354c11437fbbb4c54960a31ab5_Out_1_Vector3, _Add_4ff1aab325ae4a1390061c7820c4c33e_Out_2_Vector3);
float4 _BlurColorCustomFunction_c7a2c2bb45204613ad8e0f9740715af7_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 2, 3, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_c7a2c2bb45204613ad8e0f9740715af7_New_5_Vector4);
float3 _SceneColor_9d05ab8a95cd46d88012aad49336fe8b_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_c7a2c2bb45204613ad8e0f9740715af7_New_5_Vector4, _SceneColor_9d05ab8a95cd46d88012aad49336fe8b_Out_1_Vector3);
float4 _BlurColorCustomFunction_487413fc3bf54796b6aef264cbd59347_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 3, 4, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_487413fc3bf54796b6aef264cbd59347_New_5_Vector4);
float3 _SceneColor_374d6260655b43409d9d0e63c7c79a88_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_487413fc3bf54796b6aef264cbd59347_New_5_Vector4, _SceneColor_374d6260655b43409d9d0e63c7c79a88_Out_1_Vector3);
float3 _Add_699ba07f2fab4d84b956a17fbcee52dd_Out_2_Vector3;
Unity_Add_float3(_SceneColor_9d05ab8a95cd46d88012aad49336fe8b_Out_1_Vector3, _SceneColor_374d6260655b43409d9d0e63c7c79a88_Out_1_Vector3, _Add_699ba07f2fab4d84b956a17fbcee52dd_Out_2_Vector3);
float3 _Add_1155a9c6d2a14b8aa1f18164cffe9139_Out_2_Vector3;
Unity_Add_float3(_Add_4ff1aab325ae4a1390061c7820c4c33e_Out_2_Vector3, _Add_699ba07f2fab4d84b956a17fbcee52dd_Out_2_Vector3, _Add_1155a9c6d2a14b8aa1f18164cffe9139_Out_2_Vector3);
float4 _BlurColorCustomFunction_31eb67e3509f40c7949324e328b1e223_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 4, 5, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_31eb67e3509f40c7949324e328b1e223_New_5_Vector4);
float3 _SceneColor_8af4a0bdf2a44a1b9126ee218017b3de_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_31eb67e3509f40c7949324e328b1e223_New_5_Vector4, _SceneColor_8af4a0bdf2a44a1b9126ee218017b3de_Out_1_Vector3);
float4 _BlurColorCustomFunction_843eb9816bb5487dac6c1f061f1f9774_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 5, 6, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_843eb9816bb5487dac6c1f061f1f9774_New_5_Vector4);
float3 _SceneColor_225943d859cd4704a8287606dc43bd2f_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_843eb9816bb5487dac6c1f061f1f9774_New_5_Vector4, _SceneColor_225943d859cd4704a8287606dc43bd2f_Out_1_Vector3);
float3 _Add_98bfb3fbfb62465889530ca94a0e7555_Out_2_Vector3;
Unity_Add_float3(_SceneColor_8af4a0bdf2a44a1b9126ee218017b3de_Out_1_Vector3, _SceneColor_225943d859cd4704a8287606dc43bd2f_Out_1_Vector3, _Add_98bfb3fbfb62465889530ca94a0e7555_Out_2_Vector3);
float3 _Add_249001cfc28a468fbefbacfabf81db80_Out_2_Vector3;
Unity_Add_float3(_Add_1155a9c6d2a14b8aa1f18164cffe9139_Out_2_Vector3, _Add_98bfb3fbfb62465889530ca94a0e7555_Out_2_Vector3, _Add_249001cfc28a468fbefbacfabf81db80_Out_2_Vector3);
float _Float_2409cbe1f6ec4748981cb52c9b6fe0b6_Out_0_Float = 6;
float3 _Divide_dd2cf632c93945f4a8f609cfb33d2779_Out_2_Vector3;
Unity_Divide_float3(_Add_249001cfc28a468fbefbacfabf81db80_Out_2_Vector3, (_Float_2409cbe1f6ec4748981cb52c9b6fe0b6_Out_0_Float.xxx), _Divide_dd2cf632c93945f4a8f609cfb33d2779_Out_2_Vector3);
float2 _Property_5578a3855e8a44719d5d9b6e095765db_Out_0_Vector2 = _PositionOffset;
float4 _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4 = IN.uv0;
float2 _Vector2_c5f34fb5b59a44a2b980fca5d34d5ac7_Out_0_Vector2 = float2(0.5, 0.5);
float2 _Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2;
Unity_Subtract_float2((_UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4.xy), _Vector2_c5f34fb5b59a44a2b980fca5d34d5ac7_Out_0_Vector2, _Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2);
float _Property_3ade7a0cffd243598721cdea7f6e0006_Out_0_Float = _AspectRatio;
float2 _Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2;
Unity_Multiply_float2_float2(_Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2, (_Property_3ade7a0cffd243598721cdea7f6e0006_Out_0_Float.xx), _Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2);
float _Split_4c910cd1ca0d4824affdec5b95a21770_R_1_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[0];
float _Split_4c910cd1ca0d4824affdec5b95a21770_G_2_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[1];
float _Split_4c910cd1ca0d4824affdec5b95a21770_B_3_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[2];
float _Split_4c910cd1ca0d4824affdec5b95a21770_A_4_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[3];
float2 _Vector2_dfc49d6ff87e4a44893b8d697a64631b_Out_0_Vector2 = float2((_Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2).x, _Split_4c910cd1ca0d4824affdec5b95a21770_G_2_Float);
float2 _Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2;
Unity_Add_float2(_Property_5578a3855e8a44719d5d9b6e095765db_Out_0_Vector2, _Vector2_dfc49d6ff87e4a44893b8d697a64631b_Out_0_Vector2, _Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2);
float _Property_a5e47424cc81499184a3e241c5220179_Out_0_Float = _widht;
float _Property_457c23cec6fb477abb811b46b7efde37_Out_0_Float = _height;
float _Property_9448c79e185f4c3db9ff6a327b1ef50f_Out_0_Float = _RadiusBL;
float _Property_e7e742ecaa374eb0a6b7545569d39d0c_Out_0_Float = _RadiusBR;
float4 _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4 = IN.uv0;
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[0];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_G_2_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[1];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_B_3_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[2];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_A_4_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[3];
float _Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float;
Unity_Lerp_float(_Property_9448c79e185f4c3db9ff6a327b1ef50f_Out_0_Float, _Property_e7e742ecaa374eb0a6b7545569d39d0c_Out_0_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float, _Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float);
float _Property_e9b2750607524cf68340b65b9c1e831d_Out_0_Float = _RadiusTL;
float _Property_7e4835b76a244b7d9c4f9d13b512c1ac_Out_0_Float = _RadiusTR;
float _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float;
Unity_Lerp_float(_Property_e9b2750607524cf68340b65b9c1e831d_Out_0_Float, _Property_7e4835b76a244b7d9c4f9d13b512c1ac_Out_0_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float, _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float);
float _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float;
Unity_Lerp_float(_Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float, _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_G_2_Float, _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float);
float _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float;
Unity_Multiply_float_float(_Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float, _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float, _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float);
float _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float;
Unity_RoundedRectangle_float(_Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2, _Property_a5e47424cc81499184a3e241c5220179_Out_0_Float, _Property_457c23cec6fb477abb811b46b7efde37_Out_0_Float, _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float, _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float);
surface.BaseColor = _Divide_dd2cf632c93945f4a8f609cfb33d2779_Out_2_Vector3;
surface.Alpha = _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float;
surface.AlphaClipThreshold = 0;
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
#if VFX_USE_GRAPH_VALUES
    uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
    /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
#endif
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
#include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

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

        Stencil
        {
            Ref [_StencilRef]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            Comp [_StencilComp]
            Pass [_StencilPass]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
        }

// Debug
// <None>

// --------------------------------------------------
// Pass

HLSLPROGRAM

// Pragmas
#pragma target 2.0
#pragma exclude_renderers d3d11_9x
#pragma vertex vert
#pragma fragment frag

// Keywords
// PassKeywords: <None>
// GraphKeywords: <None>

// Defines

#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define VARYINGS_NEED_TEXCOORD0
#define FEATURES_GRAPH_VERTEX
/* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_DEPTHONLY
#define SCENESELECTIONPASS 1

#define _ALPHATEST_ON 1


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
 float4 uv0 : TEXCOORD0;
#if UNITY_ANY_INSTANCING_ENABLED
 uint instanceID : INSTANCEID_SEMANTIC;
#endif
};
struct Varyings
{
 float4 positionCS : SV_POSITION;
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
 float4 texCoord0 : INTERP0;
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
output.texCoord0.xyzw = input.texCoord0;
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
output.texCoord0 = input.texCoord0.xyzw;
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
float _StencilComp;
float _Stencil;
float _StencilOp;
float _StencilWriteMask;
float _StencilReadMask;
float _ColorMask;
float _RadiusTL;
float _RadiusTR;
float _RadiusBL;
float _RadiusBR;
float _height;
float _widht;
float2 _PositionOffset;
float _AspectRatio;
float _BlurX;
float _BlurY;
CBUFFER_END


// Object and Global properties

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

void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
{
    Out = A - B;
}

void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
{
Out = A * B;
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

void Unity_Multiply_float_float(float A, float B, out float Out)
{
Out = A * B;
}

void Unity_RoundedRectangle_float(float2 UV, float Width, float Height, float Radius, out float Out)
{
    Radius = max(min(min(abs(Radius * 2), abs(Width)), abs(Height)), 1e-5);
    float2 uv = abs(UV * 2 - 1) - float2(Width, Height) + Radius;
    float d = length(max(0, uv)) / Radius;
#if defined(SHADER_STAGE_RAY_TRACING)
    Out = saturate((1 - d) * 1e7);
#else
    float fwd = max(fwidth(d), 1e-5);
    Out = saturate((1 - d) / fwd);
#endif
}

// Custom interpolators pre vertex
/* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

// Graph Vertex
struct VertexDescription
{
float3 Position;
float3 Normal;
float3 Tangent;
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
float Alpha;
float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
SurfaceDescription surface = (SurfaceDescription)0;
float2 _Property_5578a3855e8a44719d5d9b6e095765db_Out_0_Vector2 = _PositionOffset;
float4 _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4 = IN.uv0;
float2 _Vector2_c5f34fb5b59a44a2b980fca5d34d5ac7_Out_0_Vector2 = float2(0.5, 0.5);
float2 _Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2;
Unity_Subtract_float2((_UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4.xy), _Vector2_c5f34fb5b59a44a2b980fca5d34d5ac7_Out_0_Vector2, _Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2);
float _Property_3ade7a0cffd243598721cdea7f6e0006_Out_0_Float = _AspectRatio;
float2 _Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2;
Unity_Multiply_float2_float2(_Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2, (_Property_3ade7a0cffd243598721cdea7f6e0006_Out_0_Float.xx), _Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2);
float _Split_4c910cd1ca0d4824affdec5b95a21770_R_1_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[0];
float _Split_4c910cd1ca0d4824affdec5b95a21770_G_2_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[1];
float _Split_4c910cd1ca0d4824affdec5b95a21770_B_3_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[2];
float _Split_4c910cd1ca0d4824affdec5b95a21770_A_4_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[3];
float2 _Vector2_dfc49d6ff87e4a44893b8d697a64631b_Out_0_Vector2 = float2((_Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2).x, _Split_4c910cd1ca0d4824affdec5b95a21770_G_2_Float);
float2 _Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2;
Unity_Add_float2(_Property_5578a3855e8a44719d5d9b6e095765db_Out_0_Vector2, _Vector2_dfc49d6ff87e4a44893b8d697a64631b_Out_0_Vector2, _Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2);
float _Property_a5e47424cc81499184a3e241c5220179_Out_0_Float = _widht;
float _Property_457c23cec6fb477abb811b46b7efde37_Out_0_Float = _height;
float _Property_9448c79e185f4c3db9ff6a327b1ef50f_Out_0_Float = _RadiusBL;
float _Property_e7e742ecaa374eb0a6b7545569d39d0c_Out_0_Float = _RadiusBR;
float4 _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4 = IN.uv0;
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[0];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_G_2_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[1];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_B_3_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[2];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_A_4_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[3];
float _Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float;
Unity_Lerp_float(_Property_9448c79e185f4c3db9ff6a327b1ef50f_Out_0_Float, _Property_e7e742ecaa374eb0a6b7545569d39d0c_Out_0_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float, _Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float);
float _Property_e9b2750607524cf68340b65b9c1e831d_Out_0_Float = _RadiusTL;
float _Property_7e4835b76a244b7d9c4f9d13b512c1ac_Out_0_Float = _RadiusTR;
float _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float;
Unity_Lerp_float(_Property_e9b2750607524cf68340b65b9c1e831d_Out_0_Float, _Property_7e4835b76a244b7d9c4f9d13b512c1ac_Out_0_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float, _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float);
float _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float;
Unity_Lerp_float(_Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float, _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_G_2_Float, _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float);
float _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float;
Unity_Multiply_float_float(_Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float, _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float, _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float);
float _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float;
Unity_RoundedRectangle_float(_Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2, _Property_a5e47424cc81499184a3e241c5220179_Out_0_Float, _Property_457c23cec6fb477abb811b46b7efde37_Out_0_Float, _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float, _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float);
surface.Alpha = _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float;
surface.AlphaClipThreshold = 0;
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
#if VFX_USE_GRAPH_VALUES
    uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
    /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
#endif
    /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */

#endif

    






    #if UNITY_UV_STARTS_AT_TOP
    #else
    #endif


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

        Stencil
        {
            Ref [_StencilRef]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            Comp [_StencilComp]
            Pass [_StencilPass]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
        }

// Debug
// <None>

// --------------------------------------------------
// Pass

HLSLPROGRAM

// Pragmas
#pragma target 2.0
#pragma exclude_renderers d3d11_9x
#pragma vertex vert
#pragma fragment frag

// Keywords
// PassKeywords: <None>
// GraphKeywords: <None>

// Defines

#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define VARYINGS_NEED_TEXCOORD0
#define FEATURES_GRAPH_VERTEX
/* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_DEPTHONLY
#define SCENEPICKINGPASS 1

#define _ALPHATEST_ON 1


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
 float4 uv0 : TEXCOORD0;
#if UNITY_ANY_INSTANCING_ENABLED
 uint instanceID : INSTANCEID_SEMANTIC;
#endif
};
struct Varyings
{
 float4 positionCS : SV_POSITION;
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
 float4 texCoord0 : INTERP0;
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
output.texCoord0.xyzw = input.texCoord0;
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
output.texCoord0 = input.texCoord0.xyzw;
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
float _StencilComp;
float _Stencil;
float _StencilOp;
float _StencilWriteMask;
float _StencilReadMask;
float _ColorMask;
float _RadiusTL;
float _RadiusTR;
float _RadiusBL;
float _RadiusBR;
float _height;
float _widht;
float2 _PositionOffset;
float _AspectRatio;
float _BlurX;
float _BlurY;
CBUFFER_END


// Object and Global properties

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

void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
{
    Out = A - B;
}

void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
{
Out = A * B;
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

void Unity_Multiply_float_float(float A, float B, out float Out)
{
Out = A * B;
}

void Unity_RoundedRectangle_float(float2 UV, float Width, float Height, float Radius, out float Out)
{
    Radius = max(min(min(abs(Radius * 2), abs(Width)), abs(Height)), 1e-5);
    float2 uv = abs(UV * 2 - 1) - float2(Width, Height) + Radius;
    float d = length(max(0, uv)) / Radius;
#if defined(SHADER_STAGE_RAY_TRACING)
    Out = saturate((1 - d) * 1e7);
#else
    float fwd = max(fwidth(d), 1e-5);
    Out = saturate((1 - d) / fwd);
#endif
}

// Custom interpolators pre vertex
/* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

// Graph Vertex
struct VertexDescription
{
float3 Position;
float3 Normal;
float3 Tangent;
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
float Alpha;
float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
SurfaceDescription surface = (SurfaceDescription)0;
float2 _Property_5578a3855e8a44719d5d9b6e095765db_Out_0_Vector2 = _PositionOffset;
float4 _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4 = IN.uv0;
float2 _Vector2_c5f34fb5b59a44a2b980fca5d34d5ac7_Out_0_Vector2 = float2(0.5, 0.5);
float2 _Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2;
Unity_Subtract_float2((_UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4.xy), _Vector2_c5f34fb5b59a44a2b980fca5d34d5ac7_Out_0_Vector2, _Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2);
float _Property_3ade7a0cffd243598721cdea7f6e0006_Out_0_Float = _AspectRatio;
float2 _Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2;
Unity_Multiply_float2_float2(_Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2, (_Property_3ade7a0cffd243598721cdea7f6e0006_Out_0_Float.xx), _Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2);
float _Split_4c910cd1ca0d4824affdec5b95a21770_R_1_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[0];
float _Split_4c910cd1ca0d4824affdec5b95a21770_G_2_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[1];
float _Split_4c910cd1ca0d4824affdec5b95a21770_B_3_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[2];
float _Split_4c910cd1ca0d4824affdec5b95a21770_A_4_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[3];
float2 _Vector2_dfc49d6ff87e4a44893b8d697a64631b_Out_0_Vector2 = float2((_Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2).x, _Split_4c910cd1ca0d4824affdec5b95a21770_G_2_Float);
float2 _Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2;
Unity_Add_float2(_Property_5578a3855e8a44719d5d9b6e095765db_Out_0_Vector2, _Vector2_dfc49d6ff87e4a44893b8d697a64631b_Out_0_Vector2, _Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2);
float _Property_a5e47424cc81499184a3e241c5220179_Out_0_Float = _widht;
float _Property_457c23cec6fb477abb811b46b7efde37_Out_0_Float = _height;
float _Property_9448c79e185f4c3db9ff6a327b1ef50f_Out_0_Float = _RadiusBL;
float _Property_e7e742ecaa374eb0a6b7545569d39d0c_Out_0_Float = _RadiusBR;
float4 _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4 = IN.uv0;
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[0];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_G_2_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[1];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_B_3_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[2];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_A_4_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[3];
float _Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float;
Unity_Lerp_float(_Property_9448c79e185f4c3db9ff6a327b1ef50f_Out_0_Float, _Property_e7e742ecaa374eb0a6b7545569d39d0c_Out_0_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float, _Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float);
float _Property_e9b2750607524cf68340b65b9c1e831d_Out_0_Float = _RadiusTL;
float _Property_7e4835b76a244b7d9c4f9d13b512c1ac_Out_0_Float = _RadiusTR;
float _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float;
Unity_Lerp_float(_Property_e9b2750607524cf68340b65b9c1e831d_Out_0_Float, _Property_7e4835b76a244b7d9c4f9d13b512c1ac_Out_0_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float, _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float);
float _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float;
Unity_Lerp_float(_Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float, _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_G_2_Float, _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float);
float _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float;
Unity_Multiply_float_float(_Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float, _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float, _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float);
float _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float;
Unity_RoundedRectangle_float(_Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2, _Property_a5e47424cc81499184a3e241c5220179_Out_0_Float, _Property_457c23cec6fb477abb811b46b7efde37_Out_0_Float, _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float, _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float);
surface.Alpha = _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float;
surface.AlphaClipThreshold = 0;
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
#if VFX_USE_GRAPH_VALUES
    uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
    /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
#endif
    /* WARNING: $splice Could not find named fragment 'VFXSetFragInputs' */

#endif

    






    #if UNITY_UV_STARTS_AT_TOP
    #else
    #endif


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
    Name "Sprite Unlit"
    Tags
    {
        "LightMode" = "UniversalForward"
    }

// Render State
Cull Off
Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
ZTest LEqual
ZWrite Off

        Stencil
        {
            Ref [_StencilRef]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
            Comp [_StencilComp]
            Pass [_StencilPass]
            Fail [_StencilFail]
            ZFail [_StencilZFail]
        }

// Debug
// <None>

// --------------------------------------------------
// Pass

HLSLPROGRAM

// Pragmas
#pragma target 2.0
#pragma exclude_renderers d3d11_9x
#pragma vertex vert
#pragma fragment frag

// Keywords
#pragma multi_compile_fragment _ DEBUG_DISPLAY
// GraphKeywords: <None>

// Defines

#define ATTRIBUTES_NEED_NORMAL
#define ATTRIBUTES_NEED_TANGENT
#define ATTRIBUTES_NEED_TEXCOORD0
#define ATTRIBUTES_NEED_COLOR
#define VARYINGS_NEED_POSITION_WS
#define VARYINGS_NEED_TEXCOORD0
#define VARYINGS_NEED_COLOR
#define FEATURES_GRAPH_VERTEX
/* WARNING: $splice Could not find named fragment 'PassInstancing' */
#define SHADERPASS SHADERPASS_SPRITEFORWARD
#define REQUIRE_OPAQUE_TEXTURE


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
 float4 uv0 : TEXCOORD0;
 float4 color : COLOR;
#if UNITY_ANY_INSTANCING_ENABLED
 uint instanceID : INSTANCEID_SEMANTIC;
#endif
};
struct Varyings
{
 float4 positionCS : SV_POSITION;
 float3 positionWS;
 float4 texCoord0;
 float4 color;
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
 float4 texCoord0 : INTERP0;
 float4 color : INTERP1;
 float3 positionWS : INTERP2;
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
output.texCoord0.xyzw = input.texCoord0;
output.color.xyzw = input.color;
output.positionWS.xyz = input.positionWS;
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
output.texCoord0 = input.texCoord0.xyzw;
output.color = input.color.xyzw;
output.positionWS = input.positionWS.xyz;
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
float _StencilComp;
float _Stencil;
float _StencilOp;
float _StencilWriteMask;
float _StencilReadMask;
float _ColorMask;
float _RadiusTL;
float _RadiusTR;
float _RadiusBL;
float _RadiusBR;
float _height;
float _widht;
float2 _PositionOffset;
float _AspectRatio;
float _BlurX;
float _BlurY;
CBUFFER_END


// Object and Global properties

// Graph Includes
#include "Assets/Asset Resources/Shader/BlurColor.hlsl"

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

void Unity_SceneColor_float(float4 UV, out float3 Out)
{
    Out = SHADERGRAPH_SAMPLE_SCENE_COLOR(UV.xy);
}

void Unity_Add_float3(float3 A, float3 B, out float3 Out)
{
    Out = A + B;
}

void Unity_Divide_float3(float3 A, float3 B, out float3 Out)
{
    Out = A / B;
}

void Unity_Subtract_float2(float2 A, float2 B, out float2 Out)
{
    Out = A - B;
}

void Unity_Multiply_float2_float2(float2 A, float2 B, out float2 Out)
{
Out = A * B;
}

void Unity_Add_float2(float2 A, float2 B, out float2 Out)
{
    Out = A + B;
}

void Unity_Lerp_float(float A, float B, float T, out float Out)
{
    Out = lerp(A, B, T);
}

void Unity_Multiply_float_float(float A, float B, out float Out)
{
Out = A * B;
}

void Unity_RoundedRectangle_float(float2 UV, float Width, float Height, float Radius, out float Out)
{
    Radius = max(min(min(abs(Radius * 2), abs(Width)), abs(Height)), 1e-5);
    float2 uv = abs(UV * 2 - 1) - float2(Width, Height) + Radius;
    float d = length(max(0, uv)) / Radius;
#if defined(SHADER_STAGE_RAY_TRACING)
    Out = saturate((1 - d) * 1e7);
#else
    float fwd = max(fwidth(d), 1e-5);
    Out = saturate((1 - d) / fwd);
#endif
}

// Custom interpolators pre vertex
/* WARNING: $splice Could not find named fragment 'CustomInterpolatorPreVertex' */

// Graph Vertex
struct VertexDescription
{
float3 Position;
float3 Normal;
float3 Tangent;
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
float3 BaseColor;
float Alpha;
float AlphaClipThreshold;
};

SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
{
SurfaceDescription surface = (SurfaceDescription)0;
float4 _ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4 = float4(IN.NDCPosition.xy, 0, 0);
float _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float = _BlurX;
float _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float = _BlurY;
float4 _BlurColorCustomFunction_95b3c231029f452eac6f36dcd9ea2cff_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 0, 1, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_95b3c231029f452eac6f36dcd9ea2cff_New_5_Vector4);
float3 _SceneColor_24637a447e0b43e684b93da232333e1f_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_95b3c231029f452eac6f36dcd9ea2cff_New_5_Vector4, _SceneColor_24637a447e0b43e684b93da232333e1f_Out_1_Vector3);
float4 _BlurColorCustomFunction_735154d1ca8a429d96253bbfa2ce1776_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 1, 2, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_735154d1ca8a429d96253bbfa2ce1776_New_5_Vector4);
float3 _SceneColor_650174354c11437fbbb4c54960a31ab5_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_735154d1ca8a429d96253bbfa2ce1776_New_5_Vector4, _SceneColor_650174354c11437fbbb4c54960a31ab5_Out_1_Vector3);
float3 _Add_4ff1aab325ae4a1390061c7820c4c33e_Out_2_Vector3;
Unity_Add_float3(_SceneColor_24637a447e0b43e684b93da232333e1f_Out_1_Vector3, _SceneColor_650174354c11437fbbb4c54960a31ab5_Out_1_Vector3, _Add_4ff1aab325ae4a1390061c7820c4c33e_Out_2_Vector3);
float4 _BlurColorCustomFunction_c7a2c2bb45204613ad8e0f9740715af7_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 2, 3, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_c7a2c2bb45204613ad8e0f9740715af7_New_5_Vector4);
float3 _SceneColor_9d05ab8a95cd46d88012aad49336fe8b_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_c7a2c2bb45204613ad8e0f9740715af7_New_5_Vector4, _SceneColor_9d05ab8a95cd46d88012aad49336fe8b_Out_1_Vector3);
float4 _BlurColorCustomFunction_487413fc3bf54796b6aef264cbd59347_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 3, 4, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_487413fc3bf54796b6aef264cbd59347_New_5_Vector4);
float3 _SceneColor_374d6260655b43409d9d0e63c7c79a88_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_487413fc3bf54796b6aef264cbd59347_New_5_Vector4, _SceneColor_374d6260655b43409d9d0e63c7c79a88_Out_1_Vector3);
float3 _Add_699ba07f2fab4d84b956a17fbcee52dd_Out_2_Vector3;
Unity_Add_float3(_SceneColor_9d05ab8a95cd46d88012aad49336fe8b_Out_1_Vector3, _SceneColor_374d6260655b43409d9d0e63c7c79a88_Out_1_Vector3, _Add_699ba07f2fab4d84b956a17fbcee52dd_Out_2_Vector3);
float3 _Add_1155a9c6d2a14b8aa1f18164cffe9139_Out_2_Vector3;
Unity_Add_float3(_Add_4ff1aab325ae4a1390061c7820c4c33e_Out_2_Vector3, _Add_699ba07f2fab4d84b956a17fbcee52dd_Out_2_Vector3, _Add_1155a9c6d2a14b8aa1f18164cffe9139_Out_2_Vector3);
float4 _BlurColorCustomFunction_31eb67e3509f40c7949324e328b1e223_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 4, 5, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_31eb67e3509f40c7949324e328b1e223_New_5_Vector4);
float3 _SceneColor_8af4a0bdf2a44a1b9126ee218017b3de_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_31eb67e3509f40c7949324e328b1e223_New_5_Vector4, _SceneColor_8af4a0bdf2a44a1b9126ee218017b3de_Out_1_Vector3);
float4 _BlurColorCustomFunction_843eb9816bb5487dac6c1f061f1f9774_New_5_Vector4;
BlurColor_float(_ScreenPosition_ae1dcbc017cc43b4b04026fbdcd8ef0b_Out_0_Vector4, 5, 6, _Property_ad430b09095049b8add58ccd030d032b_Out_0_Float, _Property_cf52e33d78054720ae93a53b792b467e_Out_0_Float, _BlurColorCustomFunction_843eb9816bb5487dac6c1f061f1f9774_New_5_Vector4);
float3 _SceneColor_225943d859cd4704a8287606dc43bd2f_Out_1_Vector3;
Unity_SceneColor_float(_BlurColorCustomFunction_843eb9816bb5487dac6c1f061f1f9774_New_5_Vector4, _SceneColor_225943d859cd4704a8287606dc43bd2f_Out_1_Vector3);
float3 _Add_98bfb3fbfb62465889530ca94a0e7555_Out_2_Vector3;
Unity_Add_float3(_SceneColor_8af4a0bdf2a44a1b9126ee218017b3de_Out_1_Vector3, _SceneColor_225943d859cd4704a8287606dc43bd2f_Out_1_Vector3, _Add_98bfb3fbfb62465889530ca94a0e7555_Out_2_Vector3);
float3 _Add_249001cfc28a468fbefbacfabf81db80_Out_2_Vector3;
Unity_Add_float3(_Add_1155a9c6d2a14b8aa1f18164cffe9139_Out_2_Vector3, _Add_98bfb3fbfb62465889530ca94a0e7555_Out_2_Vector3, _Add_249001cfc28a468fbefbacfabf81db80_Out_2_Vector3);
float _Float_2409cbe1f6ec4748981cb52c9b6fe0b6_Out_0_Float = 6;
float3 _Divide_dd2cf632c93945f4a8f609cfb33d2779_Out_2_Vector3;
Unity_Divide_float3(_Add_249001cfc28a468fbefbacfabf81db80_Out_2_Vector3, (_Float_2409cbe1f6ec4748981cb52c9b6fe0b6_Out_0_Float.xxx), _Divide_dd2cf632c93945f4a8f609cfb33d2779_Out_2_Vector3);
float2 _Property_5578a3855e8a44719d5d9b6e095765db_Out_0_Vector2 = _PositionOffset;
float4 _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4 = IN.uv0;
float2 _Vector2_c5f34fb5b59a44a2b980fca5d34d5ac7_Out_0_Vector2 = float2(0.5, 0.5);
float2 _Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2;
Unity_Subtract_float2((_UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4.xy), _Vector2_c5f34fb5b59a44a2b980fca5d34d5ac7_Out_0_Vector2, _Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2);
float _Property_3ade7a0cffd243598721cdea7f6e0006_Out_0_Float = _AspectRatio;
float2 _Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2;
Unity_Multiply_float2_float2(_Subtract_b96d4c55db974864a86fd4376ec61f90_Out_2_Vector2, (_Property_3ade7a0cffd243598721cdea7f6e0006_Out_0_Float.xx), _Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2);
float _Split_4c910cd1ca0d4824affdec5b95a21770_R_1_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[0];
float _Split_4c910cd1ca0d4824affdec5b95a21770_G_2_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[1];
float _Split_4c910cd1ca0d4824affdec5b95a21770_B_3_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[2];
float _Split_4c910cd1ca0d4824affdec5b95a21770_A_4_Float = _UV_f63694cfa795478a804bac62e3750280_Out_0_Vector4[3];
float2 _Vector2_dfc49d6ff87e4a44893b8d697a64631b_Out_0_Vector2 = float2((_Multiply_d5fe440d98164a8b9e703bcfa3cbc5d2_Out_2_Vector2).x, _Split_4c910cd1ca0d4824affdec5b95a21770_G_2_Float);
float2 _Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2;
Unity_Add_float2(_Property_5578a3855e8a44719d5d9b6e095765db_Out_0_Vector2, _Vector2_dfc49d6ff87e4a44893b8d697a64631b_Out_0_Vector2, _Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2);
float _Property_a5e47424cc81499184a3e241c5220179_Out_0_Float = _widht;
float _Property_457c23cec6fb477abb811b46b7efde37_Out_0_Float = _height;
float _Property_9448c79e185f4c3db9ff6a327b1ef50f_Out_0_Float = _RadiusBL;
float _Property_e7e742ecaa374eb0a6b7545569d39d0c_Out_0_Float = _RadiusBR;
float4 _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4 = IN.uv0;
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[0];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_G_2_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[1];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_B_3_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[2];
float _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_A_4_Float = _UV_81a63811e0e947f990b7d38d861f1374_Out_0_Vector4[3];
float _Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float;
Unity_Lerp_float(_Property_9448c79e185f4c3db9ff6a327b1ef50f_Out_0_Float, _Property_e7e742ecaa374eb0a6b7545569d39d0c_Out_0_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float, _Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float);
float _Property_e9b2750607524cf68340b65b9c1e831d_Out_0_Float = _RadiusTL;
float _Property_7e4835b76a244b7d9c4f9d13b512c1ac_Out_0_Float = _RadiusTR;
float _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float;
Unity_Lerp_float(_Property_e9b2750607524cf68340b65b9c1e831d_Out_0_Float, _Property_7e4835b76a244b7d9c4f9d13b512c1ac_Out_0_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_R_1_Float, _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float);
float _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float;
Unity_Lerp_float(_Lerp_1f9fd6c3c44249c9a411ae95e319d27c_Out_3_Float, _Lerp_e453bcfe5fa1406683907c1de48f1bc8_Out_3_Float, _Split_da9ff98f05a04b5bb8d1cac662ccd5e4_G_2_Float, _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float);
float _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float;
Unity_Multiply_float_float(_Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float, _Lerp_a7aa823a54ab4b8f8c58225d1339aa80_Out_3_Float, _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float);
float _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float;
Unity_RoundedRectangle_float(_Add_f5238f4d61934d71a1c01dbdb85c0b45_Out_2_Vector2, _Property_a5e47424cc81499184a3e241c5220179_Out_0_Float, _Property_457c23cec6fb477abb811b46b7efde37_Out_0_Float, _Multiply_b28e4d8b955349098329837930cdc260_Out_2_Float, _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float);
surface.BaseColor = _Divide_dd2cf632c93945f4a8f609cfb33d2779_Out_2_Vector3;
surface.Alpha = _RoundedRectangle_c8aeeee302af4d8d9a3f96bb69541aed_Out_4_Float;
surface.AlphaClipThreshold = 0;
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
#if VFX_USE_GRAPH_VALUES
    uint instanceActiveIndex = asuint(UNITY_ACCESS_INSTANCED_PROP(PerInstance, _InstanceActiveIndex));
    /* WARNING: $splice Could not find named fragment 'VFXLoadGraphValues' */
#endif
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
#include "Packages/com.unity.render-pipelines.universal/Editor/2D/ShaderGraph/Includes/SpriteUnlitPass.hlsl"

// --------------------------------------------------
// Visual Effect Vertex Invocations
#ifdef HAVE_VFX_MODIFICATION
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/VisualEffectVertex.hlsl"
#endif

ENDHLSL
}
}
CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
FallBack "Hidden/Shader Graph/FallbackError"
}