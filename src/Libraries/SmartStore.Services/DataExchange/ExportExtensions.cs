using System.Linq;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange
{
	public static class ExportExtensions
	{
		/// <summary>
		/// Returns a value indicating whether the export provider is valid
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <returns><c>true</c> provider is valid, <c>false</c> provider is invalid.</returns>
		public static bool IsValid(this Provider<IExportProvider> provider)
		{
			return (
				provider != null &&
				provider.Value.FileExtension.HasValue()
			);
		}

		/// <summary>
		/// Returns a value indicating whether the export provider supports a projection type
		/// </summary>
		/// <param name="provider">Export provider</param>
		/// <param name="type">The type to check</param>
		/// <returns><c>true</c> provider supports type, <c>false</c> provider does not support type.</returns>
		public static bool Supports(this Provider<IExportProvider> provider, ExportProjectionSupport type)
		{
			if (provider != null)
				return provider.Metadata.ExportProjectionSupport.Contains(type);
			return false;
		}
	}
}
