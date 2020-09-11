using System.Web;
using System.Web.Mvc;
using SmartStore.Services.Pdf;
using SmartStore.Utilities;

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
                response.AddHeader("Content-Disposition", "attachment; filename=\"{0}\"".FormatCurrent(PathHelper.SanitizeFileName(FileName)));
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
    }
}
