using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Services.Pdf
{
	public interface IPdfConverter
	{
		/// <summary>
		/// Converts html content to PDF
		/// </summary>
		/// <param name="html">The html content</param>
		/// <param name="options">The options to be used for the conversion process</param>
		/// <returns>The PDF binary data</returns>
		byte[] ConvertHtml(string html, PdfConvertOptions options);

		/// <summary>
		/// Converts any html file to PDF
		/// </summary>
		/// <param name="htmlFilePath">path to HTML file or absolute URL</param>
		/// <param name="options">The options to be used for the conversion process</param>
		/// <param name="coverHtml">First page HTML</param>
		/// <returns>The PDF binary data</returns>
		byte[] ConvertFile(string htmlFilePath, PdfConvertOptions options, string coverHtml = null);


		/// <summary>
		/// Converts html content to PDF
		/// </summary>
		/// <param name="settings">The settings to be used for the conversion process</param>
		/// <returns>The PDF binary data</returns>
		byte[] Convert(PdfConvertSettings settings);
	}
}
