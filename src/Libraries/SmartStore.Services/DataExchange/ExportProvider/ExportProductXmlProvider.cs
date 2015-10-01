using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.ExportProvider
{
	[SystemName("Exports.SmartStoreNetProductXml")]
	[FriendlyName("SmartStore.NET XML product export")]
	[IsHidden(true)]
	[ExportSupporting(ExportSupport.HighDataDepth)]
	public class ExportProductXmlProvider : IExportProvider
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreNetProductXml"; }
		}

		public ExportConfigurationInfo ConfigurationInfo
		{
			get { return null; }
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.Product; }
		}

		public string FileExtension
		{
			get { return "XML"; }
		}

		public void Execute(IExportExecuteContext context)
		{
			var settings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				CheckCharacters = false,
				Indent = true,
				IndentChars = "\t"
			};

			var path = context.FilePath;
			var invariantCulture = CultureInfo.InvariantCulture;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var writer = XmlWriter.Create(stream, settings))
			{
				var xmlHelper = new ExportXmlHelper(writer, invariantCulture);

				writer.WriteStartDocument();
				writer.WriteStartElement("Products");
				writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Data.ReadNextSegment())
				{
					var segment = context.Data.CurrentSegment;

					foreach (dynamic product in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						int productId = product.Id;

						try
						{
							xmlHelper.WriteProduct(product, "Product");

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.Log.Error("Error while processing product with id {0}: {1}".FormatInvariant(productId, exc.ToAllMessages()), exc);
							++context.RecordsFailed;
						}
					}
				}

				writer.WriteEndElement();	// Products
				writer.WriteEndDocument();
			}
		}

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}
