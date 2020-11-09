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
            var destinationFolder = deployment.GetDeploymentFolder(true);

            if (destinationFolder.IsEmpty())
                return;

            if (!System.IO.Directory.Exists(destinationFolder))
            {
                System.IO.Directory.CreateDirectory(destinationFolder);
            }

            if (context.CreateZipArchive)
            {
                if (File.Exists(context.ZipPath))
                {
                    var destinationFile = Path.Combine(destinationFolder, Path.GetFileName(context.ZipPath));

                    File.Copy(context.ZipPath, destinationFile, true);

                    context.Log.Info($"Copied zipped export data to {destinationFile}.");
                }
            }
            else
            {
                if (!FileSystemHelper.CopyDirectory(new DirectoryInfo(context.FolderContent), new DirectoryInfo(destinationFolder)))
                {
                    context.Result.LastError = context.T("Admin.DataExchange.Export.Deployment.CopyFileFailed");
                }

                context.Log.Info($"Copied export data files to {destinationFolder}.");
            }
        }
    }
}
