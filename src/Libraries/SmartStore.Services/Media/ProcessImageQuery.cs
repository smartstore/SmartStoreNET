using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using SmartStore.Collections;
using System.Drawing;
using ImageProcessor.Imaging.Formats;

namespace SmartStore.Services.Media
{
	public class ProcessImageQuery : QueryString
	{
		private readonly static HashSet<string> _supportedTokens = new HashSet<string> { "w", "h", "q", "m", "size" };

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
			: base(SanitizeCollection(query))
		{
			Guard.NotNull(query, nameof(query));

			Source = source;
			DisposeSource = true;
			Notify = true;
		}

		public ProcessImageQuery(ProcessImageQuery query)
			: base(SanitizeCollection(query))
		{
			Guard.NotNull(query, nameof(query));

			Source = query.Source;
			Format = query.Format;
			DisposeSource = query.DisposeSource;
		}

		private static NameValueCollection SanitizeCollection(NameValueCollection query)
		{
			// We just need the supported flags
			var sanitizable = query.AllKeys.Any(x => !_supportedTokens.Contains(x));
			if (sanitizable)
			{
				var copy = new NameValueCollection(query);
				foreach (var key in copy.AllKeys)
				{
					if (!_supportedTokens.Contains(key))
					{
						copy.Remove(key);
					}
				}

				return copy;
			}

			return query;
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

		/// <summary>
		/// max (default) | boxpad | crop | min | pad | stretch
		/// </summary>
		public string ScaleMode
		{
			get { return Get<string>("m"); }
			set { Set("m", value); }
		}

		/// <summary>
		/// center (default) | top | bottom | left | right | top-left | top-right | bottom-left | bottom-right
		/// </summary>
		public string AnchorPosition
		{
			get { return Get<string>("pos"); }
			set { Set("pos", value); }
		}

		public string BackgroundColor
		{
			get { return Get<string>("bg"); }
			set { Set("bg", value); }
		}

		/// <summary>
		/// Gets or sets the output file format either as a string ("png", "jpg", "gif" and "svg"),
		/// or as a format object instance.
		/// When format is not specified, the original format of the source image is used (unless it is not a web safe format - jpeg is the fallback in that scenario).
		/// </summary>
		public object Format { get; set; }

		public bool IsValidationMode { get; set; }

		public bool Notify { get; set; }

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


		public bool NeedsProcessing(bool ignoreQualityFlag = false)
		{
			if (base.Count == 0)
				return false;

            if (object.Equals(Format, "svg"))
            {
                // SVG cannot be processed.
                return false;
            }

			if (ignoreQualityFlag && base.Count == 1 && base["q"] != null)
			{
				// Return false if ignoreQualityFlag is true and "q" is the only flag.
				return false;
			}

			if (base.Count == 1 && Quality == 100)
			{
				// If "q" is the only flag and its value is 100, we don't need to process
				return false;
			}

			return true;
		}

		public string CreateHash()
		{
			var hash = string.Empty;

			foreach (var key in base.AllKeys)
			{
				if (key == "m" && base["m"] == "max")
					continue; // Mode 'max' is default and can be omitted

                hash += "-" + key + base[key];

            }

			return hash;
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
