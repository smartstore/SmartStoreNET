using System;
using System.IO;
using SmartStore.Core.Domain;
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

		// TODO: a more complex result type is required. e.g. IEnumerable<ExportSegment>....
		void Execute(ExportExecuteContext context);
	}
}
