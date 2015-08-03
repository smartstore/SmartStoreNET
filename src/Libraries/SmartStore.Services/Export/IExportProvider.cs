using System.IO;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Export;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Export
{
	public partial interface IExportProvider : IProvider, IUserEditable
	{
		MemoryStream Execute(ExportProfile profile);

		ExportFileType[] SupportedFileTypes { get; }

		ExportEntityType EntityType { get; }
	}
}
