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
			get;
			internal set;
		}

		[JsonIgnore]
		public bool HasRegularStyle
		{
			get;
			internal set;
		}

		[JsonIgnore]
		public bool IsPro
		{
			get;
			internal set;
		}

		public string GetCssClass(string style)
		{
			var prefix = "fa";

			if (IsBrandIcon)
			{
				prefix = "fab";
			}
			else
			{
				switch (style)
				{
					case "solid":
					case "fas":
						prefix = "fas";
						break;
					case "regular":
					case "far":
						prefix = "far";
						break;
					case "light":
					case "fal":
						prefix = "fal";
						break;
				}
			}

			return string.Concat(prefix, " fa-", Name);
		}

		public class Search
		{
			[JsonProperty("terms")]
			public string[] Terms { get; set; }
		}
	}
}
