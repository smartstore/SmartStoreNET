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
	}
}
