using System.IO;
using SmartStore.Core.Domain;
using SmartStore.Core.Logging;
using SmartStore.Utilities;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
	public class FileSystemFilePublisher : IFilePublisher
	{
		public virtual void Publish(ExportDeploymentContext context, ExportDeployment deployment)
		{
			string destinationFolder = null;

			if (deployment.FileSystemPath.IsEmpty())
			{
				return;
			}
			else if (Path.IsPathRooted(deployment.FileSystemPath))
			{
				destinationFolder = deployment.FileSystemPath;
			}
			else
			{
				destinationFolder = FileSystemHelper.ValidateRootPath(deployment.FileSystemPath);
				destinationFolder = CommonHelper.MapPath(destinationFolder);
			}

			if (!System.IO.Directory.Exists(destinationFolder))
			{
				System.IO.Directory.CreateDirectory(destinationFolder);
			}

			if (!FileSystemHelper.CopyDirectory(new DirectoryInfo(context.FolderContent), new DirectoryInfo(destinationFolder)))
			{
				context.Result.LastError = context.T("Admin.DataExchange.Export.Deployment.CopyFileFailed");
			}

			context.Log.Information("Copied export data files to " + destinationFolder);
		}
	}
}
