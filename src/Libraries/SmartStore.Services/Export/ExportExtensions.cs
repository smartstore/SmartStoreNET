using System.Linq;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Export
{
	public static class ExportExtensions
	{
		public static bool IsValid(this Provider<IExportProvider> provider)
		{
			return (
				provider != null && 
				provider.Value.SupportedFileTypes != null && provider.Value.SupportedFileTypes.Any()
			);
		}
	}
}
