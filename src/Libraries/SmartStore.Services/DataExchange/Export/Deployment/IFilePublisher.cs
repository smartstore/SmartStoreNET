using System.Collections.Generic;
using System.IO;
using SmartStore.Core.Domain;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
    public interface IFilePublisher
    {
        void Publish(ExportDeploymentContext context, ExportDeployment deployment);
    }


    public class ExportDeploymentContext
    {
        public Localizer T { get; set; }
        public ILogger Log { get; set; }

        public string FolderContent { get; set; }

        public string ZipPath { get; set; }
        public bool CreateZipArchive { get; set; }

        public DataDeploymentResult Result { get; set; }

        public IEnumerable<string> GetDeploymentFiles()
        {
            if (!CreateZipArchive)
            {
                return System.IO.Directory.EnumerateFiles(FolderContent, "*", SearchOption.AllDirectories);
            }

            if (File.Exists(ZipPath))
            {
                return new string[] { ZipPath };
            }

            return new string[0];
        }
    }
}
