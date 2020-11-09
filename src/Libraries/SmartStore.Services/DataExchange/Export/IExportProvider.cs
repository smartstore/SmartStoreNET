using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange.Export
{
    public partial interface IExportProvider : IProvider, IUserEditable
    {
        /// <summary>
        /// The exported entity type
        /// </summary>
        ExportEntityType EntityType { get; }

        /// <summary>
        /// File extension of the export files (without dot). Return <c>null</c> for a non file based, on-the-fly export.
        /// </summary>
        string FileExtension { get; }

        /// <summary>
        /// Get provider specific configuration information. Return <c>null</c> when no provider specific configuration is required.
        /// </summary>
        ExportConfigurationInfo ConfigurationInfo { get; }

        /// <summary>
        /// Export data to a file
        /// </summary>
        /// <param name="context">Export execution context</param>
        void Execute(ExportExecuteContext context);

        /// <summary>
        /// Called once per store when export execution ended
        /// </summary>
        /// <param name="context">Export execution context</param>
        void OnExecuted(ExportExecuteContext context);
    }
}
