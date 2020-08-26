// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UT/Light Bridge"
{
	Properties
	{
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.682353
		_Metallic("Metallic", Range( 0 , 1)) = 0.6588235
		_Albedo("Albedo", Color) = (0.4481132,0.6734691,1,0.6901961)
		[HDR]_Emission("Emission", Color) = (2.917647,0.2196078,3.968627,0)
		_InputPosition("InputPosition", Vector) = (0,0,0,0)
		_StartDistance("Start Distance", Float) = 0
		_FadeDistance("Fade Distance", Float) = 0.01
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float4 _Albedo;
		uniform float4 _Emission;
		uniform float4 _InputPosition;
		uniform float _StartDistance;
		uniform float _FadeDistance;
		uniform float _Metallic;
		uniform float _Smoothness;


		float2 voronoihash25( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi25( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash25( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.5 * dot( r, r );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			 		}
			 	}
			}
			return (F2 + F1) * 0.5;
		}


		float2 voronoihash6( float2 p )
		{
			
			p = float2( dot( p, float2( 127.1, 311.7 ) ), dot( p, float2( 269.5, 183.3 ) ) );
			return frac( sin( p ) *43758.5453);
		}


		float voronoi6( float2 v, float time, inout float2 id, inout float2 mr, float smoothness )
		{
			float2 n = floor( v );
			float2 f = frac( v );
			float F1 = 8.0;
			float F2 = 8.0; float2 mg = 0;
			for ( int j = -1; j <= 1; j++ )
			{
				for ( int i = -1; i <= 1; i++ )
			 	{
			 		float2 g = float2( i, j );
			 		float2 o = voronoihash6( n + g );
					o = ( sin( time + o * 6.2831 ) * 0.5 + 0.5 ); float2 r = f - g - o;
					float d = 0.776 * pow( ( pow( abs( r.x ), 2.73 ) + pow( abs( r.y ), 2.73 ) ), 0.366 );
			 		if( d<F1 ) {
			 			F2 = F1;
			 			F1 = d; mg = g; mr = r; id = o;
			 		} else if( d<F2 ) {
			 			F2 = d;
			 		}
			 	}
			}
			return (F2 + F1) * 0.5;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Albedo.rgb;
			float time25 = _Time.y;
			float2 uv_TexCoord29 = i.uv_texcoord * float2( 0.64,0.64 );
			float2 coords25 = uv_TexCoord29 * 6.66;
			float2 id25 = 0;
			float2 uv25 = 0;
			float fade25 = 0.5;
			float voroi25 = 0;
			float rest25 = 0;
			for( int it25 = 0; it25 <2; it25++ ){
			voroi25 += fade25 * voronoi25( coords25, time25, id25, uv25, 0 );
			rest25 += fade25;
			coords25 *= 2;
			fade25 *= 0.5;
			}//Voronoi25
			voroi25 /= rest25;
			float smoothstepResult26 = smoothstep( 0.1 , 1.0 , voroi25);
			float time6 = _Time.y;
			float2 coords6 = i.uv_texcoord * 6.66;
			float2 id6 = 0;
			float2 uv6 = 0;
			float fade6 = 0.5;
			float voroi6 = 0;
			float rest6 = 0;
			for( int it6 = 0; it6 <2; it6++ ){
			voroi6 += fade6 * voronoi6( coords6, time6, id6, uv6, 0 );
			rest6 += fade6;
			coords6 *= 2;
			fade6 *= 0.5;
			}//Voronoi6
			voroi6 /= rest6;
			float smoothstepResult10 = smoothstep( 0.09 , 1.0 , voroi6);
			float4 appendResult42 = (float4(_InputPosition.x , _InputPosition.y , _InputPosition.z , 1.0));
			float4 transform14 = mul(unity_WorldToObject,appendResult42);
			float4 ase_vertex4Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float temp_output_17_0 = (0.0 + (distance( transform14 , ase_vertex4Pos ) - _StartDistance) * (1.0 - 0.0) / (( _StartDistance + _FadeDistance ) - _StartDistance));
			float dotResult35 = dot( temp_output_17_0 , temp_output_17_0 );
			float smoothstepResult40 = smoothstep( 0.01 , 0.39 , saturate( ( 1.0 / ( dotResult35 + 1.0 ) ) ));
			float fade22 = smoothstepResult40;
			o.Emission = ( saturate( smoothstepResult26 ) * ( ( saturate( smoothstepResult10 ) * _Emission ) * fade22 ) ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = _Albedo.a;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard alpha:fade keepalpha fullforwardshadows 

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
				float2 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
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
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
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
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
2785.333;28;2328;1284;2920.402;98.61401;1;True;False
Node;AmplifyShaderEditor.Vector4Node;13;-2442.948,355.4484;Inherit;False;Property;_InputPosition;InputPosition;4;0;Create;True;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;42;-2224.402,376.386;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;1;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PosVertexDataNode;16;-2236.122,577.2401;Inherit;False;1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;20;-1864.813,645.956;Inherit;False;Property;_FadeDistance;Fade Distance;6;0;Create;True;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldToObjectTransfNode;14;-2046.851,371.0926;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;18;-1853.964,561.5679;Inherit;False;Property;_StartDistance;Start Distance;5;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-1639.377,601.3508;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;15;-1737.027,445.8362;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;17;-1459.752,472.358;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;35;-1200.197,480.7017;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;32;-1022.469,385.3301;Inherit;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;36;-934.3896,501.0381;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;9;-1754.819,-212.0429;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-1946.296,-355.3709;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;33;-785.9457,414.5351;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;6;-1504.296,-327.3709;Inherit;True;0;4;2.73;3;2;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;6.66;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SaturateNode;37;-572.0645,432.0445;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;40;-432.4016,409.386;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.01;False;2;FLOAT;0.39;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-1835.99,-581.2999;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.64,0.64;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;10;-1242.296,-285.3709;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.09;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;22;-244.5394,419.5335;Inherit;False;fade;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-1244.235,-55.01525;Inherit;False;Property;_Emission;Emission;3;1;[HDR];Create;True;0;0;False;0;False;2.917647,0.2196078,3.968627,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;11;-992.2951,-220.3709;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VoronoiNode;25;-1460.349,-633.3923;Inherit;True;0;0;2;3;2;False;1;False;False;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;6.66;False;3;FLOAT;0;False;3;FLOAT;0;FLOAT2;1;FLOAT2;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-826.5402,-104.3316;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SmoothstepOpNode;26;-1266.99,-591.2999;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0.1;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;23;-1098.43,153.9556;Inherit;False;22;fade;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;27;-1041.99,-499.2999;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;30;-669.9902,-95.29993;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;4;-301,-343;Inherit;False;Property;_Albedo;Albedo;2;0;Create;True;0;0;False;0;False;0.4481132,0.6734691,1,0.6901961;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;2;-337,165;Inherit;False;Property;_Smoothness;Smoothness;0;0;Create;True;0;0;False;0;False;0.682353;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-471.9902,-146.2999;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-336,81;Inherit;False;Property;_Metallic;Metallic;1;0;Create;True;0;0;False;0;False;0.6588235;0.6588235;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;75,-82;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;UT/Light Bridge;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;42;0;13;1
WireConnection;42;1;13;2
WireConnection;42;2;13;3
WireConnection;14;0;42;0
WireConnection;19;0;18;0
WireConnection;19;1;20;0
WireConnection;15;0;14;0
WireConnection;15;1;16;0
WireConnection;17;0;15;0
WireConnection;17;1;18;0
WireConnection;17;2;19;0
WireConnection;35;0;17;0
WireConnection;35;1;17;0
WireConnection;36;0;35;0
WireConnection;33;0;32;0
WireConnection;33;1;36;0
WireConnection;6;0;7;0
WireConnection;6;1;9;0
WireConnection;37;0;33;0
WireConnection;40;0;37;0
WireConnection;10;0;6;0
WireConnection;22;0;40;0
WireConnection;11;0;10;0
WireConnection;25;0;29;0
WireConnection;25;1;9;0
WireConnection;12;0;11;0
WireConnection;12;1;5;0
WireConnection;26;0;25;0
WireConnection;27;0;26;0
WireConnection;30;0;12;0
WireConnection;30;1;23;0
WireConnection;28;0;27;0
WireConnection;28;1;30;0
WireConnection;0;0;4;0
WireConnection;0;2;28;0
WireConnection;0;3;1;0
WireConnection;0;4;2;0
WireConnection;0;9;4;4
ASEEND*/
//CHKSM=04551CEDB080463F5FDB1275C8CF29166EE4BD8D