// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "UT/Pulse Shader"
{
	Properties
	{
		_Albedo("Albedo", Color) = (0.4811321,0.4811321,0.4811321,0)
		_CycleStart("CycleStart", Float) = 0
		_CycleLength("CycleLength", Float) = 0
		[HDR]_PulseColor("Pulse Color", Color) = (0.7215686,2.384314,8,0)
		_Metallic("Metallic", Range( 0 , 1)) = 1
		_Smoothness("Smoothness", Range( 0 , 1)) = 0.6
		_SceneStart("SceneStart", Float) = 0
		_HueShift("Hue Shift", Range( 0 , 1)) = 0
		_JumpScale("Jump Scale", Float) = 0.2
		_CutoffHeight("Cutoff Height", Float) = 0
		_CutoffRange("Cutoff Range", Float) = 0.0001
		[Enum(False,0,True,1)]_CutoffActive("Cutoff Active", Int) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			half filler;
		};

		uniform float _JumpScale;
		uniform float _SceneStart;
		uniform float _CycleStart;
		uniform float _CycleLength;
		uniform int _CutoffActive;
		uniform float _CutoffHeight;
		uniform float _CutoffRange;
		uniform float4 _Albedo;
		uniform float _HueShift;
		uniform float4 _PulseColor;
		uniform float _Metallic;
		uniform float _Smoothness;


		float3 HSVToRGB( float3 c )
		{
			float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
			float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
			return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
		}


		float3 RGBToHSV(float3 c)
		{
			float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
			float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
			float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
			float d = q.x - min( q.w, q.y );
			float e = 1.0e-10;
			return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
		}

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float cycle29 = saturate( ( 1.0 - (0.0 + (( ( _SceneStart + _Time.y ) - _CycleStart ) - 0.0) * (1.0 - 0.0) / (_CycleLength - 0.0)) ) );
			float lerpResult32 = lerp( 0.0 , _JumpScale , cycle29);
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 appendResult27 = (float3(0.0 , ( lerpResult32 * ( (float)_CutoffActive == 1.0 ? saturate( (0.0 + (ase_vertex3Pos.y - _CutoffHeight) * (1.0 - 0.0) / (( _CutoffHeight + _CutoffRange ) - _CutoffHeight)) ) : 1.0 ) ) , 0.0));
			v.vertex.xyz += appendResult27;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Albedo = _Albedo.rgb;
			float3 appendResult22 = (float3(_PulseColor.r , _PulseColor.g , _PulseColor.b));
			float3 hsvTorgb21 = RGBToHSV( appendResult22 );
			float3 hsvTorgb23 = HSVToRGB( float3(( _HueShift + hsvTorgb21 ).x,hsvTorgb21.y,hsvTorgb21.z) );
			float cycle29 = saturate( ( 1.0 - (0.0 + (( ( _SceneStart + _Time.y ) - _CycleStart ) - 0.0) * (1.0 - 0.0) / (_CycleLength - 0.0)) ) );
			o.Emission = ( hsvTorgb23 * cycle29 );
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18300
2785.333;28;2328;1284;1773.03;125.3494;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;6;-1885.758,-47;Inherit;False;Property;_SceneStart;SceneStart;6;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;7;-1891.758,34;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;1;-1871.758,121;Inherit;False;Property;_CycleStart;CycleStart;1;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-1669.758,-55;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-1532.758,85.99999;Inherit;False;Property;_CycleLength;CycleLength;2;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;11;-1502.758,-37;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;12;-1323.758,-32;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-1248.555,789.049;Inherit;False;Property;_CutoffRange;Cutoff Range;10;0;Create;True;0;0;False;0;False;0.0001;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-1231.216,688.049;Inherit;False;Property;_CutoffHeight;Cutoff Height;9;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;13;-1135.758,-33;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;14;-1562,-275;Inherit;False;Property;_PulseColor;Pulse Color;3;1;[HDR];Create;True;0;0;False;0;False;0.7215686,2.384314,8,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;19;-940.3393,-9.768555;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;-1042.03,752.6506;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;33;-1240.458,541.2809;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;34;-908.5552,675.049;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;22;-1241.582,-244.7686;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;29;-739.1511,3.155309;Inherit;False;cycle;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;41;-923.03,546.6506;Inherit;False;Property;_CutoffActive;Cutoff Active;11;1;[Enum];Create;True;2;False;0;True;1;0;False;0;False;0;0;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-1242.582,-421.7686;Inherit;False;Property;_HueShift;Hue Shift;7;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-1059.833,317.9204;Inherit;False;Property;_JumpScale;Jump Scale;8;0;Create;True;0;0;False;0;False;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RGBToHSVNode;21;-1104.582,-260.7686;Inherit;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;30;-1115.669,420.2895;Inherit;False;29;cycle;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;43;-728.03,742.6506;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-871.5815,-293.7686;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Compare;39;-634.03,545.6506;Inherit;False;0;4;0;INT;0;False;1;FLOAT;1;False;2;FLOAT;1;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;32;-703.5552,374.049;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-486.5552,419.049;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.HSVToRGBNode;23;-725.5815,-239.7686;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;3;-349,-422;Inherit;False;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;False;0.4811321,0.4811321,0.4811321,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;5;-434,159;Inherit;False;Property;_Smoothness;Smoothness;5;0;Create;True;0;0;False;0;False;0.6;0.6;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;-427,-67;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;27;-266.8325,395.9204;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-436,81;Inherit;False;Property;_Metallic;Metallic;4;0;Create;True;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;0,0;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;UT/Pulse Shader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;8;0;6;0
WireConnection;8;1;7;0
WireConnection;11;0;8;0
WireConnection;11;1;1;0
WireConnection;12;0;11;0
WireConnection;12;2;2;0
WireConnection;13;0;12;0
WireConnection;19;0;13;0
WireConnection;38;0;35;0
WireConnection;38;1;36;0
WireConnection;34;0;33;2
WireConnection;34;1;35;0
WireConnection;34;2;38;0
WireConnection;22;0;14;1
WireConnection;22;1;14;2
WireConnection;22;2;14;3
WireConnection;29;0;19;0
WireConnection;21;0;22;0
WireConnection;43;0;34;0
WireConnection;25;0;24;0
WireConnection;25;1;21;0
WireConnection;39;0;41;0
WireConnection;39;2;43;0
WireConnection;32;1;28;0
WireConnection;32;2;30;0
WireConnection;37;0;32;0
WireConnection;37;1;39;0
WireConnection;23;0;25;0
WireConnection;23;1;21;2
WireConnection;23;2;21;3
WireConnection;15;0;23;0
WireConnection;15;1;29;0
WireConnection;27;1;37;0
WireConnection;0;0;3;0
WireConnection;0;2;15;0
WireConnection;0;3;4;0
WireConnection;0;4;5;0
WireConnection;0;11;27;0
ASEEND*/
//CHKSM=1C76F42FCEB6C17CC00DBB95E46D08363E3E45F2