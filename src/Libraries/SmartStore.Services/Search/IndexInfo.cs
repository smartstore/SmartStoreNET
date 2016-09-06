using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public enum IndexingStatus
	{
		Unavailable = -1,
		Idle = 0,
		Rebuilding = 1,
		Updating = 2
	}

	public class IndexInfo
	{
		public string Scope { get; set; }
		public int DocumentCount { get; set; }
		public IEnumerable<string> Fields { get; set; }

		// loaded from status file
		public DateTime? LastIndexedUtc { get; set; }
		public IndexingStatus Status { get; set; }

		public string ToXml()
		{
			return new XDocument(
					new XElement("info",
						new XElement("status", this.Status),
						new XElement("last-indexed-utc", LastIndexedUtc?.ToString("u"))
			)).ToString();
		}

		public static IndexInfo FromXml(string xml)
		{
			var info = new IndexInfo();

			try
			{
				var doc = XDocument.Parse(xml);

				var lastIndexed = doc.Descendants("last-indexed-utc").FirstOrDefault()?.Value;
				if (lastIndexed.HasValue())
				{
					info.LastIndexedUtc = lastIndexed.Convert<DateTime?>()?.ToUniversalTime();
				}

				var status = doc.Descendants("status").FirstOrDefault()?.Value;
				if (status.HasValue())
				{
					info.Status = status.Convert<IndexingStatus>();
				}

				return info;
			}
			catch
			{
				return info;
			}
		}
	}
}
