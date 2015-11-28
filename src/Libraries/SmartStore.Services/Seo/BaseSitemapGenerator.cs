using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Data;

namespace SmartStore.Services.Seo
{
	/// <summary>
	/// Represents a base sitemap generator
	/// </summary>
	public abstract partial class BaseSitemapGenerator : ISitemapGenerator
	{
		#region Fields

		private const string DateFormat = @"yyyy-MM-dd";
		private XmlTextWriter _writer;

		#endregion

		#region Utilities

		protected abstract void GenerateUrlNodes(UrlHelper urlHelper);

		protected void WriteUrlLocation(string url, UpdateFrequency updateFrequency, DateTime lastUpdated)
		{
			if (url.IsEmpty())
				return;

			string loc = XmlHelper.XmlEncode(url);
			if (url.IsEmpty())
				return;

			_writer.WriteStartElement("url");
			_writer.WriteElementString("loc", loc);
			//_writer.WriteElementString("changefreq", updateFrequency.ToString().ToLowerInvariant());
			_writer.WriteElementString("lastmod", lastUpdated.ToString(DateFormat));
			_writer.WriteEndElement();
		}

		#endregion

		#region Methods

		public string Generate(UrlHelper urlHelper)
		{
			using (var stream = new MemoryStream())
			{
				Generate(urlHelper, stream);
				return Encoding.UTF8.GetString(stream.ToArray());
			}
		}

		public void Generate(UrlHelper urlHelper, Stream stream)
		{
			using (var scope = new DbContextScope(autoDetectChanges: false, forceNoTracking: true))
			{
				_writer = new XmlTextWriter(stream, Encoding.UTF8);
				_writer.Formatting = Formatting.Indented;
				_writer.WriteStartDocument();
				_writer.WriteStartElement("urlset");
				_writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
				_writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
				_writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

				GenerateUrlNodes(urlHelper);

				_writer.WriteEndElement();
				_writer.Close();
			}
		}

		#endregion

	}
}
