using System;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Providers
{
	/// <summary>
	/// Exports XML formatted customer data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreCustomerXml")]
	[FriendlyName("SmartStore XML customer export")]
	[IsHidden(true)]
	public class CustomerXmlExportProvider : ExportProviderBase
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreCustomerXml"; }
		}

		public override ExportEntityType EntityType
		{
			get { return ExportEntityType.Customer; }
		}

		public override string FileExtension
		{
			get { return "XML"; }
		}

		public override void Execute(IExportExecuteContext context)
		{
			using (var helper = new ExportXmlHelper(context.DataStream))
			{
				helper.Writer.WriteStartDocument();
				helper.Writer.WriteStartElement("Customers");
				helper.Writer.WriteAttributeString("Version", SmartStoreVersion.CurrentVersion);

				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic customer in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						try
						{
							helper.WriteCustomer(customer, "Customer");

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)customer.Id);
						}
					}
				}

				helper.Writer.WriteEndElement();	// Customers
				helper.Writer.WriteEndDocument();
			}
		}
	}
}
