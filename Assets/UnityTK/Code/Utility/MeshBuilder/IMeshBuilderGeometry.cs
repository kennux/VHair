using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTK
{
	/// <summary>
	/// Interface to be used for implementing geometry for <see cref="MeshBuilder"/>.
	/// Implementations can be structs, which can be submitted to the builder via 
	/// </summary>
	public interface IMeshBuilderGeometry
	{
		/// <summary>
		/// Called in order to build the geometry represented by this object / struct.
		/// </summary>
		/// <param name="builder">The builder to build the geometry in.</param>
		void Build(MeshBuilder builder);
	}
}
