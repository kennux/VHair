using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.Prototypes
{
	/// <summary>
	/// This attribute can be used to mark classes or structs to be serialized for <see cref="PrototypeParser"/>.
	/// A marked class will be generating a type cache object in <see cref="PrototypeCaches"/>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class PrototypeDataSerializableAttribute : Attribute
	{
	}
}