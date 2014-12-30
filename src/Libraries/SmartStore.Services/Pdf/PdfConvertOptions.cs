using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace SmartStore.Services.Pdf
{
	public class PdfConvertOptions
	{
		public PdfConvertOptions()
		{
			this.Zoom = 1f;
			this.FormsAuthenticationCookieName = FormsAuthentication.FormsCookieName;
			this.Post = new Dictionary<string, string>();
			this.Cookies = new Dictionary<string, string>();
			this.UsePrintMediaType = true;
			this.Margins = new PdfPageMargins();
			this.HeaderSpacing = 5;
			this.FooterSpacing = 5;
			this.Orientation = PdfPagePrientation.Default;
			this.Size = PdfPageSize.Default;
		}
		
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
		public PdfPageMargins Margins { get; set; }

		/// <summary>
		/// Get or set PDF page orientation
		/// </summary>
		public PdfPagePrientation Orientation { get; set; }

		/// <summary>
		/// Get or set custom page footer
		/// </summary>
		public IRepeatablePdfSection PageFooter { get; set; }

		/// <summary>
		/// Get or set custom page header 
		/// </summary>
		public IRepeatablePdfSection PageHeader { get; set; }

		/// <summary>
		/// Get or set PDF page width (in mm)
		/// </summary>
		public float? PageWidth { get; set; }

		/// <summary>
		/// Get or set PDF page height (in mm) 
		/// </summary>
		public float? PageHeight { get; set; }

		/// <summary>
		/// Get or set repeatable header spacing (in mm)
		/// </summary>
		public float? HeaderSpacing { get; set; }

		/// <summary>
		/// Get or set repeatable footer spacing (in mm)
		/// </summary>
		public float? FooterSpacing { get; set; }

		/// <summary>
		/// Get or set PDF page orientation 
		/// </summary>
		public PdfPageSize Size { get; set; }

		/// <summary>
		/// Get or set zoom factor 
		/// </summary>
		public float Zoom { get; set; }

		/// <summary>
		/// Indicates whether the page background should be disabled.
		/// </summary>
		public bool BackgroundDisabled { get; set; }

		/// <summary>
		/// Custom name of authentication cookie used by forms authentication.
		/// </summary>
		public string FormsAuthenticationCookieName { get; set; }

		/// <summary>
		/// HTTP Authentication username.
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		/// HTTP Authentication password.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Sets cookies.
		/// </summary>
		public Dictionary<string, string> Post { get; set; }

		/// <summary>
		/// Sets post values.
		/// </summary>
		public Dictionary<string, string> Cookies { get; set; }

		/// <summary>
		/// Custom global pdf tool options
		/// </summary>
		public string CustomFlags { get; set; }

		/// <summary>
		/// Custom pdf tool page options
		/// </summary>
		public string CustomPageFlags { get; set; }

		/// <summary>
		/// Use print media-type instead of screen
		/// </summary>
		public bool UsePrintMediaType { get; set; }

		/// <summary>
		/// Specifies a user style sheet to load with every page
		/// </summary>
		public string UserStylesheetUrl { get; set; }
	}
}
