using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using NReco.PdfGenerator;

namespace SmartStore.Web.Framework.Pdf
{
	public abstract class PdfResultBase : ActionResult
	{
		private const string ContentType = "application/pdf";

		public PdfResultBase()
		{
			this.Zoom = 1f;
		}

		#region Properties

		/// <summary>
		/// The name of the generated PDF file.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// Get or set option to generate grayscale PDF 
		/// </summary>
		public bool Grayscale { get; set; }

		/// <summary>
		/// Get or set option to generate low quality PDF (shrink the result document space) 
		/// </summary>
		public bool LowQuality { get; set; }

		/// <summary>
		/// Get or set PDF page margins (in mm) 
		/// </summary>
		public string Margins { get; set; }

		/// <summary>
		/// Get or set PDF page orientation
		/// </summary>
		public string Orientation { get; set; }

		/// <summary>
		/// Get or set custom page footer HTML
		/// </summary>
		public string PageFooterHtml { get; set; }

		/// <summary>
		/// Get or set custom page header HTML 
		/// </summary>
		public string PageHeaderHtml { get; set; }

		/// <summary>
		/// Get or set PDF page width (in mm)
		/// </summary>
		public float? PageWidth { get; set; }

		/// <summary>
		/// Get or set PDF page height (in mm) 
		/// </summary>
		public float? PageHeight { get; set; }

		/// <summary>
		/// Get or set PDF page orientation 
		/// </summary>
		public string Size { get; set; }

		/// <summary>
		/// Custom WkHtmlToPdf global options 
		/// </summary>
		public string CustomWkHtmlArgs { get; set; }

		/// <summary>
		/// Custom WkHtmlToPdf page options 
		/// </summary>
		public string CustomWkHtmlPageArgs { get; set; }

		/// <summary>
		/// Get or set zoom factor 
		/// </summary>
		public float Zoom { get; set; }

		#endregion

		#region Methods

		protected abstract string GetUrl(ControllerContext context);

		protected HtmlToPdfConverter CreateConverter()
		{
			var converter = new HtmlToPdfConverter 
			{
				CustomWkHtmlArgs = this.CustomWkHtmlArgs,
				CustomWkHtmlPageArgs = this.CustomWkHtmlPageArgs,
				Grayscale = this.Grayscale,
				LowQuality = this.LowQuality,
				PageFooterHtml = this.PageFooterHtml,
				PageHeaderHtml = this.PageHeaderHtml,
				PageHeight = this.PageHeight,
				PageWidth = this.PageWidth,
				Zoom = this.Zoom
			};

			return converter;
		}

		protected HttpResponseBase PrepareResponse(HttpResponseBase response)
		{
			response.ContentType = ContentType;

			if (FileName.HasValue())
			{
				response.AddHeader("Content-Disposition", "attachment; filename=\"{0}\"".FormatCurrent(SanitizeFileName(FileName)));
			}

			response.AddHeader("Content-Type", ContentType);

			return response;
		}

		public byte[] BuildPdf(ControllerContext context)
		{
			Guard.ArgumentNotNull(() => context);

			var converter = CreateConverter();
			if (converter == null)
			{
				// TODO: ErrHandling
			}

			var buffer = CallConverter(context, converter);

			return buffer;
		}

		protected virtual byte[] CallConverter(ControllerContext context, HtmlToPdfConverter converter)
		{
			var url = this.GetUrl(context);
			var buffer = converter.GeneratePdfFromFile(url, null);
			return buffer;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			var buffer = BuildPdf(context);
			var response = PrepareResponse(context.HttpContext.Response);
			response.OutputStream.Write(buffer, 0, buffer.Length);
		}

		private static string SanitizeFileName(string name)
		{
			string invalidChars = Regex.Escape(new string(Path.GetInvalidPathChars()) + new string(Path.GetInvalidFileNameChars()));
			string invalidCharsPattern = string.Format(@"[{0}]+", invalidChars);

			string result = Regex.Replace(name, invalidCharsPattern, "-");
			return result;
		}

		#endregion
	}
}
