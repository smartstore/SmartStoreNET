using System;

namespace SmartStore.Services.Pdf
{
	public interface IRepeatablePdfSection
	{
		string Process(out bool isUrl);
	}
}
