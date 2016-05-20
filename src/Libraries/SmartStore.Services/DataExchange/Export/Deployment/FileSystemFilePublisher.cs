using System.IO;
using SmartStore.Core.Domain;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
	public class FileSystemFilePublisher : IFilePublisher
	{
		public virtual bool Publish(ExportDeploymentContext context, ExportDeployment deployment)
		{
			string destinationFolder = null;

			if (deployment.FileSystemPath.IsEmpty())
			{
				return false;
			}
			else if (Path.IsPathRooted(deployment.FileSystemPath))
			{
				destinationFolder = deployment.FileSystemPath;
			}
			else
			{
				destinationFolder = deployment.FileSystemPath;

				if (!destinationFolder.StartsWith("~/"))
				{
					if (destinationFolder.StartsWith("~"))
						destinationFolder = destinationFolder.Substring(1);

					destinationFolder = (destinationFolder.StartsWith("/") ? "~" : "~/") + destinationFolder;
				}

				destinationFolder = CommonHelper.MapPath(destinationFolder);
			}

			if (!System.IO.Directory.Exists(destinationFolder))
			{
				System.IO.Directory.CreateDirectory(destinationFolder);
			}

			FileSystemHelper.CopyDirectory(new DirectoryInfo(context.FolderContent), new DirectoryInfo(destinationFolder));

			context.Log.Information("Copied export data files to " + destinationFolder);

			return true;
		}
	}
}
