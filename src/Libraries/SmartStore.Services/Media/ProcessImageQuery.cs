using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using System.Web.Routing;
using SmartStore.Collections;
using System.Drawing;
using ImageProcessor.Imaging.Formats;

namespace SmartStore.Services.Media
{
	public class ProcessImageQuery : QueryString
	{
		public ProcessImageQuery()
			: this(null, new NameValueCollection())
		{
		}

		public ProcessImageQuery(byte[] source)
			: this(source, new NameValueCollection())
		{
		}

		public ProcessImageQuery(Stream source)
			: this(source, new NameValueCollection())
		{
		}

		public ProcessImageQuery(Image source)
			: this(source, new NameValueCollection())
		{
		}

		public ProcessImageQuery(string source)
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

		public ProcessImageQuery(ProcessImageQuery query)
			: base(query)
		{
			Guard.NotNull(query, nameof(query));

			Source = query.Source;
			Format = query.Format;
			DisposeSource = query.DisposeSource;
		}

		/// <summary>
		/// The source image's physical path, app-relative virtual path, or a Stream, byte array or Image instance.
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
			set { Set("w", value); }
		}

		public int? MaxHeight
		{
			get { return Get<int?>("h"); }
			set { Set("h", value); }
		}

		public int? Quality
		{
			get { return Get<int?>("q"); }
			set { Set("q", value); }
		}

		// TODO: (mc) make Enum
		public string ScaleMode
		{
			get { return Get<string>("m"); }
			set { Set("m", value); }
		}

		/// <summary>
		/// Gets or sets the output file format either as a string ("png", "jpg", and "gif"),
		/// or as a format object instance.
		/// When format is not specified, the original format of the source image is used (unless it is not a web safe format - jpeg is the fallback in that scenario).
		/// </summary>
		public object Format { get; set; }

		public bool IsValidationMode { get; set; }


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
				if (key == "m" && base["m"] == "max")
					continue; // Mode 'max' is default and can be omitted

				sb.Append("-");
				sb.Append(key);
				sb.Append(base[key]);
			}

			return sb.ToString();
		}

		public string GetResultExtension()
		{
			if (Format == null)
			{
				return null;
			}
			else if (Format is ISupportedImageFormat)
			{
				return ((ISupportedImageFormat)Format).DefaultExtension;
			}
			else if (Format is string)
			{
				return (string)Format;
			}

			return null;
		}
	}
}
