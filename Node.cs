using CollectionHelper;
using MathHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TextHelper;
using UnityEngine;

namespace XmlHelper
{
	public static partial class Node
	{
		/// <summary>
		/// Find a node based on the given path from their tag names,
		/// separated by dots.
		/// Case-insensitive.
		/// </summary>
		public static XmlNode Find(this XmlNode element, string path)
		{
			XmlNode[] result = Find(element, path.Split('.'));
			return result?[^1];
		}

		/// <returns>
		/// The hierarchy, including the root node.
		/// `null` if it doesn't exist.
		/// </returns>
		public static XmlNode[] Find(this XmlNode element, string[] path)
		{
			if (element == null)
				return null;

			if (path.IsNullOrEmpty())
				return new XmlNode[] { element };

			XmlNode[] result = new XmlNode[path.Length + 1];
			result[0] = element;
			
			for (int i = 0; i < path.Length; i++)
			{
				string name = path[i];
				result[i + 1] = element = element.Element(name);

				if (element == null)
					return null;
			}

			return result;
		}

		/// <summary>
		/// Find all nodes with the same path and name.
		/// </summary>
		public static IEnumerable<XmlNode> FindAll
			(this XmlNode element,
			string[] path) =>
			path.IsNullOrEmpty()
			? new XmlNode[0]
			: FindAll_Internal(element, path, 0);

		private static IEnumerable<XmlNode> FindAll_Internal
			(this XmlNode element,
			string[] path,
			int index)
		{
			IEnumerable<XmlNode> list = element.Elements(path[index]);
			int max = path.Length - 1;

			if (index < max)
			{
				foreach (XmlNode node in list)
					foreach (XmlNode target in FindAll_Internal(node, path, index + 1))
						yield return target;

				yield break;
			}
			else if (index == max)
				foreach (XmlNode node in list)
					yield return node;
		}

		/// <summary>
		/// Returns all elements, including its descendants.
		/// </summary>
		public static IEnumerable<XmlNode> Descendants(this XmlNode element)
		{
			if (element == null)
				yield break;

			Stack<XmlNode> stack = new();
			stack.Push(element);

			while (stack.Count > 0)
			{
				element = stack.Pop();

				foreach (XmlNode child in element.Elements())
				{
					stack.Push(child);
					yield return child;
				}
			}
		}

		/// <summary>
		/// Returns all elements in the given element,
		/// including its descendants,
		/// that are empty or text only.
		/// </summary>
		public static IEnumerable<XmlNode> TextOnlyElements
			(this XmlNode element)
		{
			if (element == null)
				yield break;

			Stack<XmlNode> stack = new();
			stack.Push(element);

			while (stack.Count > 0)
				foreach (XmlNode child in stack.Pop().Elements())
					if (child.EmptyOrText())
						yield return child;
					else
						stack.Push(child);
		}

		public static bool EmptyOrText(this XmlNode element)
		{
			if (element == null)
				return false;

			foreach (XmlNode child in element)
				if (child.NodeType != XmlNodeType.Text)
					return false;

			return true;
		}

		/// <summary>
		/// Get all elements.
		/// </summary>
		public static IEnumerable<XmlNode> Elements(this XmlNode element)
		{
			if (element == null)
				yield break;

			foreach (XmlNode child in element)
				if (child.NodeType == XmlNodeType.Element)
					yield return child;
		}

		/// <summary>
		/// Get all elements that match the given names.
		/// Case-insensitive.
		/// </summary>
		public static IEnumerable<XmlNode> Elements
			(this XmlNode element,
			params string[] nodeNames)
		{
			if (element == null)
				yield break;

			foreach (XmlNode child in element)
			{
				if (child.NodeType != XmlNodeType.Element)
					continue;

				if (nodeNames.Contains(child.Name, StringComparer.OrdinalIgnoreCase))
					yield return child;
			}
		}

