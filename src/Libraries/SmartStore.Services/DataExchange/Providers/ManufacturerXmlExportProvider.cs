using System;
using System.IO;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Providers
{
	/// <summary>
	/// Exports XML formatted manufacturer data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreManufacturerXml")]
	[FriendlyName("SmartStore XML manufacturer export")]
	[IsHidden(true)]
	public class ManufacturerXmlExportProvider : ExportProviderBase
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreManufacturerXml"; }
		}

		public override ExportEntityType EntityType
		{
			get { return ExportEntityType.Manufacturer; }
		}

		public override string FileExtension
		{
			get { return "XML"; }
		}

		public override void Execute(IExportExecuteContext context)
		{
			var path = context.FilePath;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var helper = new ExportXmlHelper(stream))
			{
				helper.Writer.WriteStartDocument();
				helper.Writer.WriteStartElement("Manufacturers");
				helper.Writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic manufacturer in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						helper.Writer.WriteStartElement("Manufacturer");

						try
						{
							helper.WriteManufacturer(manufacturer, null);

							helper.Writer.WriteStartElement("ProductManufacturers");
							foreach (dynamic productManu in manufacturer.ProductManufacturers)
							{
								helper.Writer.WriteStartElement("ProductManufacturer");
								helper.Writer.Write("Id", ((int)productManu.Id).ToString());
								helper.Writer.Write("ProductId", ((int)productManu.ProductId).ToString());
								helper.Writer.Write("DisplayOrder", ((int)productManu.DisplayOrder).ToString());
								helper.Writer.Write("IsFeaturedProduct", ((bool)productManu.IsFeaturedProduct).ToString());
								helper.Writer.Write("ManufacturerId", ((int)productManu.ManufacturerId).ToString());
								helper.Writer.WriteEndElement();	// ProductManufacturer
							}
							helper.Writer.WriteEndElement();	// ProductManufacturers

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)manufacturer.Id);
						}

						helper.Writer.WriteEndElement();	// Manufacturer
					}
				}

				helper.Writer.WriteEndElement();	// Manufacturers
				helper.Writer.WriteEndDocument();
			}
		}
	}
}
