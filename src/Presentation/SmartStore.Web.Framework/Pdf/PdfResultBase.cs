using System.IO;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SmartStore.Services.Pdf;

namespace SmartStore.Web.Framework.Pdf
{
	public abstract class PdfResultBase : ActionResult
	{
		private const string ContentType = "application/pdf";

		protected PdfResultBase(IPdfConverter converter, PdfConvertOptions options)
		{
			Guard.ArgumentNotNull(() => converter);
			
			this.Converter = converter;
			this.Options = options ?? new PdfConvertOptions();
		}

		protected IPdfConverter Converter { get; set; }

		protected PdfConvertOptions Options { get; set; }

		/// <summary>
		/// The name of the generated PDF file.
		/// </summary>
		public string FileName { get; set; }

		protected abstract string GetUrl(ControllerContext context);

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

		protected virtual byte[] CallConverter(ControllerContext context)
		{
			var url = this.GetUrl(context);
			var buffer = Converter.ConvertFile(url, Options, null);
			return buffer;
		}

		public override void ExecuteResult(ControllerContext context)
		{
			var buffer = CallConverter(context);
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
