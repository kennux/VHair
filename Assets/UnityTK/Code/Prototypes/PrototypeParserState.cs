using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;

namespace UnityTK.Prototypes
{
	/// <summary>
	/// The current state of the <see cref="PrototypeParser"/> parser.
	/// Used to inform sub-modules of the prototypes parser of the high-level state.
	/// </summary>
	public class PrototypeParserState
	{
		public PrototypeParseParameters parameters;
	}
}
