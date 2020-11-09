using System;
using System.Globalization;
using System.Text;
using NReco.PdfGenerator;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.Pdf
{
    public class WkHtmlToPdfConverter : IPdfConverter
    {
        public WkHtmlToPdfConverter()
        {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }


        public byte[] Convert(PdfConvertSettings settings)
        {
            Guard.NotNull(settings, nameof(settings));
            if (settings.Page == null)
            {
                throw Error.InvalidOperation("The 'Page' property of the 'settings' argument cannot be null.");
            }

            try
            {
                var converter = CreateWkConverter(settings);

                var input = settings.Page.Process("page");

                if (settings.Page.Kind == PdfContentKind.Url)
                {
                    Logger.DebugFormat("Generating PDF from URL '{0}'. CustomWkHtmlPageArgs: {1}", input, converter.CustomWkHtmlPageArgs);
                    return converter.GeneratePdfFromFile(input, null);
                }
                else
                {
                    Logger.DebugFormat("Generating PDF from HTML. CustomWkHtmlPageArgs: {0}", converter.CustomWkHtmlPageArgs);
                    return converter.GeneratePdf(input, null);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Html to Pdf conversion error");
                throw;
            }
            finally
            {
                TeardownContent(settings.Cover);
                TeardownContent(settings.Footer);
                TeardownContent(settings.Header);
                TeardownContent(settings.Page);
            }
        }

        private void TeardownContent(IPdfContent content)
        {
            if (content != null)
                content.Teardown();
        }

        internal HtmlToPdfConverter CreateWkConverter(PdfConvertSettings settings)
        {
            // Global options
            var converter = new HtmlToPdfConverter
            {
                Grayscale = settings.Grayscale,
                LowQuality = settings.LowQuality,
                Orientation = (PageOrientation)(int)settings.Orientation,
                PageHeight = settings.PageHeight,
                PageWidth = settings.PageWidth,
                Size = (PageSize)(int)settings.Size,
                PdfToolPath = FileSystemHelper.TempDir("wkhtmltopdf"),
                TempFilesPath = FileSystemHelper.TempDirTenant()
            };

            if (settings.Margins != null)
            {
                converter.Margins.Bottom = settings.Margins.Bottom;
                converter.Margins.Left = settings.Margins.Left;
                converter.Margins.Right = settings.Margins.Right;
                converter.Margins.Top = settings.Margins.Top;
            }

            var sb = new StringBuilder(settings.CustomFlags);

            // doc title
            if (settings.Title.HasValue())
            {
                sb.AppendFormat(CultureInfo.CurrentCulture, " --title \"{0}\"", settings.Title);
            }

            // Cover content & options
            if (settings.Cover != null)
            {
                var path = settings.Cover.Process("cover");
                if (path.HasValue())
                {
                    sb.AppendFormat(" cover \"{0}\" ", path);
                    settings.Cover.WriteArguments("cover", sb);
                    if (settings.CoverOptions != null)
                    {
                        settings.CoverOptions.Process("cover", sb);
                    }
                }
            }

            // Toc options
            if (settings.TocOptions != null && settings.TocOptions.Enabled)
            {
                settings.TocOptions.Process("toc", sb);
            }

            // apply cover & toc
            converter.CustomWkHtmlArgs = sb.ToString().Trim().NullEmpty();
            sb.Clear();

            // Page options
            if (settings.PageOptions != null)
            {
                settings.PageOptions.Process("page", sb);
            }

            // Header content & options
            if (settings.Header != null)
            {
                var path = settings.Header.Process("header");
                if (path.HasValue())
                {
                    sb.AppendFormat(" --header-html \"{0}\"", path);
                    settings.Header.WriteArguments("header", sb);
                }
            }
            if (settings.HeaderOptions != null && (settings.Header != null || settings.HeaderOptions.HasText))
            {
                settings.HeaderOptions.Process("header", sb);
            }

            // Footer content & options
            if (settings.Footer != null)
            {
                var path = settings.Footer.Process("footer");
                if (path.HasValue())
                {
                    sb.AppendFormat(" --footer-html \"{0}\"", path);
                    settings.Footer.WriteArguments("footer", sb);
                }
            }
            if (settings.FooterOptions != null && (settings.Footer != null || settings.FooterOptions.HasText))
            {
                settings.FooterOptions.Process("footer", sb);
            }

            // apply settings
            converter.CustomWkHtmlPageArgs = sb.ToString().Trim().NullEmpty();

            return converter;
        }

    }
}
