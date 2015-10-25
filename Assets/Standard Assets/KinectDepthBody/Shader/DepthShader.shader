Shader "Custom/DepthShader" {
	SubShader {
		//ZWrite Off
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 200

		Pass
		{
			Cull Off ZWrite On Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			//Define vertex and fragment-shader functions
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry GS_Main

			#pragma exclude_renderers flash
			#pragma only_renderers d3d11
			#pragma target 5.0

			#include "UnityCG.cginc"


			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 col : TEXCOORD0;
				float size : PSIZE;
			};

			struct colorSpacePoint
			{
				float x;
				float y;
			};

			float3 ReferencePosition;
			float3 ScaleOffset;
			float3 PositionOffset;

			float3 RotateVector;
			float3 RotationPoint;
			float PointSize = 1;
			
			int FilterBody;

			//Mapping the ComputeBuffer to this shader
			StructuredBuffer<float3> particleBuffer;
			StructuredBuffer<float> bodyIndexBuffer;
			StructuredBuffer<float3> colorBuffer;

			float4x4 rotate(float3 r, float4 d)
			{
				float cx, cy, cz, sx, sy, sz;
				sincos(r.x, sx, cx);
				sincos(r.y, sy, cy);
				sincos(r.z, sz, cz);	
				return float4x4(cy*cz,      -cy*sz,     sy, d.x,
							cx*sz+cz*sx*sy, cx*cz-sx*sy*sz, -cy*sx, d.y,
							sx*sz-cx*cz*sy, cx*sy*sz+cz*sx, cx*cy,  d.z,
							0,              0,      0,  1);
			}

			vertexOutput vert (appdata_full input, uint inst : SV_InstanceID)
			{       
				//Define VertexShader Output
				vertexOutput output;

				float3 pos = particleBuffer[inst];

				//YX Inverterieren
				pos.x = (1 - pos.x) + 512;
				pos.y = (1 - pos.y) + 424;

				//Positionieren und Skalieren

				pos.x *= ScaleOffset.x;
				pos.y *= ScaleOffset.y;
				pos.z *= ScaleOffset.z;

				//Rotieren
				float4x4 rot_mat_x, rot_mat_y, rot_mat_z;
				rot_mat_x = rotate(float3(RotateVector.x, 0, 0), float4(RotationPoint,1));
				rot_mat_y = rotate(float3(0, RotateVector.y, 0), float4(RotationPoint,1));
				rot_mat_z = rotate(float3(0, 0, RotateVector.z), float4(RotationPoint,1));

				float4 tmpPos = mul(rot_mat_x, float4(pos, 1));
				tmpPos = mul(rot_mat_y, tmpPos);
				tmpPos = mul(rot_mat_z, tmpPos);


				tmpPos.x = ReferencePosition.x + PositionOffset.x + tmpPos.x;
				tmpPos.y = ReferencePosition.y + PositionOffset.y + tmpPos.y;
				tmpPos.z = ReferencePosition.z + PositionOffset.z + tmpPos.z;


				//Auf Unity Welt anpassen
				output.pos = mul(UNITY_MATRIX_MVP, tmpPos);
				
				//Show just body or all Particle
				if(FilterBody == 1)
				{
					//Hide particle for default
					

					//Find Body
					float player = bodyIndexBuffer[inst];
					if (player != 255  && !particleBuffer[inst].z == 0)
					{
						//Set Color for 
						float3 tmp = colorBuffer[inst];
						output.col = float4(tmp.x, tmp.y, tmp.z, 1);
						
					}
					else
					{
						output.col = float4(0, 0, 0, 0);
						output.pos = float4(0, -1000, -1000, 0);
					}
				}
				else
				{
					float3 tmp = colorBuffer[inst];
					output.col = float4(tmp.x, tmp.y, tmp.z, 1);
				}

				output.size = PointSize;
				return output;
			}

			

			float4 frag(vertexOutput input) : COLOR
			{
				return input.col; 
			}

			[maxvertexcount(4)]
			void GS_Main(point vertexOutput p[1], inout TriangleStream<vertexOutput> triStream)	
			{
				float4 right;
				float4 up;

				right.x = UNITY_MATRIX_MV[0][0];  // 0
				right.y = UNITY_MATRIX_MV[1][0];  // 4
				right.z = UNITY_MATRIX_MV[2][0];  // 8
				right.w = 0;

				up.x = UNITY_MATRIX_MV[0][1];  // 1
				up.y = UNITY_MATRIX_MV[1][1];  // 5
				up.z = UNITY_MATRIX_MV[2][1];  // 9
				up.w = 0;

				float scale = p[0].size;

				float val = scale * 0.5;

				float4 v1 = p[0].pos    + float4(-val, -val, 0, 0);
				float4 v2 = p[0].pos    + float4(-val, val, 0, 0);
				float4 v3 = p[0].pos    + float4(val, -val, 0, 0);
				float4 v4 = p[0].pos    + float4(val, val, 0, 0);
				
				vertexOutput output;
				output.pos = v1;
				output.col = p[0].col;
				output.size = p[0].size;
				triStream.Append(output);

				output.pos = v2;
				output.col = p[0].col;
				output.size = p[0].size;
				triStream.Append(output);

				output.pos = v3;
				output.col = p[0].col;
				output.size = p[0].size;
				triStream.Append(output);

				output.pos = v4;
				output.col = p[0].col;
				output.size = p[0].size;
				triStream.Append(output);


			}
		
			ENDCG
		}
	} 
	FallBack "Diffuse"
}