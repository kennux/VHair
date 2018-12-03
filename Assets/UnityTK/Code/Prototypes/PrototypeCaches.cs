using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Linq;

namespace UnityTK.Prototypes
{
	/// <summary>
	/// UnityTK static cache class.
	/// 
	/// This will generate a type and serializer cache when <see cref="LazyInit"/> is called.
	/// LazyInit can be called whenever this is about to be accessed in order to make sure the prototype cache is ready.
	/// </summary>
	public static class PrototypeCaches
	{
		private static List<IPrototypeDataSerializer> serializers = new List<IPrototypeDataSerializer>();
		private static Dictionary<Type, SerializableTypeCache> typeCache = new Dictionary<Type, SerializableTypeCache>();
		private static bool wasInitialized = false;

		/// <summary>
		/// Returns the best known data serializer for the specified type.
		/// Currently this will always return the first serializer found, TODO: Implement serializer rating and selecting the most appropriate one.
		/// </summary>
		/// <param name="type">The type to get a serializer for.</param>
		/// <returns>Null if not found, the serializer otherwise.</returns>
		public static IPrototypeDataSerializer GetBestSerializerFor(Type type)
		{
			foreach (var instance in serializers)
			{
				if (instance.CanBeUsedFor(type))
					return instance;
			}
			return null;
		}

		/// <summary>
		/// Returns the serializable type cache if known for the specified type.
		/// Serializable type caches are being genearted in <see cref="LazyInit"/> from classes with <see cref="PrototypeDataSerializableAttribute"/> attributes via reflection.
		/// </summary>
		public static SerializableTypeCache GetSerializableTypeCacheFor(Type type)
		{
			SerializableTypeCache cache;
			if (!typeCache.TryGetValue(type, out cache))
				return null;
			return cache;
		}

		public static SerializableTypeCache LookupSerializableTypeCache(string writtenName, string preferredNamespace)
		{
			List<SerializableTypeCache> tmp = ListPool<SerializableTypeCache>.Get();

			try
			{
				foreach (var cache in typeCache)
				{
					if (string.Equals(cache.Key.Name, writtenName))
						tmp.Add(cache.Value);
				}

				if (tmp.Count > 0)
				{
					foreach (var cache in tmp)
						if (string.Equals(cache.type.Namespace, preferredNamespace))
							return cache;
				}

				return tmp.Count == 0 ? null : tmp[0];
			}
			finally
			{
				ListPool<SerializableTypeCache>.Return(tmp);
			}
		}

		/// <summary>
		/// Lazy init method to ensure the cache is built.
		/// </summary>
		public static void LazyInit()
		{
			if (wasInitialized)
				return;

			foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
			{
				foreach (var type in asm.GetTypes())
				{
					if (typeof(IPrototype).IsAssignableFrom(type) || type.GetCustomAttributes(true).Any((a) => a.GetType() == typeof(PrototypeDataSerializableAttribute)))
					{
						SerializableTypeCache cache = new SerializableTypeCache();
						cache.Build(type);
						typeCache.Add(type, cache);
					}
					else if (!type.IsAbstract && !type.IsInterface && typeof(IPrototypeDataSerializer).IsAssignableFrom(type))
					{
						serializers.Add(Activator.CreateInstance(type) as IPrototypeDataSerializer);
					}
				}
			}

			wasInitialized = true;
		}
	}
}