		public static IEnumerable<XmlNode> Elements(this XmlNode element, Func<XmlNode, bool> predicate)
		{
			if (element == null)
				yield break;

			foreach (XmlNode child in element)
			{
				if (child.NodeType != XmlNodeType.Element)
					continue;

				if (predicate(child))
					yield return child;
			}
		}

		public static bool TryElement
			(this XmlNode element,
			string nodeName,
			out XmlNode value)
		{
			if (element == null)
			{
				value = null;
				return false;
			}

			nodeName = nodeName.ToUpper();

			foreach (XmlNode node in element)
			{
				if (node.NodeType != XmlNodeType.Element)
					continue;

				if (node.Name.ToUpper() != nodeName)
					continue;

				value = node;
				return true;
			}

			value = null;
			return false;
		}

		/// <summary>
		/// Case-insensitive tag name filter.
		/// </summary>
		public static XmlNode Element
			(this XmlNode element,
			string nodeName) =>
			TryElement(element, nodeName, out XmlNode value)
			? value
			: null;

		public static XmlNode Element
			(this XmlNode element,
			params string[] nodeNames)
		{
			foreach (string nodeName in nodeNames)
			{
				foreach (XmlNode node in element)
				{
					element = null;

					if (node.NodeType != XmlNodeType.Element)
						continue;

					if (node.Name.ToUpper() != nodeName.ToUpper())
						continue;

					element = node;
					break;
				}

				if (element == null)
					return null;
			}

			return element;
		}

		/// <summary>
		/// Get an array of strings from the given node's InnerText.
		/// Separator is comma ',' and newline '\n'.
		/// </summary>
		public static string[] StringsText(this XmlNode element, bool uppercase = true)
		{
			if (element == null)
				return new string[0];

			return TextHelper.Strings.Split(element.InnerText, uppercase, ",", "\n").ToArray();
		}

		/// <summary>
		/// Get an array of strings from the node's children.
		/// </summary>
		public static string[] Strings
			(this IEnumerable<XmlNode> elements,
			bool trim = true,
			bool uppercase = true)
		{
			if (elements == null)
				return new string[0];

			List<string> array = new();

			foreach (XmlNode child in elements)
			{
				string value = child.InnerText;

				if (trim)
					value = value.Trim();

				if (uppercase)
					value = value.ToUpper();

				array.Add(value);
			}

			return array.ToArray();
		}

		public static T EnumOf<T>
			(this XmlNode element,
			string nodeName,
			T @default = default)
			where T : struct
		{
			if (element == null)
				return @default;

			XmlNode node = element.Element(nodeName);

			if (node == null)
				return @default;

			if (!Enum.TryParse(node.InnerText.Trim(), true, out T value))
				return @default;

			return value;
		}

		public static string String
			(this XmlNode node,
			bool trim = false,
			bool uppercase = false,
			string @default = "")
		{
			if (node == null)
				return @default;

			string value = node.InnerText;

			if (trim)
				value = value.Trim();

			if (uppercase)
				value = value.ToUpper();

			return value;
		}

		public static string StringOf
			(this XmlNode node,
			string childName,
			bool trim = false,
			bool uppercase = false,
			string @default = "") =>
			String(node.Element(childName), trim, uppercase, @default);

		/// <summary>
		/// Returns a trimmed text with normalized whitespaces.
		/// </summary>
		/*public static string Text
			(this XmlNode element,
			string @default = "") =>
			element != null
			? element.InnerText.NormalizeWhiteSpace()
			: @default;

		public static string TextOf
			(this XmlNode element,
			string nodeName,
			string @default = "") =>
			Text(element.Element(nodeName), @default);*/

		public static int IntOf
			(this XmlNode element,
			string nodeName,
			int @default = 0)
		{
			if (element == null)
				return @default;

			XmlNode node = element.Element(nodeName);

			if (node != null && int.TryParse(node.InnerText, out int value))
				return value;

			return @default;
		}

