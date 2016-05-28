using SmartStore.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Services.DataExchange.Deployment
{
	public interface IFilePublisher
	{
		void Publish(DataExportResult result, ExportDeployment deployment);
    }

	/*
		Implement without IoC: HttpFilePublisher, EmailFilePublisher, FtpFilePublisher, FileSystemFilePublisher
	*/
}
