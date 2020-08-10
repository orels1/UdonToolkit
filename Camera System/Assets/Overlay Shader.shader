// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UT/Camera/Overlay Shader"
{
	Properties
	{
		_MainTex("MainTex", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[PerRendererData]_ForceShow("Force Show", Int) = 0
		_WatermarkImage("Watermark Image", 2D) = "black" {}
		[PerRendererData]_Watermark("Watermark", Int) = 1
		[PerRendererData]_EndFadeTime("EndFadeTime", Float) = 0
		[PerRendererData]_SceneStartTime("Scene Start Time", Float) = 0
		_DesktopHelpOverlay("Desktop Help Overlay", 2D) = "black" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Overlay+0" "IsEmissive" = "true"  }
		Cull Front
		ZWrite On
		ZTest Always
		CGPROGRAM
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf StandardCustomLighting keepalpha noshadow 
		struct Input
		{
			float4 screenPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _MainTex;
		uniform sampler2D _WatermarkImage;
		uniform int _Watermark;
		uniform sampler2D _DesktopHelpOverlay;
		uniform float _EndFadeTime;
		uniform float _SceneStartTime;
		uniform int _ForceShow;
		uniform float _Cutoff = 0.5;


		int IsCamera4(  )
		{
			#if UNITY_SINGLE_PASS_STEREO
			  return 0;
			#else
			  if (abs(UNITY_MATRIX_V[0].y) > 0.0000005) {
			    return 1;
			  }
			  return 0;
			#endif
		}


		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			int localIsCamera4 = IsCamera4();
			c.rgb = 0;
			c.a = 1;
			clip( (float)( _ForceShow + localIsCamera4 ) - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float2 appendResult2 = (float2(ase_screenPosNorm.x , ase_screenPosNorm.y));
			float4 tex2DNode9 = tex2D( _WatermarkImage, appendResult2 );
			float4 lerpResult32 = lerp( tex2D( _MainTex, appendResult2 ) , tex2DNode9 , ( _Watermark * tex2DNode9.a ));
			float4 tex2DNode20 = tex2D( _DesktopHelpOverlay, appendResult2 );
			float4 lerpResult34 = lerp( lerpResult32 , tex2DNode20 , ( saturate( (0.0 + (( _EndFadeTime - ( _Time.y + _SceneStartTime ) ) - 0.0) * (1.0 - 0.0) / (2.0 - 0.0)) ) * tex2DNode20.a ));
			o.Emission = lerpResult34.rgb;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18300
2695.333;28;2328;1284;2013.326;1130.891;1;True;False
Node;AmplifyShaderEditor.SimpleTimeNode;16;-1822.175,-790.1926;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;27;-1758.102,-652.8344;Inherit;False;Property;_SceneStartTime;Scene Start Time;6;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;1;-1751.363,72.08673;Float;False;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;14;-1591.442,-909.7822;Inherit;False;Property;_EndFadeTime;EndFadeTime;5;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-1543.193,-763.5781;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;2;-1443.83,93.82007;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;17;-1349.434,-897.7357;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;13;-889.1721,-334.4624;Inherit;False;Property;_Watermark;Watermark;4;1;[PerRendererData];Create;True;0;0;False;0;False;1;1;0;1;INT;0
Node;AmplifyShaderEditor.TFHCRemapNode;18;-1151.308,-887.4995;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;2;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;9;-1012.937,-158.5219;Inherit;True;Property;_WatermarkImage;Watermark Image;3;0;Create;True;0;0;False;0;False;-1;11b9048f039e83147952f0c125c6b558;8110a33ebec38754d8d96e18efa9729a;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;20;-1212.847,-670.4114;Inherit;True;Property;_DesktopHelpOverlay;Desktop Help Overlay;7;0;Create;True;0;0;False;0;False;-1;3e86b19f0ddf4c445a9f4524ed01d281;3e86b19f0ddf4c445a9f4524ed01d281;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-662.3262,-93.89111;Inherit;False;2;2;0;INT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;3;-732.9453,76.02296;Inherit;True;Property;_MainTex;MainTex;0;0;Create;True;0;0;False;0;False;-1;88d1ca75cc191cf4b85668ade69165fc;88d1ca75cc191cf4b85668ade69165fc;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;23;-865.0602,-821.594;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;32;-393.3262,-112.8911;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-683.0679,-583.7103;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;7;-689.303,303.0246;Inherit;False;Property;_ForceShow;Force Show;2;1;[PerRendererData];Create;True;0;0;False;0;False;0;0;0;1;INT;0
Node;AmplifyShaderEditor.CustomExpressionNode;4;-653.0362,430.7235;Inherit;False;#if UNITY_SINGLE_PASS_STEREO$  return 0@$#else$  if (abs(UNITY_MATRIX_V[0].y) > 0.0000005) {$    return 1@$  }$  return 0@$#endif;0;False;0;IsCamera;True;False;0;0;1;INT;0
Node;AmplifyShaderEditor.LerpOp;34;-172.3262,-198.8911;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-360.4028,345.9246;Inherit;False;2;2;0;INT;0;False;1;INT;0;False;1;INT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;174.7822,-64.24359;Float;False;True;-1;2;ASEMaterialInspector;0;0;CustomLighting;UT/Camera/Overlay Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Front;1;False;-1;7;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;TransparentCutout;;Overlay;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Absolute;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;28;0;16;0
WireConnection;28;1;27;0
WireConnection;2;0;1;1
WireConnection;2;1;1;2
WireConnection;17;0;14;0
WireConnection;17;1;28;0
WireConnection;18;0;17;0
WireConnection;9;1;2;0
WireConnection;20;1;2;0
WireConnection;33;0;13;0
WireConnection;33;1;9;4
WireConnection;3;1;2;0
WireConnection;23;0;18;0
WireConnection;32;0;3;0
WireConnection;32;1;9;0
WireConnection;32;2;33;0
WireConnection;21;0;23;0
WireConnection;21;1;20;4
WireConnection;34;0;32;0
WireConnection;34;1;20;0
WireConnection;34;2;21;0
WireConnection;8;0;7;0
WireConnection;8;1;4;0
WireConnection;0;2;34;0
WireConnection;0;10;8;0
ASEEND*/
//CHKSM=271CB6213F241922DB1B89E39B9BDD4ADE10A8F5