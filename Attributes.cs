using System;

namespace XmlHelper
{
	public static class Attributes
	{
		public static bool HasAttribute
			(this object @object,
			Type attribute) =>
			HasAttribute(@object.GetType(), attribute);

		public static bool HasAttribute
			(this Type @object,
			Type attribute) =>
			Attribute.IsDefined(@object, attribute);
	}
}
