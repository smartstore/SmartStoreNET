using System;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange
{
	public partial interface IExportProvider : IProvider, IUserEditable
	{
		ExportEntityType EntityType { get; }

		string FileType { get; }

		/// <summary>
		/// Get configuration information
		/// </summary>
		/// <param name="partialViewName">The partial view name for the configuration</param>
		/// <param name="modelType">Type of the view model</param>
		/// <returns>Whether configuration is required</returns>
		bool RequiresConfiguration(out string partialViewName, out Type modelType);

		/// <summary>
		/// A record needs to be exported to a file
		/// </summary>
		/// <param name="context">Export execution context</param>
		void Execute(ExportExecuteContext context);
	}
}
