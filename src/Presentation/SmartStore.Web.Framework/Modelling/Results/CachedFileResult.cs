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

		public CachedFileResult(string path, string contentType = null)
			: base(contentType ?? MimeTypes.MapNameToMimeType(path))
		{
			_etag = GenerateETag(path);
			_path = path;
		}

		public CachedFileResult(FileInfo file, string contentType = null, Func<Stream> reader = null)
			: this(GenerateETag(file), contentType ?? MimeTypes.MapNameToMimeType(file.Name), reader ?? file.OpenRead)
		{
			LastModifiedUtc = file.LastWriteTimeUtc;
		}

		public CachedFileResult(IFile file, string contentType = null, Func<Stream> reader = null)
			: this(GenerateETag(file), contentType ?? MimeTypes.MapNameToMimeType(file.Name), reader ?? file.OpenRead)
		{
			LastModifiedUtc = file.LastUpdated;
		}

		public CachedFileResult(string etag, IFile file, string contentType = null, Func<Stream> reader = null)
			: this(etag, contentType ?? MimeTypes.MapNameToMimeType(file.Name), reader ?? file.OpenRead)
		{
			LastModifiedUtc = file.LastUpdated;
		}

		public CachedFileResult(VirtualFile file, string contentType = null, Func<Stream> reader = null)
			: this(GenerateETag(file), contentType ?? MimeTypes.MapNameToMimeType(file.Name), reader ?? file.Open)
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

		#region ETag generators

		public static string GenerateETag(string path)
		{
			Guard.NotEmpty(path, nameof(path));

			if (!PathHelper.IsAbsolutePhysicalPath(path))
			{
				path = CommonHelper.MapPath(path);
			}

			return GenerateETag(new FileInfo(path));
		}

		public static string GenerateETag(VirtualFile file)
		{
			Guard.NotNull(file, nameof(file));

			var fi = new FileInfo(CommonHelper.MapPath(file.VirtualPath));
			return GenerateETag(fi);
		}

		public static string GenerateETag(FileInfo file)
		{
			Guard.NotNull(file, nameof(file));

			if (!file.Exists)
			{
				throw new FileNotFoundException("File to create ETag for must exist.", file.FullName);
			}

			return GenerateETag(file.FullName, file.Length.GetHashCode(), file.LastWriteTimeUtc);
		}

		public static string GenerateETag(IFile file)
		{
			Guard.NotNull(file, nameof(file));

			if (!file.Exists)
			{
				throw new FileNotFoundException("File to create ETag for must exist.", file.Path);
			}

			return GenerateETag(file.Path, file.Size.GetHashCode(), file.LastUpdated);
		}

		public static string GenerateETag(params object[] tokens)
		{
			var sb = new StringBuilder();
			var tag = string.Empty;

			foreach (var token in tokens)
			{
				if (token is DateTime dt)
				{
					tag += FixLastModifiedDate(dt).ToUnixTime().ToString();
				}
				else
				{
					tag += token.Convert<string>();
				}
			}

			return "\"" + tag.Hash(Encoding.UTF8) + "\"";
		}

		private static DateTime FixLastModifiedDate(DateTime date)
		{
			var result = new DateTime(
				date.Year,
				date.Month,
				date.Day,
				date.Hour,
				date.Minute,
				date.Second,
				0,
				DateTimeKind.Utc);

			// Because we can't set a "Last-Modified" header to any time
			// in the future, check the last modified time and set it to
			// DateTime.Now if it's in the future. 
			// This is to fix VSWhidbey #402323
			DateTime utcNow = DateTime.UtcNow;
			if (result > utcNow)
			{
				// use 1 second resolution
				result = new DateTime(utcNow.Ticks - (utcNow.Ticks % TimeSpan.TicksPerSecond), DateTimeKind.Utc);
			}

			return result;
		}

		#endregion

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

			// We support byte ranges
			response.AppendHeader("Accept-Ranges", "bytes");

			cache.SetCacheability(System.Web.HttpCacheability.Public);
			cache.VaryByHeaders["Accept-Encoding"] = true;
			cache.SetExpires(Expiration);
			cache.SetMaxAge(MaxAge);
			cache.SetRevalidation(HttpCacheRevalidation.AllCaches);

			if (setLastModifiedDate && LastModifiedUtc.HasValue)
			{
				cache.SetLastModified(FixLastModifiedDate(LastModifiedUtc.Value));
			}
		}
	}
}
