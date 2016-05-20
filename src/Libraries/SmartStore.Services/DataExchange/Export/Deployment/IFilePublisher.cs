using System.Collections.Generic;
using System.IO;
using System.Linq;
using SmartStore.Core.Domain;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
	public interface IFilePublisher
	{
		bool Publish(ExportDeploymentContext context, ExportDeployment deployment);
    }


	public class ExportDeploymentContext
	{
		public ILogger Log { get; set; }

		public string FolderContent { get; set; }

		public string ZipPath { get; set; }

		public bool CreateZipArchive { get; set; }

		public IEnumerable<string> GetDeploymentFiles()
		{
			if (!CreateZipArchive)
			{
				return System.IO.Directory.EnumerateFiles(FolderContent, "*", SearchOption.AllDirectories).ToArray();
			}

			if (File.Exists(ZipPath))
			{
				return new string[] { ZipPath };
			}

			return new string[0];
		}
	}
}
