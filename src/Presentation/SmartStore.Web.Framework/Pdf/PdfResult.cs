using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SmartStore.Services.Pdf;

namespace SmartStore.Web.Framework.Pdf
{
	public class PdfResult : ActionResult
	{
		private const string ContentType = "application/pdf";

		public PdfResult(IPdfConverter converter, PdfConvertSettings settings)
		{
			Guard.NotNull(converter, nameof(converter));
			
			this.Converter = converter;
			this.Settings = settings ?? new PdfConvertSettings();
		}

		protected IPdfConverter Converter { get; set; }

		protected PdfConvertSettings Settings { get; set; }

		/// <summary>
		/// The name of the generated PDF file.
		/// </summary>
		public string FileName { get; set; }

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

		public override void ExecuteResult(ControllerContext context)
		{	
			var buffer = Converter.Convert(Settings);
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

	}
}
