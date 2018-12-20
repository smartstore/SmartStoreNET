using System;
using System.Linq;
using Newtonsoft.Json;

namespace SmartStore.Web.Framework.UI
{
	public class IconDescription
	{
		[JsonIgnore]
		public string Name { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }

		[JsonProperty("unicode")]
		public string Unicode { get; set; }

		[JsonProperty("styles")]
		public string[] Styles { get; set; }

		[JsonProperty("search")]
		public Search SearchInfo { get; set; }

		[JsonIgnore]
		public bool IsBrandIcon
		{
			get
			{
				if (Styles != null && Styles.Length == 1)
				{
					return Styles[0] == "brands";
				}

				return false;
			}
		}

		[JsonIgnore]
		public bool HasRegularStyle
		{
			get
			{
				if (Styles != null)
				{
					return Styles.Contains("regular");
				}

				return false;
			}
		}

		public class Search
		{
			[JsonProperty("terms")]
			public string[] Terms { get; set; }
		}
	}
}
