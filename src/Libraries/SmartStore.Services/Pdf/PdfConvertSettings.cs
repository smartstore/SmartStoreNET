using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace SmartStore.Services.Pdf
{
	public class PdfConvertSettings
	{
		public PdfConvertSettings()
		{
			this.Margins = new PdfPageMargins();
			this.Orientation = PdfPagePrientation.Default;
			this.Size = PdfPageSize.Default;
			this.PageOptions = new PdfPageOptions();
			this.HeaderOptions = new PdfHeaderFooterOptions();
			this.FooterOptions = new PdfHeaderFooterOptions();
			this.CoverOptions = new PdfPageOptions();
			this.TocOptions = new PdfTocOptions();
		}

		/// <summary>
		/// The title of the generated pdf file (The title of the first document is used if not specified)
		/// </summary>
		public string Title { get; set; }

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
		public PdfPageSize Size { get; set; }

		/// <summary>
		/// Custom global pdf tool options
		/// </summary>
		public string CustomFlags { get; set; }



		/// <summary>
		/// Cover content
		/// </summary>
		public IPdfContent Cover { get; set; }

		/// <summary>
		/// Cover content options
		/// </summary>
		public PdfPageOptions CoverOptions { get; set; }

		/// <summary>
		/// Toc (table of contents) options
		/// </summary>
		public PdfTocOptions TocOptions { get; set; }

		/// <summary>
		/// Page content (required)
		/// </summary>
		public IPdfContent Page { get; set; }

		/// <summary>
		/// Page content options
		/// </summary>
		public PdfPageOptions PageOptions { get; set; }

		/// <summary>
		/// Footer content
		/// </summary>
		public IPdfContent Footer { get; set; }

		/// <summary>
		/// Footer content options
		/// </summary>
		public PdfHeaderFooterOptions FooterOptions { get; set; }

		/// <summary>
		/// Header content
		/// </summary>
		public IPdfContent Header { get; set; }

		/// <summary>
		/// Header content options
		/// </summary>
		public PdfHeaderFooterOptions HeaderOptions { get; set; }
	}
}
