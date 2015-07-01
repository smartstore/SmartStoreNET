using System;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Xml.Linq;
using SmartStore.Core.Domain.Localization;

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
				this.AttributeExtensions.Add(new XmlQualifiedName("content", XNamespace.Xmlns.ToString()), UrlPurlContent);
		}

		public void Init(string selfLink, Language language = null)
		{
			this.ElementExtensions.Add(new XElement(((XNamespace)UrlAtom) + "link", new XAttribute("href", selfLink), new XAttribute("rel", "self"), new XAttribute("type", "application/rss+xml")));

			if (language != null)
				this.Language = language.LanguageCulture.EmptyNull().ToLower();
		}
	}
}
