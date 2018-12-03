using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UnityTK.Prototypes
{
	class SerializedData
	{
		public enum CollectionOverrideAction
		{
			Replace,
			Combine
		}

		public string inherits;
		
		/// <summary>
		/// All fields serialized in the data of this object.
		/// </summary>
		private Dictionary<string, object> fields = new Dictionary<string, object>();

		private Dictionary<string, IXmlLineInfo> debug = new Dictionary<string, IXmlLineInfo>();
		private Dictionary<string, CollectionOverrideAction> collectionOverrideActions = new Dictionary<string, CollectionOverrideAction>();
		
		public readonly SerializableTypeCache targetType;
		public readonly XElement xElement;
		public readonly string filename;

		/// <summary>
		/// </summary>
		/// <param name="targetType">The target type to be parsed.</param>
		/// <param name="element">The xml element to parse the data from.</param>
		/// <param name="filename">The filename to be reported in case of errors.</param>
		public SerializedData(SerializableTypeCache targetType, XElement element, string filename)
		{
			this.targetType = targetType;
			this.xElement = element;
			this.filename = filename;

			var inheritsAttrib = element.Attribute(PrototypeParser.PrototypeAttributeInherits);
			if (!ReferenceEquals(inheritsAttrib, null))
			{
				this.inherits = inheritsAttrib.Value;
			}
		}

		/// <summary>
		/// Actually loads the data from the previous parse <see cref="ParseFields(List{ParsingError})"/>.
		/// It will for every field with string data deserialize this data using <see cref="IPrototypeDataSerializer"/>.
		/// 
		/// For every sub-data field (<seealso cref="ParseFields(List{ParsingError})"/>) a <see cref="SerializedData"/> object is being written to <see cref="fields"/>.
		/// The sub-data object will have <see cref="PrepareParse(SerializableTypeCache, XElement, string)"/>, <see cref="ParseFields(List{ParsingError})"/> and <see cref="LoadFields(List{ParsingError}, PrototypeParserState)"/> called.
		/// </summary>
		public void LoadFields(List<ParsingError> errors, PrototypeParserState state)
		{
			foreach (var xNode in xElement.Nodes())
			{
				if (!ParsingValidation.NodeIsElement(xNode, this.filename, errors)) // Malformed XML
					continue;

				var xElement = xNode as XElement;
				var elementName = xElement.Name.LocalName;

				// Field unknown?
				if (!ParsingValidation.FieldKnown(xElement, targetType, elementName, filename, errors))
					continue;

				debug.Add(elementName, xElement);
				var fieldData = targetType.GetFieldData(elementName);
				var fieldType = fieldData.serializableTypeCache;
				if (fieldType == null)
				{
					// Not a prototype or data serializable

					// Is this field a collection?
					if (SerializedCollectionData.IsCollection(fieldData.fieldInfo.FieldType))
					{
						var col = new SerializedCollectionData(fieldData.fieldInfo.FieldType, xElement, this.filename);
						col.ParseAndLoadData(errors, state);
						fields.Add(elementName, col);

						// Collection override action?
						var collectionOverrideAttrib = xElement.Attribute(PrototypeParser.PrototypeAttributeCollectionOverrideAction);
						if (!ReferenceEquals(collectionOverrideAttrib, null))
							collectionOverrideActions.Set(elementName, (CollectionOverrideAction)Enum.Parse(typeof(CollectionOverrideAction), collectionOverrideAttrib.Value));
					}
					// Value type serialized
					else
					{
						try
						{
							var serializer = PrototypeCaches.GetBestSerializerFor(fieldData.fieldInfo.FieldType);
							if (!ParsingValidation.SerializerWasFound(xElement, serializer, elementName, targetType == null ? null : targetType.type, fieldType == null ? null : fieldType.type, filename, errors))
								continue;

							fields.Add(elementName, serializer.Deserialize(fieldData.fieldInfo.FieldType, xElement, state));
						}
						catch (Exception ex)
						{
							errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, -1, "Serializer threw exception on field " + elementName + " on type " + targetType.type + ":\n\n" + ex.ToString() + "\n\nSkipping field!"));
						}
					}
				}
				else
				{
					// A known serializable

					// Prototype reference?
					if (fieldData.isPrototype)
					{
						fields.Add(elementName, new SerializedPrototypeReference()
						{
							name = xElement.Value as string
						});
					}
					else
					{
						// Determine which type to serialize
						var targetType = fieldData.serializableTypeCache;
						string typeName = fieldData.fieldInfo.Name;

						// Check if element explicitly overwrites the type to support polymorphism
						// The field type might be some base class type and the xml overwrites this type with a class extending from the base
						var classAttrib = xElement.Attribute(PrototypeParser.PrototypeAttributeType);
						if (!ReferenceEquals(classAttrib, null))
						{
							targetType = PrototypeCaches.LookupSerializableTypeCache(classAttrib.Value, state.parameters.standardNamespace);
							typeName = classAttrib.Value;
						}

						// Field not serializable?
						if (!ParsingValidation.DataFieldSerializerFound(xElement, targetType, typeName, elementName, filename, errors))
							continue;

						// Resolve field name type
						var d = new SerializedData(targetType, xElement as XElement, this.filename);
						d.LoadFields(errors, state);
						fields.Add(elementName, d);
					}
				}
			}
		}

		/// <summary>
		/// The last step for loading data.
		/// In this step, prototype references are being resolved for this serialized data and their sub data objects.
		/// </summary>
		/// <param name="prototypes">Prototypes to use for remapping</param>
		/// <param name="errors"></param>
		public void ResolveReferenceFields(List<IPrototype> prototypes, List<ParsingError> errors, PrototypeParserState state)
		{
			Dictionary<string, object> updates = DictionaryPool<string, object>.Get();

			try
			{
				foreach (var field in fields)
				{
					var @ref = field.Value as SerializedPrototypeReference;
					if (!ReferenceEquals(@ref, null))
						updates.Add(field.Key, @ref.Resolve(prototypes));

					var sub = field.Value as SerializedData;
					if (!ReferenceEquals(sub, null))
						sub.ResolveReferenceFields(prototypes, errors, state);

					var col = field.Value as SerializedCollectionData;
					if (!ReferenceEquals(col, null))
						col.ResolveReferenceFieldsAndSubData(prototypes, errors, state);
				}
				
				// Write updates
				foreach (var update in updates)
					this.fields[update.Key] = update.Value;
			}
			finally
			{
				DictionaryPool<string, object>.Return(updates);
			}
		}

		/// <summary>
		/// Applies the data stored in this serialized data to the specified object using reflection.
		/// </summary>
		public void ApplyTo(object obj, List<ParsingError> errors, PrototypeParserState state)
		{
			foreach (var field in fields)
			{
				var fieldInfo = this.targetType.GetFieldData(field.Key);
				var value = field.Value;

				var sub = value as SerializedData;
				if (!ReferenceEquals(sub, null))
				{
					// Field already set?
					value = fieldInfo.fieldInfo.GetValue(obj);

					// If not, create new obj
					if (ReferenceEquals(value, null))
						value = sub.targetType.Create();

					sub.ApplyTo(value, errors, state);
				}

				var col = value as SerializedCollectionData;
				if (!ReferenceEquals(col, null))
				{
					// Field already set?
					value = fieldInfo.fieldInfo.GetValue(obj);
					
					CollectionOverrideAction action;
					if (!ReferenceEquals(value, null) && collectionOverrideActions.TryGetValue(field.Key, out action))
					{
						switch (action)
						{
							case CollectionOverrideAction.Combine: value = col.CombineWithInNew(value); break;
							case CollectionOverrideAction.Replace: value = col.CreateCollection(); break;
						}
					}
					else // Write new collection
						value = col.CreateCollection();
				}

				if (!ParsingValidation.TypeCheck(debug.TryGet(field.Key), field.Key, value, fieldInfo.fieldInfo.FieldType, filename, errors))
					continue;

				fieldInfo.fieldInfo.SetValue(obj, value);
			}
		}

		/// <summary>
		/// Returns an enumeration to be used to iterate over every reference in this data and all its sub data objects.
		/// </summary>
		public IEnumerable<string> GetReferencedPrototypes()
		{
			foreach (var field in fields)
			{
				var v = field.Value as SerializedPrototypeReference;
				if (!ReferenceEquals(v, null))
					yield return v.name;

				var sub = field.Value as SerializedData;
				if (!ReferenceEquals(sub, null))
					foreach (var @ref in sub.GetReferencedPrototypes())
						yield return @ref;

				var scd = field.Value as SerializedCollectionData;
				if (!ReferenceEquals(scd, null))
					foreach (var @ref in scd.GetReferencedPrototypes())
						yield return @ref;
			}
		}
	}
}
