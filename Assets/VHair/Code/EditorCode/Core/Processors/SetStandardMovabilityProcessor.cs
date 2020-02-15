using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VHair.Editor
{
	public class SetStandardMovabilityProcessor : IHairAssetProcessor
	{
		public string Name => "Set standard movability";

		public string Description => "Sets the first vertex of every strand immovable and every other vertex movable.";

		public void OnGUI()
		{
		}

		public void Run(HairAsset asset)
		{
			uint[] movability = HairMovability.CreateData(asset.VertexCount);
			HairStrand[] strands = asset.CreateStrandDataCopy();

			for (int i = 0; i < strands.Length; i++)
			{
				HairStrand strand = strands[i];
				HairMovability.SetMovable(strand.firstVertex, false, movability);
				int cnt = (strand.lastVertex - strand.firstVertex) + 1;
				for (int j = 1; j < cnt; j++)
				{
					HairMovability.SetMovable(strand.firstVertex + j, true, movability);
				}
			}

			asset.InitializeMovability(movability);
		}
	}
}
