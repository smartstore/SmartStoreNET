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

			if (options.Margins != null)
			{
				converter.Margins.Bottom = options.Margins.Bottom;
				converter.Margins.Left = options.Margins.Left;
				converter.Margins.Right = options.Margins.Right;
				converter.Margins.Top = options.Margins.Top;
			}

			converter.CustomWkHtmlArgs = options.CustomFlags;
			converter.CustomWkHtmlPageArgs = CreateCustomPageFlags(options);

			if (options.PageHeader != null)
			{
				ProcessRepeatableSection("header", options.PageHeader, converter);
			}
			if (options.PageFooter != null)
			{
				ProcessRepeatableSection("footer", options.PageFooter, converter);
			}

			return converter;
		}

		private void ProcessRepeatableSection(string flag, IRepeatablePdfSection section, HtmlToPdfConverter converter)
		{
			bool isUrl;
			var result = section.Process(out isUrl);

			if (result.IsEmpty())
				return;

			if (isUrl)
			{
				converter.CustomWkHtmlPageArgs += " --{0}-html \"{1}\"".FormatInvariant(flag, result);
			}
			else
			{
				// TODO: (MC) This is a very weak mechanism to determine if html is partial. Find a better way.
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
