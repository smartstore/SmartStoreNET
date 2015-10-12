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
	/// <summary>
	/// Exports XML formatted manufacturer data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreManufacturerXml")]
	[FriendlyName("SmartStore XML manufacturer export")]
	[IsHidden(true)]
	public class ExportManufacturerXmlProvider : IExportProvider
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreManufacturerXml"; }
		}

		public ExportConfigurationInfo ConfigurationInfo
		{
			get { return null; }
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.Manufacturer; }
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

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var writer = XmlWriter.Create(stream, settings))
			{
				var xmlHelper = new ExportXmlHelper(writer, CultureInfo.InvariantCulture);

				writer.WriteStartDocument();
				writer.WriteStartElement("Manufacturers");
				writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic manufacturer in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						writer.WriteStartElement("Manufacturer");

						try
						{
							xmlHelper.WriteManufacturer(manufacturer, null);

							writer.WriteStartElement("ProductManufacturers");
							foreach (dynamic productManu in manufacturer.ProductManufacturers)
							{
								writer.WriteStartElement("ProductManufacturer");
								writer.Write("Id", ((int)productManu.Id).ToString());
								writer.Write("ProductId", ((int)productManu.ProductId).ToString());
								writer.Write("DisplayOrder", ((int)productManu.DisplayOrder).ToString());
								writer.Write("IsFeaturedProduct", ((bool)productManu.IsFeaturedProduct).ToString());
								writer.Write("ManufacturerId", ((int)productManu.ManufacturerId).ToString());
								writer.WriteEndElement();	// ProductManufacturer
							}
							writer.WriteEndElement();	// ProductManufacturers

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)manufacturer.Id);
						}

						writer.WriteEndElement();	// Manufacturer
					}
				}

				writer.WriteEndElement();	// Manufacturers
				writer.WriteEndDocument();
			}
		}

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}
