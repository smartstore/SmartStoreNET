using System;
using System.Globalization;
using System.Text;

namespace SmartStore.Services.Pdf
{
	public class PdfPageOptions : IPdfOptions
	{

		public PdfPageOptions()
		{
			this.Zoom = 1f;
			this.UsePrintMediaType = true;
		}

		/// <summary>
		/// Get or set zoom factor 
		/// </summary>
		public float Zoom { get; set; }

		/// <summary>
		/// Indicates whether the page background should be disabled.
		/// </summary>
		public bool BackgroundDisabled { get; set; }

		/// <summary>
		/// Use print media-type instead of screen
		/// </summary>
		public bool UsePrintMediaType { get; set; }

		/// <summary>
		/// Specifies a user style sheet to load with every page
		/// </summary>
		public string UserStylesheetUrl { get; set; }

		/// <summary>
		/// Custom page pdf tool options
		/// </summary>
		public string CustomFlags { get; set; }



		public virtual void Process(string flag, StringBuilder builder)
		{
			if (UserStylesheetUrl.HasValue())
			{
				builder.AppendFormat(CultureInfo.InvariantCulture, " --user-style-sheet \"{0}\"", UserStylesheetUrl);
			}

			if (UsePrintMediaType)
			{
				builder.Append(" --print-media-type");
			}

			if (BackgroundDisabled)
			{
				builder.Append(" --no-background");
			}

			if (Zoom != 1 /*&& flag.IsCaseInsensitiveEqual("page")*/)
			{
				builder.AppendFormat(" --zoom {0}", Zoom);
			}

			if (CustomFlags.HasValue())
			{
				builder.AppendFormat(" {0}", CustomFlags);
			}
		}

	}
}
