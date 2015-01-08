using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SmartStore.Services.Pdf;

namespace SmartStore.Web.Framework.Pdf
{

	public class SimplePdfHeaderFooter : IPdfHeaderFooter
	{

		public string FontName { get; set; }
		public float? FontSize { get; set; }

		public string TextLeft { get; set; }
		public string TextCenter { get; set; }
		public string TextRight { get; set; }

		public PdfHeaderFooterKind Kind
		{
			get { return PdfHeaderFooterKind.Args; }
		}

		public string Process(string flag)
		{
			var sb = new StringBuilder();

			if (FontName.HasValue())
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, " --{0}-font-name \"{1}\"", flag, FontName);
			}
			if (FontSize.HasValue)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, " --{0}-font-size {1}", flag, FontSize.Value);
			}

			if (TextLeft.HasValue())
			{
				sb.AppendFormat(CultureInfo.CurrentCulture, " --{0}-left \"{1}\"", flag, TextLeft);
			}
			if (TextCenter.HasValue())
			{
				sb.AppendFormat(CultureInfo.CurrentCulture, " --{0}-center \"{1}\"", flag, TextCenter);
			}
			if (TextRight.HasValue())
			{
				sb.AppendFormat(CultureInfo.CurrentCulture, " --{0}-right \"{1}\"", flag, TextRight);
			}

			return sb.ToString().Trim().NullEmpty();
		}
	}

}
