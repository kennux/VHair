using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace UnityTK.Prototypes
{
	/// <summary>
	/// TODO: Actual caching :>
	/// </summary>
	public class SerializableTypeCache
	{
		public class FieldCache
		{
			public FieldInfo fieldInfo;
			public SerializableTypeCache serializableTypeCache
			{
				get { return PrototypeCaches.GetSerializableTypeCacheFor(this.fieldInfo.FieldType); }
			}
			public bool isPrototype
			{
				get { return typeof(IPrototype).IsAssignableFrom(this.fieldInfo.FieldType); }
			}

			public FieldCache(FieldInfo fieldInfo)
			{
				this.fieldInfo = fieldInfo;
			}
		}

		public Type type
		{
			get;
			private set;
		}

		public object Create()
		{
			return Activator.CreateInstance(this.type);
		}

		public void Build(Type type)
		{
			this.type = type;
		}

		public bool HasField(string fieldName)
		{
			return !ReferenceEquals(this.type.GetField(fieldName), null);
		}

		public FieldCache GetFieldData(string fieldName)
		{
			var fi = this.type.GetField(fieldName);
			return ReferenceEquals(fi, null) ? null : new FieldCache(fi);
		}
	}
}