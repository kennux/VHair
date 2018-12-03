using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

namespace UnityTK.Prototypes
{
	/// <summary>
	/// Parser object with the ability to parse prototypes from XML content.
	/// </summary>
	public class PrototypeParser
	{
		public const string PrototypeContainerXMLName = "PrototypeContainer";
		public const string PrototypeContainerAttributeType = "Type";

		public const string PrototypeElementXMLName = "Prototype";
		public const string PrototypeAttributeInherits = "Inherits";
		public const string PrototypeAttributeIdentifier = "Id";
		public const string PrototypeAttributeType = "Type";
		public const string PrototypeAttributeAbstract = "Abstract";
		public const string PrototypeAttributeCollectionOverrideAction = "CollectionOverrideAction";

		private List<ParsingError> errors = new List<ParsingError>();
		private List<IPrototype> prototypes = new List<IPrototype>();
		private Dictionary<string, SerializedData> serializedData = new Dictionary<string, SerializedData>();

		/// <summary>
		/// Returns the internal prototypes list.
		/// </summary>
		public List<IPrototype> GetPrototypes()
		{
			return this.prototypes;
		}

		/// <summary>
		/// Returns the internal errors list.
		/// </summary>
		public List<ParsingError> GetParsingErrors()
		{
			return this.errors;
		}

		/// <summary>
		/// Parses the specified XML content and returns all prototypes which could be parsed.
		/// </summary>
		/// <param name="xmlContent">The xml content to use for parsing-</param>
		/// <param name="parameters">The parameters for the parser.</param>
		/// <param name="extend">The result to extend when parsing. Setting this will give the parser the ability to let data to be loaded now inherited from data loaded in extend.</param>
		/// <returns></returns>
		public void Parse(string xmlContent, string filename, PrototypeParseParameters parameters)
		{
			PrototypeCaches.LazyInit();

			// Pre-parse data
			var data = new List<SerializedData>();
			_PreParse(xmlContent, filename, ref parameters, data);
			_Parse(data, ref parameters);
		}

		/// <summary>
		/// Same as <see cref="Parse(string, string)"/>, but can parse many xmls with relationships / dependencies in them together.
		/// This will make it possible to have a prototype in one file which is inherited from in another file.
		/// 
		/// The prototypes will be loaded in order and able to resolve references across multiple files!
		/// </summary>
		public void Parse(string[] xmlContents, string[] filenames, PrototypeParseParameters parameters)
		{
			PrototypeCaches.LazyInit();
			List<SerializedData> data = new List<SerializedData>();

			if (xmlContents.Length != filenames.Length)
				throw new ArgumentException("Xml content string count must match filename count in Prototypes.Parse()!");

			for (int i = 0; i < xmlContents.Length; i++)
			{
				_PreParse(xmlContents[i], filenames[i], ref parameters, data);
			}
			
			_Parse(data, ref parameters);
		}
		
		private void _PreParse(string xmlContent, string filename, ref PrototypeParseParameters parameters, List<SerializedData> result)
		{
			ListPool<SerializedData>.GetIfNull(ref result);
			ListPool<ParsingError>.GetIfNull(ref errors);
			var xElement = XElement.Parse(xmlContent);


			// Validity checks
			if (!ParsingValidation.ContainerElementName(xElement, filename, errors) ||
				!ParsingValidation.ContainerTypeAttribute(xElement, filename, errors))
				return;
				
			// Get type
			XAttribute typeAttribute = xElement.Attribute(PrototypeContainerAttributeType);
			var type = LookupSerializableTypeCache(typeAttribute.Value, ref parameters);
			if (!ParsingValidation.TypeFound(xElement, typeAttribute, type, filename, errors))
				return;

			// Iterate over nodes
			foreach (var xNode in xElement.Nodes())
			{
				var elementType = type;
				var nodeXElement = xNode as XElement;

				// Validity checks
				if (!ParsingValidation.NodeIsElement(xNode, filename, errors) ||
					!ParsingValidation.PrototypeElementName(nodeXElement, filename, errors))
					continue;


				var elementTypeAttribute = nodeXElement.Attribute(PrototypeContainerAttributeType);
				if (!ReferenceEquals(elementTypeAttribute, null))
				{
					elementType = LookupSerializableTypeCache(elementTypeAttribute.Value, ref parameters);
					if (!ParsingValidation.TypeFound(nodeXElement, elementTypeAttribute, elementType, filename, errors))
						continue;
				}

				// Prepare
				var data = new SerializedData(elementType, nodeXElement, filename);
				result.Add(data);
			}
		}

