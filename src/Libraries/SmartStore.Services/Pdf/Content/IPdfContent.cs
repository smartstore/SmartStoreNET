using System;
using System.Collections.Generic;
using System.Text;

namespace SmartStore.Services.Pdf
{
	public interface IPdfContent
	{
		PdfContentKind Kind { get; }

		string Process(string flag);

		void WriteArguments(string flag, StringBuilder builder);

		void Teardown();
	}

	public enum PdfContentKind
	{
		Html,
		Url
	}
}
