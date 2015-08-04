using System.IO;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.DataExchange
{
	public partial interface IExportProvider : IProvider, IUserEditable
	{
		// TODO: a more complex result type is required. e.g. IEnumerable<ExportSegment>....
		MemoryStream Execute(ExportProfile profile);

		ExportEntityType EntityType { get; }

		string FileType { get; }
	}
}
