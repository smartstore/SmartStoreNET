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
				if (context.CreateZipArchive)
				{
					if (File.Exists(context.ZipPath))
					{
						var destinationFile = Path.Combine(destinationFolder, Path.GetFileName(context.ZipPath));

						File.Copy(context.ZipPath, destinationFile, true);

						context.Log.Information("Copied zipped export data to " + destinationFile);
					}
				}
				else
				{
					FileSystemHelper.CopyDirectory(new DirectoryInfo(context.FolderContent), new DirectoryInfo(destinationFolder));

					context.Log.Information("Copied export data files to " + destinationFolder);
				}
			}
		}
	}
}
