using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Deployment
{
	public interface IFilePublisher
	{
		ExportDeploymentType DeploymentType { get; }

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
