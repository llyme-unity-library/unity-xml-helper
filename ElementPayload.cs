using System.Collections.Generic;

namespace XmlHelper
{
	public struct ElementPayload
	{
		public string key;
		public string value;
		public Dictionary<string, string> attributes;

		public KeyValuePair<string, string> Pair =>
			new KeyValuePair<string, string>(key, value);
	}
}
