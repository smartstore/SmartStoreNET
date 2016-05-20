using System.IO;
using SmartStore.Core.Domain;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
	public class PublicFolderPublisher : IFilePublisher
	{
		public virtual bool Publish(ExportDeploymentContext context, ExportDeployment deployment)
		{
			var destinationFolder = deployment.GetPublicFolder(true);
			var result = false;

			if (destinationFolder.IsEmpty())
				return false;

			if (context.CreateZipArchive)
			{
				if (File.Exists(context.ZipPath))
				{
					var destinationFile = Path.Combine(destinationFolder, Path.GetFileName(context.ZipPath));

					File.Copy(context.ZipPath, destinationFile, true);
					result = true;

					context.Log.Information("Copied zipped export data to " + destinationFile);
				}
			}
			else
			{
				result = FileSystemHelper.CopyDirectory(new DirectoryInfo(context.FolderContent), new DirectoryInfo(destinationFolder));

				context.Log.Information("Copied export data files to " + destinationFolder);
			}

			return result;
		}
	}
}
