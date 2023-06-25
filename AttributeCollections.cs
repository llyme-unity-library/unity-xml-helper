using System;
using System.Xml;

namespace XmlHelper
{
	public static class AttributeCollections
	{
		public static T Enum<T>
			(this XmlAttributeCollection attributes,
			string name,
			T @default = default)
			where T : struct
		{
			if (attributes == null)
				return @default;

			XmlAttribute attribute = attributes.Attribute(name);

			if (attribute == null)
				return @default;

			if (!System.Enum.TryParse(attribute.InnerText.Trim(), true, out T value))
				return @default;

			return value;
		}

		public static bool TryAttribute
			(this XmlAttributeCollection attributes,
			string name,
			out XmlAttribute value)
		{
			if (attributes != null)
				foreach (XmlAttribute attribute in attributes)
				{
					bool flag = string.Equals(
						attribute.Name,
						name,
						StringComparison.OrdinalIgnoreCase
					);

					if (flag)
					{
						value = attribute;
						return true;
					}
				}

			value = null;
			return false;
		}

		public static XmlAttribute Attribute
			(this XmlAttributeCollection attributes,
			string name) =>
			TryAttribute(attributes, name, out XmlAttribute attribute)
			? attribute
			: null;

		public static string String
			(this XmlAttributeCollection attributes,
			string name,
			string @default = "")
		{
			string text =
				TryAttribute(attributes, name, out XmlAttribute attribute)
				? attribute.InnerText
				: @default;

			return text;
		}

		public static bool TryString
			(this XmlAttributeCollection attributes,
			string name,
			out string value)
		{
			if (!TryAttribute(attributes, name, out XmlAttribute attribute))
			{
				value = null;
				return false;
			}

			value = attribute.InnerText;
			return true;
		}

		public static float Float
			(this XmlAttributeCollection attributes,
			string name,
			float @default = 0) =>
			TryAttribute(attributes, name, out XmlAttribute attribute) &&
			float.TryParse(attribute.InnerText, out float value)
			? value
			: @default;

		public static bool TryInt
			(this XmlAttributeCollection attributes,
			string name,
			out int value)
		{
			value = default;
			return TryAttribute(attributes, name, out XmlAttribute attribute) &&
				int.TryParse(attribute.InnerText, out value);
		}

		public static int Int
			(this XmlAttributeCollection attributes,
			string name,
			int @default = 0) =>
			TryInt(attributes, name, out int value)
			? value
			: @default;

		public static bool TryBool
			(this XmlAttributeCollection attributes,
			string name,
			out bool value)
		{
			value = default;
			return TryAttribute(attributes, name, out XmlAttribute attribute) &&
				bool.TryParse(attribute.InnerText, out value);
		}

		public static bool Bool
			(this XmlAttributeCollection attributes,
			string name,
			bool @default = false) =>
			TryBool(attributes, name, out bool value)
			? value
			: @default;
	}
}
