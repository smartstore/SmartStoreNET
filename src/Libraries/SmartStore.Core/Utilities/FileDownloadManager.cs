using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.IO;
using SmartStore.Core.Logging;

namespace SmartStore.Utilities
{
	public class FileDownloadManager
	{
		private const int _bufferSize = 16384;

		private readonly HttpRequestBase _httpRequest;

		public FileDownloadManager(HttpRequestBase httpRequest)
		{
			_httpRequest = httpRequest;
		}

		/// <summary>
		/// Downloads a single file synchronously
		/// </summary>
		/// <param name="url">The URL to download the file from (either a fully qualified URL or an app relative/absolute path)</param>
		/// <param name="sendAuthCookie">Specifies whether the FormsAuthenticationCookie should be sent</param>
		/// <param name="timeout">Timeout in milliseconds</param>
		/// <param name="isLocal">Specifiers whether the file is located on the local server</param>
		public FileDownloadResponse DownloadFile(string url, bool sendAuthCookie = false, int? timeout = null, bool isLocal = false)
		{
			Guard.NotEmpty(url, nameof(url));
			
			url = WebHelper.GetAbsoluteUrl(url, _httpRequest);

			HttpWebRequest req;

			if (isLocal)
			{
				req = WebHelper.CreateHttpRequestForSafeLocalCall(new Uri(url));
			}
			else
			{
				req = WebRequest.CreateHttp(url);
				req.UserAgent = "SmartStore.NET";
			}

			if (timeout.HasValue)
			{
				req.Timeout = timeout.Value;
			}

			if (sendAuthCookie)
			{
				req.SetFormsAuthenticationCookie(_httpRequest);
				req.SetAnonymousIdentCookie(_httpRequest);
			}

			using (var resp = (HttpWebResponse)req.GetResponse())
			using (var stream = resp.GetResponseStream())
			{
				if (resp.StatusCode == HttpStatusCode.OK)
				{
					var data = stream.ToByteArray();
					if (data != null && data.Length != 0)
					{
						string fileName = null;

						var contentDisposition = resp.Headers["Content-Disposition"];
						if (contentDisposition.HasValue())
						{
							var cd = new ContentDisposition(contentDisposition);
							fileName = cd.FileName;
						}

						if (fileName.IsEmpty())
						{
							try
							{
								var uri = new Uri(url);
								fileName = Path.GetFileName(uri.LocalPath);
							}
							catch { }
						}

						return new FileDownloadResponse(data, fileName, resp.ContentType);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Starts asynchronous download of files and saves them to disk
		/// </summary>
		/// <param name="context">Download context</param>
		/// <param name="items">Items to be downloaded</param>
		public async Task DownloadAsync(FileDownloadManagerContext context, IEnumerable<FileDownloadManagerItem> items)
		{
			await DownloadFiles(context, items);
		}

		private async Task DownloadFiles(FileDownloadManagerContext context, IEnumerable<FileDownloadManagerItem> items)
		{
			try
			{
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue();
					client.DefaultRequestHeaders.CacheControl.NoCache = true;
					client.DefaultRequestHeaders.Add("Connection", "Keep-alive");

					if (context.Timeout.TotalMilliseconds > 0 && context.Timeout != Timeout.InfiniteTimeSpan)
						client.Timeout = context.Timeout;

					IEnumerable<Task> downloadTasksQuery =
						from item in items
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
			}
			catch (Exception exception)
			{
				if (context.Logger != null)
					context.Logger.ErrorsAll(exception);
			}
		}

		private async Task ProcessUrl(FileDownloadManagerContext context, HttpClient client, FileDownloadManagerItem item)
		{
			try
			{
				var count = 0;
				var canceled = false;
				var bytes = new byte[_bufferSize];

				using (var response = await client.GetAsync(item.Url))
				{
					if (response.IsSuccessStatusCode && response.Content.Headers.ContentType != null)
					{
						var contentType = response.Content.Headers.ContentType.MediaType;
						if (contentType.HasValue() && !contentType.IsCaseInsensitiveEqual(item.MimeType))
						{
							// Update mime type and local path.
							var extension = MimeTypes.MapMimeTypeToExtension(contentType).NullEmpty() ?? ".jpg";

							item.MimeType = contentType;
							item.Path = Path.ChangeExtension(item.Path, extension.EnsureStartsWith("."));
						}
					}

					//Task <Stream> task = client.GetStreamAsync(item.Url);
					Task<Stream> task = response.Content.ReadAsStreamAsync();
					await task;

					using (var srcStream = task.Result)
					using (var dstStream = File.Open(item.Path, FileMode.Create))
					{
						while ((count = srcStream.Read(bytes, 0, bytes.Length)) != 0 && !canceled)
						{
							dstStream.Write(bytes, 0, count);

							if (context.CancellationToken != null && context.CancellationToken.IsCancellationRequested)
							{
								canceled = true;
							}
						}
					}

					item.Success = (!task.IsFaulted && !canceled);
				}
			}
			catch (Exception exception)
			{
				try
				{
					item.Success = false;
					item.ErrorMessage = exception.ToAllMessages();

					var webExc = exception.InnerException as WebException;
					if (webExc != null)
						item.ExceptionStatus = webExc.Status;

					if (context.Logger != null)
						context.Logger.Error(exception, item.ToString());
				}
				catch { }
			}
		}
	}


	public class FileDownloadResponse
	{
		public FileDownloadResponse(byte[] data, string fileName, string contentType)
		{
			Guard.NotNull(data, nameof(data));

			Data = data;
			FileName = fileName;
			ContentType = contentType;
		}
		
		/// <summary>
		/// The downloaded file's byte array
		/// </summary>
		public byte[] Data { get; private set; }

		/// <summary>
		/// The file name
		/// </summary>
		public string FileName { get; private set; }

		/// <summary>
		/// The mime type opf the downloaded file
		/// </summary>
		public string ContentType { get; private set; }
	}

	public class FileDownloadManagerContext
	{
		/// <summary>
		/// Optional logger to log errors
		/// </summary>
		public ILogger Logger { get; set; }

		/// <summary>
		/// Cancellation token
		/// </summary>
		public CancellationToken CancellationToken { get; set; }

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
			var str = "Result: {0} {1}{2}, {3}".FormatInvariant(
				Success,
				ExceptionStatus.ToString(),
				ErrorMessage.HasValue() ? " ({0})".FormatInvariant(ErrorMessage) : "",
				Path);

			return str;
		}
	}
}