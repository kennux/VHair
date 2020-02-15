using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VHair
{
	/// <summary>
	/// Datastructure for storing hair strand information in <see cref="HairAsset"/>
	/// </summary>
	[System.Serializable]
	public struct HairStrand
	{
		/// <summary>
		/// Pointer to <see cref="HairAsset.vertices"/>.
		/// The first vertex of the strand.
		/// </summary>
		public int firstVertex;

		/// <summary>
		/// Pointer to <see cref="HairAsset.vertices"/>.
		/// The last vertex of the strand.
		/// </summary>
		public int lastVertex;
	}
}