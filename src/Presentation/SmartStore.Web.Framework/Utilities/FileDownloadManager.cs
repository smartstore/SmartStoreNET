using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using SmartStore.Core.Logging;

namespace SmartStore.Web.Framework.Utilities
{
	public class FileDownloadManager
	{
		private const int _bufferSize = 16384;

		private async Task ProcessUrl(FileDownloadManagerContext context, HttpClient client, FileDownloadManagerItem item)
		{
			try
			{
				Task<Stream> task = client.GetStreamAsync(item.Url);
				await task;

				int count;
				bool canceled = false;
				byte[] bytes = new byte[_bufferSize];

				using (var srcStream = task.Result)
				using (var dstStream = File.OpenWrite(item.Path))
				{
					while ((count = srcStream.Read(bytes, 0, bytes.Length)) != 0 && !canceled)
					{
						dstStream.Write(bytes, 0, count);

						if (context.CancellationToken.IsCancellationRequested)
							canceled = true;
					}
				}

				item.Success = (!task.IsFaulted && !canceled);
			}
			catch (Exception exc)
			{
				item.Success = false;
				item.ErrorMessage = exc.ToAllMessages();
				
				var webExc = exc.InnerException as WebException;
				if (webExc != null)
					item.ExceptionStatus = webExc.Status;

				if (context.Logger != null)
					context.Logger.Error(item.ToString(), exc);
			}
		}

		private async Task DownloadFiles(FileDownloadManagerContext context)
		{
			var client = new HttpClient();
			
			client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue();
			client.DefaultRequestHeaders.CacheControl.NoCache = true;
			client.DefaultRequestHeaders.Add("Connection", "Keep-alive");

			if (context.Timeout != null)
				client.Timeout = context.Timeout;

			IEnumerable<Task> downloadTasksQuery =
				from item in context.Items
				select ProcessUrl(context, client, item);

			// now execute the bunch
			List<Task> downloadTasks = downloadTasksQuery.ToList();

			while (downloadTasks.Count > 0)
			{
				// identify the first task that completes
				Task firstFinishedTask = await Task.WhenAny(downloadTasks);

				// process only once
				downloadTasks.Remove(firstFinishedTask);

				await firstFinishedTask;
			}
		}

		/// <summary>
		/// Start asynchronous download of files
		/// </summary>
		public async Task Start(FileDownloadManagerContext context)
		{
			await DownloadFiles(context);
		}
	}


	public class FileDownloadManagerContext
	{
		/// <summary>
		/// Items to be downloaded
		/// </summary>
		public List<FileDownloadManagerItem> Items { get; set; }

		/// <summary>
		/// Optional logger to log errors
		/// </summary>
		public TraceLogger Logger { get; set; }

		/// <summary>
		/// Cancellation token
		/// </summary>
		public CancellationTokenSource CancellationToken { get; set; }

		/// <summary>
		/// Timeout for the HTTP client
		/// </summary>
		public TimeSpan Timeout { get; set; }
	}

	public class FileDownloadManagerItem
	{
		/// <summary>
		/// Identifier of the item
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// New identifier of the downloaded item
		/// </summary>
		public int NewId { get; set; }

		/// <summary>
		/// Display order of the item
		/// </summary>
		public int DisplayOrder { get; set; }

		/// <summary>
		/// Download URL
		/// </summary>
		public string Url { get; set; }

		/// <summary>
		/// Absolute path for saving the item
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// File name without file extension
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Mime type of the item
		/// </summary>
		public string MimeType { get; set; }

		/// <summary>
		/// Whether the operation succeeded
		/// </summary>
		public bool? Success { get; set; }

		/// <summary>
		/// Exception status if an exception of type WebException occurred
		/// </summary>
		public WebExceptionStatus ExceptionStatus { get; set; }

		/// <summary>
		/// Error message
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Whether the operation timed out
		/// </summary>
		public bool HasTimedOut
		{
			get
			{
				return ExceptionStatus == WebExceptionStatus.Timeout || ExceptionStatus == WebExceptionStatus.RequestCanceled;
			}
		}

		/// <summary>
		/// Use dictionary for any required extra data
		/// </summary>
		public IDictionary<string, object> CustomProperties { get; set; }

		public override string ToString()
		{
			string str = "Result: {0} {1}{2}, {3}".FormatInvariant(
				Success,
				ExceptionStatus.ToString(),
				ErrorMessage.HasValue() ? " ({0})".FormatInvariant(ErrorMessage) : "",
				Path);

			return str;
		}
	}
}