		public static float FloatOf
			(this XmlNode element,
			string nodeName,
			float @default = 0f)
		{
			if (element == null)
				return @default;

			XmlNode node = element.Element(nodeName);

			if (node != null && float.TryParse(node.InnerText, out float value))
				return value;

			return @default;
		}

		public static bool BoolOf
			(this XmlNode element,
			string nodeName,
			bool @default = false)
		{
			if (element == null)
				return @default;

			XmlNode node = element.Element(nodeName);

			if (node != null && bool.TryParse(node.InnerText, out bool value))
				return value;

			return @default;
		}

		public static Quaternion QuaternionOf
			(this XmlNode node,
			string nodeName,
			Quaternion @default = default)
		{
			if (node == null)
				return @default;

			XmlNode node0 = node.Element(nodeName);

			return
				node0 != null
				? TextHelper.Strings.Quaternion(node0.InnerText)
				: @default;
		}

		public static Vector3 Vector3Of
			(this XmlNode node,
			string nodeName,
			Vector3 @default = default)
		{
			if (node == null)
				return @default;

			XmlNode node0 = node.Element(nodeName);

			return
				node0 != null
				? TextHelper.Strings.Vector3(node0.InnerText)
				: @default;
		}

		public static Vector2 Vector2Of
			(this XmlNode node,
			string nodeName,
			Vector2 @default = default)
		{
			if (node == null)
				return @default;

			XmlNode node0 = node.Element(nodeName);
			return
				node0 != null
				? TextHelper.Strings.Vector2(node0.InnerText)
				: @default;
		}

		public static Vector2Int Vector2IntOf
			(this XmlNode element,
			string nodeName,
			Vector2Int @default = default)
		{
			if (element == null)
				return @default;

			XmlNode node = element.Element(nodeName);
			return node != null ? node.InnerText.Vector2Int() : @default;
		}

		public static Color ColorOf
			(this XmlNode element,
			string nodeName,
			Color @default = default)
		{
			if (element == null)
				return @default;

			XmlNode node = element.Element(nodeName);
			return node != null ? node.InnerText.Color() : @default;
		}

		public static RangeInt[] RangesInt
			(this XmlNode element,
			RangeInt[] @default = null) =>
			element == null
				? @default
				: element.InnerText.ToRangeInts();

		public static Bounds BoundsOf
			(this XmlNode element,
			string nodeName,
			Bounds @default = default)
		{
			XmlNode node = element.Element(nodeName);

			if (node == null)
				return @default;

			return node.InnerText.Bounds();

			/*int[] array = node.InnerText
				.Split(',')
				.Select(v => int.TryParse(v, out int value) ? value : 0)
				.TakeExactly(6);

			return new Bounds(
				new Vector3(array[0], array[1], array[2]),
				new Vector3(array[3], array[4], array[5])
			);*/
		}

		public static Dictionary<string, int> StringIntDictionary
			(this XmlNode element,
			Func<string, string> keyPredicate = null,
			Func<string, string> valuePredicate = null)
		{
			Dictionary<string, int> result = new();

			if (element != null)
				foreach (XmlNode parameter in element.Elements())
				{
					string key = parameter.Name;
					string value = parameter.InnerText;

					if (keyPredicate != null)
						key = keyPredicate(key);

					if (valuePredicate != null)
						value = valuePredicate(value);

					result[key] = int.TryParse(value, out int value0) ? value0 : 0;
				}

			return result;
		}

		public static Dictionary<string, bool> AsBoolDictionary
			(this XmlNode element,
			bool keyUppercase = true)
		{
			Dictionary<string, bool> result = new();

			if (element != null)
				foreach (XmlNode item in element.Elements())
				{
					string key = item.Name;

					if (keyUppercase)
						key = key.ToUpper();

					result[key] = bool.TryParse(item.InnerText, out bool value) && value;
				}

			return result;
		}

		public static XmlAttributeCollection AttributesOf
			(this XmlNode element,
			string nodeName) =>
			element.Element(nodeName)?.Attributes;

