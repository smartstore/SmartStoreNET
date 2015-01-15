using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using NReco.PdfGenerator;
using SmartStore.Core.Logging;

namespace SmartStore.Services.Pdf
{
	public class WkHtmlToPdfConverter : IPdfConverter
	{
		private readonly HttpContextBase _httpContext;

		public WkHtmlToPdfConverter(HttpContextBase httpContext)
		{
			this._httpContext = httpContext;
			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		public byte[] ConvertHtml(string html, PdfConvertOptions options)
		{
			Guard.ArgumentNotEmpty(() => html);
			Guard.ArgumentNotNull(() => options);

			try
			{
				var converter = CreateWkConverter(options);
				var buffer = converter.GeneratePdf(html);
				return buffer;
			}
			catch (Exception ex)
			{
				Logger.Error("Html to Pdf conversion error", ex);
				throw;
			}
		}

		public byte[] ConvertFile(string htmlFilePath, PdfConvertOptions options, string coverHtml = null)
		{
			Guard.ArgumentNotEmpty(() => htmlFilePath);
			Guard.ArgumentNotNull(() => options);

			try
			{
				var converter = CreateWkConverter(options);
				var buffer = converter.GeneratePdfFromFile(htmlFilePath, coverHtml);
				return buffer;
			}
			catch (Exception ex)
			{
				Logger.Error("Html to Pdf conversion error", ex);
				throw;
			}
		}

		public byte[] Convert(PdfConvertSettings settings)
		{
			Guard.ArgumentNotNull(() => settings);
			if (settings.Page == null)
			{
				throw Error.InvalidOperation("The 'Page' property of the 'settings' argument cannot be null.");
			}

			try
			{
				var converter = CreateWkConverter2(settings);

				var input = settings.Page.Process("page");

				if (settings.Page.Kind == PdfContentKind.Url)
				{
					return converter.GeneratePdfFromFile(input, null);
				}
				else
				{
					return converter.GeneratePdf(input, null);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Html to Pdf conversion error", ex);
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

		internal HtmlToPdfConverter CreateWkConverter2(PdfConvertSettings settings)
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
			};

			//// apply our special "margin fix" to header & footer
			//ApplyHeaderFooterMarginFix(settings);

			if (settings.Margins != null)
			{
				converter.Margins.Bottom = settings.Margins.Bottom;
				converter.Margins.Left = settings.Margins.Left;
				converter.Margins.Right = settings.Margins.Right;
				converter.Margins.Top = settings.Margins.Top;
			}

			var sb = new StringBuilder(settings.CustomFlags);

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
			if (settings.TocOptions != null)
			{
				settings.CoverOptions.Process("cover", sb);
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
					sb.AppendFormat(" --header-html \"{0}\" ", path);
					settings.Header.WriteArguments("header", sb);
				}
			}
			if (settings.HeaderOptions != null)
			{
				settings.HeaderOptions.Process("header", sb);
			}

			// Footer content & options
			if (settings.Footer != null)
			{
				var path = settings.Footer.Process("footer");
				if (path.HasValue())
				{
					sb.AppendFormat(" --footer-html \"{0}\" ", path);
					settings.Footer.WriteArguments("footer", sb);
				}
			}
			if (settings.FooterOptions != null)
			{
				settings.FooterOptions.Process("footer", sb);
			}

			// apply settings
			converter.CustomWkHtmlPageArgs = sb.ToString().Trim().NullEmpty();

			return converter;
		}

		internal HtmlToPdfConverter CreateWkConverter(PdfConvertOptions options)
		{
			var converter = new HtmlToPdfConverter
			{
				Grayscale = options.Grayscale,
				LowQuality = options.LowQuality,
				Orientation = (PageOrientation)(int)options.Orientation,
				PageHeight = options.PageHeight,
				PageWidth = options.PageWidth,
				Size = (PageSize)(int)options.Size,
				Zoom = options.Zoom
			};

			converter.CustomWkHtmlArgs = options.CustomFlags;
			converter.CustomWkHtmlPageArgs = CreateCustomPageFlags(options);

			if (options.PageHeader != null)
			{
				ProcessHeaderFooter("header", options.PageHeader, converter);
			}
			if (options.PageFooter != null)
			{
				ProcessHeaderFooter("footer", options.PageFooter, converter);
			}

			// apply our special "margin fix" to header & footer
			ApplyHeaderFooterMarginFix(options);

			if (options.Margins != null)
			{
				converter.Margins.Bottom = options.Margins.Bottom;
				converter.Margins.Left = options.Margins.Left;
				converter.Margins.Right = options.Margins.Right;
				converter.Margins.Top = options.Margins.Top;
			}

			return converter;
		}

		//private void ApplyHeaderFooterMarginFix2(PdfConvertSettings settings)
		//{
		//	var header = settings.Header;
		//	var footer = settings.Footer;
		//	var t = settings.Margins.Top;
		//	var b = settings.Margins.Bottom;

		//	if (header == null || footer == null)
		//		return;

		//	float? newT = null;
		//	float? newB = null;

		//	// When both are simple: set both to 15.
		//	// When only one is simple: set simple to 15 and the other (html) to 35
		//	// When both are html: do nothing

		//	if (header.Kind == PdfHeaderFooterKind.Args && header.Kind == footer.Kind)
		//	{
		//		// both are simple
		//		newT = 15;
		//		newB = 15;
		//	}
		//	else
		//	{
		//		if (header.Kind != PdfHeaderFooterKind.Args && footer.Kind == PdfHeaderFooterKind.Args)
		//		{
		//			// header is html, footer is simple
		//			newT = 35;
		//			newB = 15;
		//		}
		//		else if (header.Kind == PdfHeaderFooterKind.Args && footer.Kind != PdfHeaderFooterKind.Args)
		//		{
		//			// header is simple, footer is html
		//			newT = 15;
		//			newB = 35;
		//		}
		//	}

		//	if (!t.HasValue && newT.HasValue)
		//		settings.Margins.Top = newT;

		//	if (!b.HasValue && newB.HasValue)
		//		settings.Margins.Bottom = newB;
		//}

		private void ApplyHeaderFooterMarginFix(PdfConvertOptions o)
		{
			var header = o.PageHeader;
			var footer = o.PageFooter;
			var t = o.Margins.Top;
			var b = o.Margins.Bottom;

			if (header == null || footer == null)
				return;

			float? newT = null;
			float? newB = null;

			// When both are simple: set both to 15.
			// When only one is simple: set simple to 15 and the other (html) to 35
			// When both are html: do nothing

			if (header.Kind == PdfHeaderFooterKind.Args && header.Kind == footer.Kind)
			{
				// both are simple
				newT = 15;
				newB = 15;
			}
			else
			{
				if (header.Kind != PdfHeaderFooterKind.Args && footer.Kind == PdfHeaderFooterKind.Args)
				{
					// header is html, footer is simple
					newT = 35;
					newB = 15;
				}
				else if (header.Kind == PdfHeaderFooterKind.Args && footer.Kind != PdfHeaderFooterKind.Args)
				{
					// header is simple, footer is html
					newT = 15;
					newB = 35;
				}
			}

			if (!t.HasValue && newT.HasValue)
				o.Margins.Top = newT;

			if (!b.HasValue && newB.HasValue)
				o.Margins.Bottom = newB;
		}

		private void ProcessHeaderFooter(string flag, IPdfHeaderFooter section, HtmlToPdfConverter converter)
		{
			var result = section.Process(flag);
			var kind = section.Kind;

			if (result.IsEmpty())
				return;

			if (kind == PdfHeaderFooterKind.Url)
			{
				converter.CustomWkHtmlPageArgs += " --{0}-html \"{1}\"".FormatInvariant(flag, result);
			}
			else if (kind == PdfHeaderFooterKind.Args)
			{
				converter.CustomWkHtmlPageArgs += " {0}".FormatInvariant(result);
			}
			else if (kind == PdfHeaderFooterKind.Html)
			{
				// TODO: (mc) This is a very weak mechanism to determine if html is partial. Find a better way!
				bool isPartial = !result.Trim().StartsWith("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
				if (isPartial)
				{
					// NReco.PdfConverter is very well capable of handling partial html, so delegate it.
					if (flag == "header")
					{
						converter.PageHeaderHtml = result;
					}
					else if (flag == "footer")
					{
						converter.PageFooterHtml = result;
					}
				}
				else
				{
					// TODO: (MC) Implement non-partial handling later (must create temp file and so on)
				}
			}
		}

		private string CreateCustomPageFlags(PdfConvertOptions options)
		{
			var sb = new StringBuilder(options.CustomPageFlags);

			if (options.UserStylesheetUrl.HasValue())
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, " --user-style-sheet \"{0}\"", options.UserStylesheetUrl);
			}

			if (options.UsePrintMediaType)
			{
				sb.Append(" --print-media-type");
			}

			if (options.BackgroundDisabled)
			{
				sb.Append(" --no-background");
			}

			if (options.UserName.HasValue())
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, " --username {0}", options.UserName);
			}

