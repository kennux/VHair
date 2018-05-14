using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
	public static class UnitySingleton<T> where T : MonoBehaviour
	{
		private static T instance;

		public static T Get()
		{
			return instance;
		}

		public static void Register(T obj)
		{
            if (!Essentials.UnityIsNull(instance))
                throw new InvalidOperationException("Tried registering a singleton that already has an instance set!");

			instance = obj;
		}
	}

	public static class Singleton
	{
		public static T GetUnitySingleton<T>() where T : MonoBehaviour
		{
			return UnitySingleton<T>.Get();
		}

		public static void RegisterUnitySingleton<T>(T obj) where T : MonoBehaviour
		{
			UnitySingleton<T>.Register(obj);
		}
	}
}