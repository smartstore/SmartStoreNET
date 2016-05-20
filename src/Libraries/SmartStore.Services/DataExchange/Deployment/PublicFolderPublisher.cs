using System.IO;
using SmartStore.Core.Domain;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
	public class PublicFolderPublisher : IFilePublisher
	{
		public virtual void Publish(ExportDeploymentContext context, ExportDeployment deployment)
		{
			var destinationFolder = deployment.GetPublicFolder(true);

			if (destinationFolder.HasValue())
			{
				// TODO: zip

				FileSystemHelper.CopyDirectory(new DirectoryInfo(context.FolderContent), new DirectoryInfo(destinationFolder));

				context.Log.Information("Copied export data files to " + destinationFolder);
			}
		}
	}
}
