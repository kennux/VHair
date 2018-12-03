using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK
{
	/// <summary>
	/// Line geometry implementation for <see cref="MeshBuilder"/>.
	/// </summary>
	public struct TriangulatedLineGeometry : IMeshBuilderGeometry
	{
		public Vector3 p1;
		public Vector3 p2;
		public float width;
		public Vector3 normal;
		
		public Color? color1;
		public Color? color2;

		public void Build(MeshBuilder builder)
		{
			if (builder.currentTopology != MeshTopology.Triangles)
				throw new InvalidOperationException("Triangulated line geometry only supports triangles mesh topology!");

			Vector3 dir = (this.p2 - this.p1).normalized;
			Vector3 binormal = Vector3.Cross(this.normal, dir).normalized;
			
			Vector3 p1 = this.p1 + (binormal * this.width);
			Vector3 p2 = this.p1 + (binormal * -this.width);
			Vector3 p3 = this.p2 + (binormal * this.width);
			Vector3 p4 = this.p2 + (binormal * -this.width);

			Color c1 = Color.white, c2 = Color.white, c3 = Color.white, c4 = Color.white;
			if (this.color1.HasValue && !this.color2.HasValue)
			{
				c1 = c2 = c3 = c4 = this.color1.Value;
			}
			else if (this.color1.HasValue && this.color2.HasValue)
			{
				c1 = c2 = this.color1.Value;
				c3 = c4 = this.color2.Value;
			}

			MeshBuilder.VertexData vertex1 = new MeshBuilder.VertexData()
			{
				position = p1,
				normal = this.normal,
				color = c1
			};
			MeshBuilder.VertexData vertex2 = new MeshBuilder.VertexData()
			{
				position = p2,
				normal = this.normal,
				color = c2
			};
			MeshBuilder.VertexData vertex3 = new MeshBuilder.VertexData()
			{
				position = p3,
				normal = this.normal,
				color = c3
			};
			MeshBuilder.VertexData vertex4 = new MeshBuilder.VertexData()
			{
				position = p4,
				normal = this.normal,
				color = c4
			};
			
			// Triangle 1
			builder.AddVertex(ref vertex2);
			builder.AddVertex(ref vertex4);
			builder.AddVertex(ref vertex1);
			
			// Triangle 2
			builder.AddVertex(ref vertex1);
			builder.AddVertex(ref vertex4);
			builder.AddVertex(ref vertex3);
		}
	}
}