		private void _Parse(List<SerializedData> data, ref PrototypeParseParameters parameters)
		{
			ListPool<ParsingError>.GetIfNull(ref errors);

			// Get prototypes with others inheriting from first

			// Key = type which is inheriting from something, Value = the type its inheriting from
			Dictionary<SerializedData, List<SerializedData>> inheritingFrom = new Dictionary<SerializedData, List<SerializedData>>(); // This is only used for topo sort!
			Dictionary<SerializedData, object> instances = new Dictionary<SerializedData, object>();
			Dictionary<string, SerializedData> idMapping = new Dictionary<string, SerializedData>();
			List<SerializedData> invalid = new List<SerializedData>();
			

			// Pre-parse names, create instances and apply name
			foreach (var d in data)
			{
				if (!ParsingValidation.ElementHasId(d.xElement, d.filename, errors))
				{
					invalid.Add(d);
					continue;
				}

				// Read name
				var attribName = d.xElement.Attribute(PrototypeAttributeIdentifier);
				idMapping.Add(attribName.Value, d);

				// Check if abstract prototype data
				var attribAbstract = d.xElement.Attribute(PrototypeAttributeAbstract);
				bool isAbstract = !ReferenceEquals(attribAbstract, null) && string.Equals("True", attribAbstract.Value);
				
				if (!isAbstract)
				{
					var obj = d.targetType.Create();
					instances.Add(d, obj);

					(obj as IPrototype).identifier = attribName.Value;
					this.prototypes.Add(obj as IPrototype);
				}
			}
			
			// Remove invalidated entries
			foreach (var d in invalid)
				data.Remove(d);

			invalid.Clear();
			foreach (var d in data)
			{
				SerializedData inheritedData;
				if (!string.IsNullOrEmpty(d.inherits) && idMapping.TryGetValue(d.inherits, out inheritedData))
					inheritingFrom.GetOrCreate(d).Add(inheritedData);
			}

			PrototypeParserState state = new PrototypeParserState()
			{
				parameters = parameters
			};

			// Remove invalidated entries
			foreach (var d in invalid)
				data.Remove(d);

			// Step 1 - sort by inheritance
			List<SerializedData> empty = new List<SerializedData>();
			var sorted = data.TSort((sd) => inheritingFrom.ContainsKey(sd) ? inheritingFrom[sd] : empty, true).ToList();

			// Step 2 - Preloads the fields and creates sub-data objects
			foreach (var d in sorted)
				d.LoadFields(errors, state);

			// Step 3 - run sorting algorithm for reference resolve
			foreach (var d in data)
				d.ResolveReferenceFields(this.prototypes, errors, state);

			// Step 5 - Final data apply
			List<SerializedData> inheritingFromTmp = new List<SerializedData>();
			foreach (var d in sorted)
			{
				if (!instances.ContainsKey(d))
					continue;

				// Apply inherited data first
				if (!string.IsNullOrEmpty(d.inherits))
				{
					// Look up all inherited data in bottom to top order
					inheritingFromTmp.Clear();
					var inheritedData = d.inherits;

					while (!string.IsNullOrEmpty(inheritedData))
					{
						SerializedData serializedData = null;
						if (!this.serializedData.TryGetValue(inheritedData, out serializedData) && !idMapping.TryGetValue(inheritedData, out serializedData))
							this.errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, d.filename, (d.xElement as IXmlLineInfo).LinePosition, "Could not find the prototype '" + inheritedData + "' for prototype '" + (instances[d] as IPrototype).identifier + "'! Ignoring inheritance!"));
						else
							inheritingFromTmp.Add(serializedData);

						// Recursion
						inheritedData = serializedData.inherits;
					}

					// Reverse so we apply in top to bottom order
					inheritingFromTmp.Reverse();

					// Apply
					foreach (var _d in inheritingFromTmp)
						_d.ApplyTo(instances[d], errors, state);
				}

				// Apply data over inherited
				d.ApplyTo(instances[d], errors, state);
			}

			// Step 6 - record serialized data in result
			foreach (var kvp in idMapping)
				this.serializedData.Add(kvp.Key, kvp.Value);
		}

		private static SerializableTypeCache LookupSerializableTypeCache(string name, ref PrototypeParseParameters parameters)
		{
			return PrototypeCaches.LookupSerializableTypeCache(name, parameters.standardNamespace);
		}
	}
}