using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml.Linq;
using System.Xml;
using System.Linq;

namespace UnityTK.Prototypes
{
	internal static class ParsingValidation
	{
		// TODO: string.Format()

		public static bool DataFieldSerializerFound(XElement xElement, SerializableTypeCache typeCache, string typeName, string fieldName, string filename, List<ParsingError> errors)
		{
			if (ReferenceEquals(typeCache, null))
			{
				string msg = string.Format("Field '{0}' with unknown type {1} - unknown by the serializer cache! Are you missing {2} attribute?", fieldName, typeName, nameof(PrototypeDataSerializableAttribute));
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, msg));
				return false;
			}
			return true;
		}
		
		public static bool TypeCheck(IXmlLineInfo debug, string fieldName, object value, Type expectedType, string filename, List<ParsingError> errors)
		{
			if (!ReferenceEquals(value, null) && !expectedType.IsAssignableFrom(value.GetType()))
			{
				string msg = string.Format("Fatal error deserializing field {0} - tried applying field data but types mismatched! Stored type: {1} - Expected type: {2}!", fieldName, value.GetType(), expectedType);
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, ReferenceEquals(debug, null) ? -1 : debug.LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool ContainerElementName(XElement xElement, string filename, List<ParsingError> errors)
		{
			if (!string.Equals(xElement.Name.LocalName, PrototypeParser.PrototypeContainerXMLName))
			{
				string msg = string.Format("Element name '{0}' is incorrect / not supported, must be '{1}'!", xElement.Name, PrototypeParser.PrototypeContainerXMLName);
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool PrototypeElementName(XElement xElement, string filename, List<ParsingError> errors)
		{
			if (!string.Equals(xElement.Name.LocalName, PrototypeParser.PrototypeElementXMLName)) // Unsupported
			{
				string msg = string.Format("Element name '{0}' is incorrect / not supported, must be '{1}'!", xElement.Name, PrototypeParser.PrototypeElementXMLName);
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool ContainerTypeAttribute(XElement xElement, string filename, List<ParsingError> errors)
		{
			var typeAttribute = xElement.Attribute(PrototypeParser.PrototypeContainerAttributeType);
			if (ReferenceEquals(typeAttribute, null))
			{
				string msg = string.Format("Element missing '{0}'! Need '{1}' attribute specifying the type of the prototypes to be loaded!", PrototypeParser.PrototypeContainerAttributeType, PrototypeParser.PrototypeContainerAttributeType);
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool ElementHasId(XElement xElement, string filename, List<ParsingError> errors)
		{
			var attribName = xElement.Attribute(PrototypeParser.PrototypeAttributeIdentifier);
			if (ReferenceEquals(attribName, null))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Prototype without identifier!"));
				return false;
			}
			return true;
		}

		public static bool TypeFound(XElement xElement, XAttribute typeAttribute, SerializableTypeCache type, string filename, List<ParsingError> errors)
		{
			if (ReferenceEquals(type, null))
			{
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xElement as IXmlLineInfo).LineNumber, "Element type " + typeAttribute.Value + " unknown!"));
				return false;
			}
			return true;
		}

		public static bool NodeIsElement(XNode xNode, string filename, List<ParsingError> errors)
		{
			if (!(xNode is XElement)) // Malformed XML
			{
				string msg = string.Format("Unable to cast node to element for {0}!", xNode);
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, (xNode as IXmlLineInfo).LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool SerializerWasFound(IXmlLineInfo debug, IPrototypeDataSerializer serializer, string field, Type declaringType, Type fieldType, string filename, List<ParsingError> errors)
		{
			if (ReferenceEquals(serializer, null))
			{
				// TODO: Line number
				string msg = string.Format("Serializer for field {0} on type {1} (Field type: {2}) could not be found!", field, declaringType, fieldType);
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, ReferenceEquals(debug, null) ? -1 : debug.LineNumber, msg));
				return false;
			}
			return true;
		}

		public static bool FieldKnown(IXmlLineInfo debug, SerializableTypeCache type, string field, string filename, List<ParsingError> errors)
		{
			if (!type.HasField(field))
			{
				// TODO: Line number
				string msg = string.Format("Unknown field {0}!", field);
				errors.Add(new ParsingError(ParsingErrorSeverity.ERROR, filename, ReferenceEquals(debug, null) ? -1 : debug.LineNumber, msg));
				return false;
			}
			return true;
		}
	}
}
