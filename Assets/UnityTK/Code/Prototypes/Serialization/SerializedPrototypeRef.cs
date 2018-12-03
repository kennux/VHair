using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnityTK.Prototypes
{
	class SerializedPrototypeReference
	{
		public string name;

		public IPrototype Resolve(List<IPrototype> prototypes)
		{
			foreach (var p in prototypes)
			{
				if (string.Equals(p.identifier, this.name))
					return p;
			}

			return null;
		}
	}
}