using System;

namespace SmartStore.Services.Pdf
{
	
	public interface IPdfHeaderFooter
	{
		PdfHeaderFooterKind Kind { get; }
		string Process(string flag);
	}

	public enum PdfHeaderFooterKind
	{
		Html,
		Url,
		Args
	}

}
