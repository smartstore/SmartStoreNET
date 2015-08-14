using System;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange
{
	public partial interface IExportProvider : IProvider, IUserEditable
	{
		/// <summary>
		/// Th exported entity type
		/// </summary>
		ExportEntityType EntityType { get; }

		/// <summary>
		/// Extension of the export files (without dot)
		/// </summary>
		string FileExtension { get; }

		/// <summary>
		/// Get configuration information
		/// </summary>
		/// <param name="partialViewName">The partial view name for the configuration</param>
		/// <param name="modelType">Type of the view model</param>
		/// <returns>Whether configuration is required</returns>
		bool RequiresConfiguration(out string partialViewName, out Type modelType);

		/// <summary>
		/// Export data to a file
		/// </summary>
		/// <param name="context">Export execution context</param>
		void Execute(IExportExecuteContext context);
	}
}
