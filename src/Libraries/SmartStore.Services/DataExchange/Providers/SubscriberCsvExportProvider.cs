using System;
using System.IO;
using System.Text;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Providers
{
	/// <summary>
	/// Exports CSV formatted newsletter subscription data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreNewsSubscriptionCsv")]
	[FriendlyName("SmartStore CSV newsletter subscription export")]
	[IsHidden(true)]
	public class SubscriberCsvExportProvider : ExportProviderBase
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreNewsSubscriptionCsv"; }
		}

		public override ExportEntityType EntityType
		{
			get { return ExportEntityType.NewsLetterSubscription; }
		}

		public override string FileExtension
		{
			get { return "CSV"; }
		}

		protected override void Export(IExportExecuteContext context)
		{
			using (var writer = new StreamWriter(context.DataStream, Encoding.UTF8, 1024, true))
			{
				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic subscriber in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						NewsLetterSubscription entity = subscriber.Entity;

						try
						{
							var row = "{0},{1},{2}".FormatInvariant(
								entity.Email.ReplaceCsvChars(),
								entity.Active.ToString(),
								entity.StoreId
							);

							writer.WriteLine(row);

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, entity.Id);
						}
					}
				}
			}
		}
	}
}
