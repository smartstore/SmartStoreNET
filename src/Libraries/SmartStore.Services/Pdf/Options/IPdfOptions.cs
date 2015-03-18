using System;
using System.Text;

namespace SmartStore.Services.Pdf
{
	public interface IPdfOptions
	{
		/// <summary>
		/// Processes the options by converting them to native arguments
		/// </summary>
		/// <param name="flag">The section flag</param>
		/// <param name="sb">The builder</param>
		/// <remarks>Possible flags are: page | header | footer | cover | toc</remarks>
		void Process(string flag, StringBuilder builder);
	}
}
