using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.Routing;
using SmartStore.Collections;

namespace SmartStore.Services.Media
{
	public class ProcessImageQuery : QueryString
	{
		public ProcessImageQuery()
			: this(null, new NameValueCollection())
		{
		}

		public ProcessImageQuery(object source)
			: this(source, new NameValueCollection())
		{
		}

		public ProcessImageQuery(object source, NameValueCollection query)
			: base(query)
		{
			Guard.NotNull(query, nameof(query));

			Source = source;
			DisposeSource = true;
		}

		public ProcessImageQuery(ProcessImageQuery request)
			: base(request)
		{
			Guard.NotNull(request, nameof(request));

			Source = request.Source;
			DisposeSource = request.DisposeSource;
		}

		/// <summary>
		/// The source image's physical path, app-relative virtual path, or a Stream, byte array, 
		/// Bitmap, VirtualFile, HttpPostedFile, or HttpPostedFileBase instance.
		/// </summary>
		public object Source { get; set; }

		public string FileName { get; set; }

		/// <summary>
		/// Whether to dispose the source stream after resizing completes
		/// </summary>
		public bool DisposeSource { get; set; }

		/// <summary>
		/// Whether to execute an applicable post processor which
		/// can reduce the resulting file size drastically, but also
		/// can slow down processing time.
		/// </summary>
		public bool ExecutePostProcessor { get; set; }

		public int? MaxWidth
		{
			get { return Get<int?>("w"); }
			set { Set("w", value); ScaleMode = value == null ? (string)null : "max"; }
		}

		public int? MaxHeight
		{
			get { return Get<int?>("h"); }
			set { Set("h", value); ScaleMode = value == null ? (string)null : "max"; }
		}

		public int? Quality
		{
			get { return Get<int?>("q"); }
			set { Set("q", value); }
		}

		// TODO: (mc) make Enum
		public string ScaleMode
		{
			get { return Get<string>("mode"); }
			set { Set("mode", value); }
		}

		/// <summary>
		/// Gets or sets the output file format to use. "png", "jpg", and "gif" are valid values.
		/// Returns null if unspecified. When format is not specified, the original format of the source image is used (unless it is not a web safe format - jpeg is the fallback in that scenario).
		/// </summary>
		public string Format {
			get { return Get<string>("fmt"); }
			set { Set("fmt", value); }
		}


		private T Get<T>(string name)
		{
			return base[name].Convert<T>();
		}

		private void Set<T>(string name, T val)
		{
			if (val == null)
				base.Remove(name);
			else
				base.Add(name, val.Convert<string>(), true);
		}


		public bool NeedsProcessing()
		{
			return base.Count > 0;
		}

		public string CreateHash()
		{
			var sb = new StringBuilder();

			foreach (var key in base.AllKeys)
			{
				if (key != "fmt")
				{
					sb.Append("-");
					sb.Append(key);
					sb.Append(base[key]);
				}
			}

			return sb.ToString();
		}
	}
}
