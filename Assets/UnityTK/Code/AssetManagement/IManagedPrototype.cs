using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityTK.Prototypes;

namespace UnityTK.AssetManagement
{
	/// <summary>
	/// Prototype interface extension for using <see cref="PrototypesLoader"/> in unitytk's asset management system.
	/// Use this interface to implement new prototypes that should be manageable by unitytk's asset management.
	/// </summary>
	public interface IManagedPrototype : IPrototype, IManagedAsset { }
}