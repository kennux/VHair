using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace VHair.Editor
{
	public static class HairAssetProcessors
	{

		private static List<IHairAssetProcessor> _processors = null;
		public static List<IHairAssetProcessor> GetProcessors()
		{
			if (ReferenceEquals(_processors, null))
			{
				_processors = AppDomain.CurrentDomain.GetAssemblies().SelectMany((asm) => asm.GetTypes()).Where((type) => typeof(IHairAssetProcessor).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface).Select((type) => Activator.CreateInstance(type)).Cast<IHairAssetProcessor>().ToList();
			}

			return _processors;
		}

	}
	public interface IHairAssetProcessor
	{
		string Name { get; }
		string Description { get; }

		void OnGUI();

		void Run(HairAsset asset);
	}
}