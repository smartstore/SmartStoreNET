using SmartStore.Core.Domain;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
	public interface IFilePublisher
	{
		void Publish(ExportDeploymentContext context, ExportDeployment deployment);
    }


	public class ExportDeploymentContext
	{
		public TraceLogger Log { get; set; }

		public string[] DeploymentFiles { get; set; }

		public string FolderContent { get; set; }

		public string ZipPath { get; set; }
	}
}
