// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UT/Flight System Floor"
{
	Properties
	{
		_CalibrationFloorDiffuse("CalibrationFloorDiffuse", 2D) = "white" {}
		_PlayerPos("PlayerPos", Vector) = (0,0,0,1)
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _CalibrationFloorDiffuse;
		uniform float4 _CalibrationFloorDiffuse_ST;
		uniform float4 _PlayerPos;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_CalibrationFloorDiffuse = i.uv_texcoord * _CalibrationFloorDiffuse_ST.xy + _CalibrationFloorDiffuse_ST.zw;
			float4 tex2DNode2 = tex2D( _CalibrationFloorDiffuse, uv_CalibrationFloorDiffuse );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 transform7 = mul(unity_ObjectToWorld,float4( ase_vertex3Pos , 0.0 ));
			float smoothstepResult12 = smoothstep( 0.9 , 0.901 , (0.0 + (distance( _PlayerPos , transform7 ) - 0.0) * (1.0 - 0.0) / (1.5 - 0.0)));
			float4 lerpResult16 = lerp( tex2DNode2 , ( 1.0 - tex2DNode2 ) , ( 1.0 - saturate( smoothstepResult12 ) ));
			o.Albedo = lerpResult16.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
758;240;1531;903;674.5;506;1;True;False
Node;AmplifyShaderEditor.PosVertexDataNode;6;-695.5,117;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector4Node;5;-722.5,-77;Inherit;False;Property;_PlayerPos;PlayerPos;1;0;Create;True;0;0;0;False;0;False;0,0,0,1;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ObjectToWorldTransfNode;7;-491.5,88;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DistanceOpNode;4;-381.5,-60;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;11;-203.5,-28;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1.5;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;12;-7.5,71;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0.9;False;2;FLOAT;0.901;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-555.5,-291;Inherit;True;Property;_CalibrationFloorDiffuse;CalibrationFloorDiffuse;0;0;Create;True;0;0;0;False;0;False;-1;f74216b7220d1974ba5822647df1cf7c;f74216b7220d1974ba5822647df1cf7c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;9;148.5,-20;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;17;359.5,2;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;15;-124.5,-225;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;16;267.5,-286;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;558,-164;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;UT/Flight System Floor;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;7;0;6;0
WireConnection;4;0;5;0
WireConnection;4;1;7;0
WireConnection;11;0;4;0
WireConnection;12;0;11;0
WireConnection;9;0;12;0
WireConnection;17;0;9;0
WireConnection;15;0;2;0
WireConnection;16;0;2;0
WireConnection;16;1;15;0
WireConnection;16;2;17;0
WireConnection;0;0;16;0
ASEEND*/
//CHKSM=7A264EDED105A3CA79524BD50ABC30C31B68E1E7