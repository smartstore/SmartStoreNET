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
	/// Exports XML formatted customer data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreCustomerXml")]
	[FriendlyName("SmartStore XML customer export")]
	[IsHidden(true)]
	public class ExportCustomerXmlProvider : IExportProvider
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreCustomerXml"; }
		}

		public ExportConfigurationInfo ConfigurationInfo
		{
			get { return null; }
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.Customer; }
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
				writer.WriteStartElement("Customers");
				writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic customer in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						try
						{
							xmlHelper.WriteCustomer(customer, "Customer");

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)customer.Id);
						}
					}
				}

				writer.WriteEndElement();	// Customers
				writer.WriteEndDocument();
			}
		}

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}
