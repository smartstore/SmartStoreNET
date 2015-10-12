using System;
using System.IO;
using System.Text;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.ExportProvider
{
	/// <summary>
	/// Exports CSV formatted newsletter subscription data to a file
	/// </summary>
	[SystemName("Exports.SmartStoreNewsSubscriptionCsv")]
	[FriendlyName("SmartStore CSV newsletter subscription export")]
	[IsHidden(true)]
	public class ExportNewsSubscriptionCsvProvider : IExportProvider
	{
		public static string SystemName
		{
			get { return "Exports.SmartStoreNewsSubscriptionCsv"; }
		}

		public ExportConfigurationInfo ConfigurationInfo
		{
			get { return null; }
		}

		public ExportEntityType EntityType
		{
			get { return ExportEntityType.NewsLetterSubscription; }
		}

		public string FileExtension
		{
			get { return "CSV"; }
		}

		public void Execute(IExportExecuteContext context)
		{
			var path = context.FilePath;

			context.Log.Information("Creating file " + path);

			using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
			using (var writer = new StreamWriter(stream, Encoding.UTF8))
			{
				while (context.Abort == ExportAbortion.None && context.Segmenter.ReadNextSegment())
				{
					var segment = context.Segmenter.CurrentSegment;

					foreach (dynamic subscriber in segment)
					{
						if (context.Abort != ExportAbortion.None)
							break;

						try
						{
							var row = "{0},{1},{2}".FormatInvariant(
								((string)subscriber.Email).ReplaceCsvChars(),
								((bool)subscriber.Active).ToString(),
								(int)subscriber.StoreId
							);

							writer.WriteLine(row);

							++context.RecordsSucceeded;
						}
						catch (Exception exc)
						{
							context.RecordException(exc, (int)subscriber.Id);
						}
					}
				}
			}
		}

		public void ExecuteEnded(IExportExecuteContext context)
		{
			// nothing to do
		}
	}
}
