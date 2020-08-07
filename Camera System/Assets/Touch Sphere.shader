// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UT/Camera/Touch Sphere"
{
	Properties
	{
		[HDR]_Emission("Emission", Color) = (0.8156863,2.509804,3.968627,0)
		[PerRendererData][Enum(Off,0,On,1)]_Enabled("Enabled", Int) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
		};

		uniform int _Enabled;
		uniform float4 _Emission;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float mulTime8 = _Time.y * 0.14;
			float temp_output_17_0 = ( ( 1.0 - saturate( max( (1.0 + (ase_vertex3Pos.z - -0.4) * (0.0 - 1.0) / (-0.35 - -0.4)) , (0.0 + (ase_vertex3Pos.z - 0.35) * (1.0 - 0.0) / (0.4 - 0.35)) ) ) ) * step( fmod( ( ( ase_vertex3Pos.z + 1.0 ) + mulTime8 ) , 0.06 ) , 0.019 ) );
			o.Emission = ( _Enabled * ( _Emission * temp_output_17_0 ) ).rgb;
			o.Alpha = ( _Enabled * temp_output_17_0 );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit alpha:fade keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18300
0;661;1188;699;1917.769;597.5677;2.01615;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;2;-2305.521,286.4202;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;4;-1560.336,369.0802;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;8;-1562.648,548.3123;Inherit;False;1;0;FLOAT;0.14;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;12;-1923.61,146.4275;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;0.35;False;2;FLOAT;0.4;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;11;-1932.103,-91.14972;Inherit;True;5;0;FLOAT;0;False;1;FLOAT;-0.4;False;2;FLOAT;-0.35;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;9;-1357.976,429.2096;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;14;-1391.404,61.25697;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FmodOpNode;3;-1189.102,421.9161;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.06;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;15;-1168.768,177.169;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;5;-895.0366,495.5799;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0.019;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;16;-944.3612,300.3895;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-614.3917,479.0193;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;1;-804.1085,38.24095;Inherit;False;Property;_Emission;Emission;0;1;[HDR];Create;True;0;0;False;0;False;0.8156863,2.509804,3.968627,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-458.7097,100.933;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.IntNode;19;-510.8969,-125.0182;Inherit;False;Property;_Enabled;Enabled;1;2;[PerRendererData];[Enum];Create;True;2;Off;0;On;1;0;False;0;False;0;0;0;1;INT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-94.35474,255.7456;Inherit;False;2;2;0;INT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-130.6446,76.74556;Inherit;False;2;2;0;INT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;112.1734,57.15756;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;UT/Camera/Touch Sphere;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;2;3
WireConnection;12;0;2;3
WireConnection;11;0;2;3
WireConnection;9;0;4;0
WireConnection;9;1;8;0
WireConnection;14;0;11;0
WireConnection;14;1;12;0
WireConnection;3;0;9;0
WireConnection;15;0;14;0
WireConnection;5;0;3;0
WireConnection;16;0;15;0
WireConnection;17;0;16;0
WireConnection;17;1;5;0
WireConnection;6;0;1;0
WireConnection;6;1;17;0
WireConnection;21;0;19;0
WireConnection;21;1;17;0
WireConnection;20;0;19;0
WireConnection;20;1;6;0
WireConnection;0;2;20;0
WireConnection;0;9;21;0
ASEEND*/
//CHKSM=C2F232E07930B92D0314795E7D0AEB35416C8960