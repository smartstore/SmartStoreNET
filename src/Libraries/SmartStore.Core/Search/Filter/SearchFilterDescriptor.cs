using System;
using System.Xml.Serialization;

namespace SmartStore.Core.Search.Filter
{
	[Serializable]
	public class SearchFilterDescriptor
	{
		public bool Enabled { get; set; }

		public int DisplayOrder { get; set; }

		public string FieldName { get; set; }

		[XmlIgnore]
		public string FriendlyName { get; set; }
	}
}
