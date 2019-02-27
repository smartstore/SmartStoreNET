using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SmartStore.Core.IO;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Modelling
{
	public class CachedFileResult : FileResult
	{
		// Default buffer size as defined in BufferedStream type
		private const int BufferSize = 0x1000;

		private HttpContextBase _httpContext;
		private string _etag;

		private readonly string _path;
		private readonly Func<Stream> _streamReader;
		private readonly Func<byte[]> _bufferReader;

		public CachedFileResult(FileInfo file, Func<Stream> reader = null)
			: this(CreateETag(file), MimeTypes.MapNameToMimeType(file.Name), reader ?? file.OpenRead)
		{
		}

		public CachedFileResult(IFile file, Func<Stream> reader = null)
			: this(CreateETag(file), MimeTypes.MapNameToMimeType(file.Name), reader ?? file.OpenRead)
		{
		}

		public CachedFileResult(VirtualFile file, Func<Stream> reader = null)
			: this(CreateETag(file), MimeTypes.MapNameToMimeType(file.Name), reader ?? file.Open)
		{
		}

		public CachedFileResult(string etag, string contentType, Func<Stream> reader)
			: base(contentType)
		{
			Guard.NotNull(reader, nameof(reader));
			SanitizeETag(etag);

			_streamReader = reader;
		}

		public CachedFileResult(string etag, string contentType, Func<byte[]> reader)
			: base(contentType)
		{
			Guard.NotNull(reader, nameof(reader));
			SanitizeETag(etag);

			_bufferReader = reader;
		}

		private void SanitizeETag(string etag)
		{
			Guard.NotEmpty(etag, nameof(etag));

			if (etag[0] != '"')
			{
				etag = string.Concat("\"", etag, "\"");
			}

			_etag = etag;
		}

		public DateTime? LastModifiedUtc { get; set; }

		public DateTime Expiration { get; set; } = DateTime.UtcNow.AddDays(7);

		public TimeSpan MaxAge { get; set; } = TimeSpan.FromDays(7);

		public static string CreateETag(VirtualFile file)
		{
			Guard.NotNull(file, nameof(file));

			var fi = new FileInfo(CommonHelper.MapPath(file.VirtualPath));
			return CreateETag(fi);
		}

		public static string CreateETag(FileInfo file)
		{
			Guard.NotNull(file, nameof(file));

			if (!file.Exists)
			{
				throw new FileNotFoundException("File to create ETag for must exist.", file.FullName);
			}

			return CreateETag(file.FullName, file.Length.GetHashCode(), file.CreationTimeUtc, file.LastWriteTimeUtc);
		}

		public static string CreateETag(IFile file)
		{
			Guard.NotNull(file, nameof(file));

			if (!file.Exists)
			{
				throw new FileNotFoundException("File to create ETag for must exist.", file.Path);
			}

			return CreateETag(file.Path, file.Size.GetHashCode(), file.LastUpdated);
		}

		public static string CreateETag(params object[] tokens)
		{
			var sb = new StringBuilder();
			var tag = string.Empty;

			foreach (var token in tokens)
			{
				if (token is DateTime dt)
				{
					tag += dt.ToUnixTime().ToString();
				}
				else
				{
					tag += token.Convert<string>();
				}
			}

			return "\"" + tag.Hash(Encoding.UTF8) + "\"";
		}

		public override void ExecuteResult(ControllerContext context)
		{
			_httpContext = context.HttpContext;
			base.ExecuteResult(context);
		}

		protected override void WriteFile(HttpResponseBase response)
		{
			var ifNoneMatch = _httpContext.Request.Headers["If-None-Match"];
			if (ifNoneMatch.HasValue() && _etag == ifNoneMatch)
			{
				// File hasn't changed, so return HTTP 304 without retrieving the data
				response.StatusCode = (int)HttpStatusCode.NotModified;
				response.StatusDescription = "Not Modified";

				// Explicitly set the Content-Length header so the client doesn't wait for
				// content but keeps the connection open for other requests
				response.AddHeader("Content-Length", "0");

				ApplyResponseHeaders(response, false);
			}
			else
			{
				if (_path != null)
				{
					response.TransmitFile(_path);
				}
				else if (_streamReader != null)
				{
					var stream = _streamReader();
					if (stream == null)
					{
						throw new NullReferenceException("File stream cannot be NULL.");
					}

					// Grab chunks of data and write to the output stream
					var outputStream = response.OutputStream;
					using (stream)
					{
						var buffer = new byte[BufferSize];

						while (true)
						{
							int bytesRead = stream.Read(buffer, 0, BufferSize);
							if (bytesRead == 0)
							{
								// no more data
								break;
							}

							outputStream.Write(buffer, 0, bytesRead);
						}
					}
				}
				else if (_bufferReader != null)
				{
					var buffer = _bufferReader();
					if (buffer == null)
					{
						throw new NullReferenceException("File buffer cannot be NULL.");
					}

					// Write buffer to output stream
					response.OutputStream.Write(buffer, 0, buffer.Length);
				}

				ApplyResponseHeaders(response, true);

				// Set ETag for served file (revalidated on subsequent requests)
				response.Cache.SetETag(_etag);
			}
		}

		private void ApplyResponseHeaders(HttpResponseBase response, bool setLastModifiedDate)
		{
			var cache = response.Cache;

			cache.SetCacheability(System.Web.HttpCacheability.Public);
			cache.VaryByHeaders["Accept-Encoding"] = true;
			cache.SetExpires(Expiration);
			cache.SetMaxAge(MaxAge);
			cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

			if (setLastModifiedDate && LastModifiedUtc.HasValue)
			{
				cache.SetLastModified(LastModifiedUtc.Value);
			}
		}
	}
}
