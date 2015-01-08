using System;

namespace SmartStore.Services.Pdf
{
	public class PdfPageMargins
	{
		public PdfPageMargins()
		{
			Left = 20;
		}
		
		public float? Bottom { get; set; }
		public float? Left { get; set; }
		public float? Right { get; set; }
		public float? Top { get; set; }
	}
}
