using System;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;

namespace SmartStore.Utilities
{
	public class SmartSyndicationFeed : SyndicationFeed
	{
		public SmartSyndicationFeed(Uri feedAlternateLink, string title, string description = null)
			: base(title, description ?? title, feedAlternateLink, null, DateTime.UtcNow)
		{
		}

		public static string UrlAtom { get { return "http://www.w3.org/2005/Atom"; } }
		public static string UrlPurlContent { get { return "http://purl.org/rss/1.0/modules/content/"; } }

		public void AddNamespaces(bool purlContent)
		{
			this.AttributeExtensions.Add(new XmlQualifiedName("atom", XNamespace.Xmlns.NamespaceName), UrlAtom);

			if (purlContent)
			{
				this.AttributeExtensions.Add(new XmlQualifiedName("content", XNamespace.Xmlns.ToString()), UrlPurlContent);
			}
		}

		public void Init(string selfLink, Language language = null)
		{
			this.ElementExtensions.Add(
				new XElement(((XNamespace)UrlAtom) + "link",
					new XAttribute("href", selfLink),
					new XAttribute("rel", "self"),
					new XAttribute("type", "application/rss+xml")));

			if (language != null)
			{
				this.Language = language.LanguageCulture.EmptyNull().ToLower();
			}
		}

		public SyndicationItem CreateItem(string title, string synopsis, string url, DateTimeOffset published, string contentEncoded = null)
		{
			var item = new SyndicationItem(
				title.RemoveInvalidXmlChars().EmptyNull(),
				synopsis.RemoveInvalidXmlChars().EmptyNull(),
				new Uri(url),
				url,
				published);

			if (contentEncoded != null)
			{
				item.ElementExtensions.Add("encoded", UrlPurlContent, contentEncoded.RemoveInvalidXmlChars().EmptyNull());
			}

			return item;
		}

		public bool AddEnclosure(SyndicationItem item, Picture picture, string pictureUrl)
		{
			if (picture != null && pictureUrl.HasValue())
			{
				// 0 omits the length attribute but that invalidates the feed
				long pictureLength = 10000;

				if ((picture.MediaStorageId ?? 0) != 0)
				{
					// TODO: (mc) But what about other storage provider?
					// do not care about storage provider
					pictureLength = picture.MediaStorage.Data.LongLength;
				}

				var enclosure = SyndicationLink.CreateMediaEnclosureLink(new Uri(pictureUrl), picture.MimeType.EmptyNull(), pictureLength);

				item.Links.Add(enclosure);

				return true;
			}
			return false;
		}
	}
}