		public static IEnumerable<ElementPayload> ElementNodes
			(this IEnumerable<XmlNode> elements,
			Func<string, string> keyPredicate,
			Func<string, string> valuePredicate,
			Func<XmlNode, string, string> nestedPredicate = null)
		{
			foreach (XmlNode element in elements)
			{
				Dictionary<string, string> attributes =
					new(StringComparer.OrdinalIgnoreCase);

				foreach (XmlAttribute attribute in element.Attributes)
					attributes[attribute.Name] = attribute.Value;

				string key = keyPredicate(element.Name);
				string value;

				if (element.EmptyOrText())
					value = valuePredicate(element.InnerText);
				else if (nestedPredicate != null)
					value = nestedPredicate(element, key);
				else
					continue;

				yield return new ElementPayload
				{
					key = key,
					value = value,
					attributes = attributes
				};
			}
		}

		public static IEnumerable<KeyValuePair<TKey, TValue>> KeyValuePairs<TKey, TValue>
			(this IEnumerable<XmlNode> elements,
			Func<string, TKey> keyPredicate,
			Func<string, TValue> valuePredicate,
			Func<XmlNode, TKey, TValue> nestedPredicate = null)
		{
			foreach (XmlNode element in elements)
			{
				TKey key = keyPredicate(element.Name);
				TValue value;

				if (element.EmptyOrText())
					value = valuePredicate(element.InnerText);
				else if (nestedPredicate != null)
					value = nestedPredicate(element, key);
				else
					continue;

				yield return new KeyValuePair<TKey, TValue>(key, value);
			}
		}

		public static Dictionary<string, string> StringDictionary
			(this XmlNode node,
			Dictionary<string, string> dictionary = null,
			Func<XmlNode, string, string> nestedPredicate = null) =>
			StringDictionary(
				node,
				StringComparer.OrdinalIgnoreCase,
				dictionary,
				nestedPredicate
			);

		/// <summary>
		/// Dictionary is case-insensitive by default.
		/// <br></br>
		/// Trims whitespaces on values.
		/// </summary>
		public static Dictionary<string, string> StringDictionary
			(this XmlNode node,
			StringComparer comparer,
			Dictionary<string, string> dictionary = null,
			Func<XmlNode, string, string> nestedPredicate = null)
		{
			dictionary ??= new(comparer);

			if (node != null)
				foreach (XmlNode item in node.Elements())
				{
					string key = item.Name;
					string value = item.InnerText;

					if (item.Elements().Count() > 0 &&
						nestedPredicate != null)
					{
						dictionary[key] = nestedPredicate(item, key);
						continue;
					}

					dictionary[key] = value;
				}

			return dictionary;
		}

		public static Dictionary<string, object> ObjectDictionaryOf
			(this XmlNode node,
			string nodeName,
			DictionaryValueType defaultType = DictionaryValueType.String,
			bool keyUppercase = true)
		{
			Dictionary<string, object> result = new();

			foreach (XmlNode node0 in node.Element(nodeName).Elements())
			{
				bool valid_type =
					Enum.TryParse(
						node0.Attributes
						.String("type"),
						true,
						out DictionaryValueType type
					);
					
				if (!valid_type)
					type = defaultType;
					
				string name = node0.Name;

				if (keyUppercase)
					name = name.ToUpper();

				result[name] = type switch
				{
					DictionaryValueType.Int32
					when int.TryParse(node0.InnerText, out int value0) => value0,
					DictionaryValueType.Boolean
					when bool.TryParse(node0.InnerText, out bool value1) => value1,
					DictionaryValueType.Single
					when float.TryParse(node0.InnerText, out float value2) => value2,
					_ => node0.InnerText,
				};
			}

			return result;
		}

		public static T EnumerableOf<T>
			(this XmlNode element,
			string nodeName,
			T @default = default)
			where T : struct, IConvertible
		{
			XmlNode node = Element(element, nodeName);

			if (node == null)
				return @default;

			if (Enum.TryParse(nodeName, true, out T value))
				return value;

			return default;
		}
	}
}
