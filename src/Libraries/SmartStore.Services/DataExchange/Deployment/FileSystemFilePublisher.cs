using System.IO;
using System.Web;
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

			if (deployment.IsPublic)
			{
				destinationFolder = Path.Combine(HttpRuntime.AppDomainAppPath, DataExporter.PublicFolder);
			}
			else if (deployment.FileSystemPath.IsEmpty())
			{
				return;
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

			if (deployment.CreateZip)
			{
				var name = (new DirectoryInfo(deployment.Profile.FolderName)).Name;
				if (name.IsEmpty())
					name = "ExportData";

				var path = Path.Combine(destinationFolder, name.ToValidFileName() + ".zip");

				if (FileSystemHelper.Copy(context.ZipPath, path))
					context.Log.Information("Copied ZIP archive " + path);
			}
			else
			{
				FileSystemHelper.CopyDirectory(new DirectoryInfo(context.FolderContent), new DirectoryInfo(destinationFolder));

				context.Log.Information("Copied export data files to " + destinationFolder);
			}
		}
	}
}
