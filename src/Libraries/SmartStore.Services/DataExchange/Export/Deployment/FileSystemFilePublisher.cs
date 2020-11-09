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
            var targetFolder = deployment.GetDeploymentFolder(true);

            if (targetFolder.IsEmpty())
                return;

            if (!FileSystemHelper.CopyDirectory(new DirectoryInfo(context.FolderContent), new DirectoryInfo(targetFolder)))
            {
                context.Result.LastError = context.T("Admin.DataExchange.Export.Deployment.CopyFileFailed");
            }

            context.Log.Info($"Copied export data files to {targetFolder}.");
        }
    }
}