			if (options.Password.HasValue())
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, " --password {0}", options.Password);
			}

			if (options.HeaderSpacing.HasValue && options.PageHeader != null)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, " --header-spacing {0}", options.HeaderSpacing.Value);
			}

			if (options.FooterSpacing.HasValue && options.PageFooter != null)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, " --footer-spacing {0}", options.FooterSpacing.Value);
			}

			if (options.ShowHeaderLine)
			{
				sb.Append(" --header-line");
			}

			if (options.ShowFooterLine)
			{
				sb.Append(" --footer-line");
			}

			if (options.Post != null && options.Post.Count > 0)
			{
				CreateRepeatableFlags("--post", options.Post, sb);
			}

			if (options.Cookies != null && options.Cookies.Count > 0)
			{
				CreateRepeatableFlags("--cookie", options.Cookies, sb);
			}

			// Send FormsAuthentication Cookie
			if (options.FormsAuthenticationCookieName.HasValue() && _httpContext != null && _httpContext.Request != null && _httpContext.Request.Cookies != null)
			{
				if (options.Cookies == null || !options.Cookies.ContainsKey(options.FormsAuthenticationCookieName))
				{
					HttpCookie authenticationCookie = null;
					if (_httpContext.Request.Cookies.AllKeys.Contains(options.FormsAuthenticationCookieName))
					{
						authenticationCookie = _httpContext.Request.Cookies[options.FormsAuthenticationCookieName];
					}
					if (authenticationCookie != null)
					{
						var authCookieValue = authenticationCookie.Value;
						sb.AppendFormat(CultureInfo.InvariantCulture, " {0} {1} {2}", "--cookie", options.FormsAuthenticationCookieName, authCookieValue);
					}
				}
			}

			return sb.ToString().Trim().NullEmpty();
		}

		private void CreateRepeatableFlags(string flagName, IDictionary<string, string> dict, StringBuilder sb)
		{
			foreach (var kvp in dict)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, " {0} {1} {2}", flagName, kvp.Key, kvp.Value.EmptyNull());
			}
		}

	}
}
