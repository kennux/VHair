using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Prototypes
{
	/// <summary>
	/// Prototype interface that can be used to implement prototypes loadable by <see cref="PrototypeParser"/> from XML.
	/// </summary>
	public interface IPrototype
	{
		/// <summary>
		/// The identifier of the prototype.
		/// This identifier is the one to be used for referencing prototypes in other data.
		/// 
		/// It is automatically written by the parsers using the XML attribute "Id" on the prototype XMLElement.
		/// </summary>
		string identifier { get; set; }
	}
}