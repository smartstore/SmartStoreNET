using System.Linq;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange
{
	public static class ExportExtensions
	{
		public static bool IsValid(this Provider<IExportProvider> provider)
		{
			return (
				provider != null &&
				provider.Value.FileExtension.HasValue()
			);
		}

		public static bool Supports(this Provider<IExportProvider> provider, ExportProjectionSupport type)
		{
			if (provider != null)
				return provider.Metadata.ExportProjectionSupport.Contains(type);
			return false;
		}
	}
}
