using System;
using System.IO;
using System.Linq;
using System.Net;
using SmartStore.Core.Domain;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
	public class FtpFilePublisher : IFilePublisher
	{
		public virtual void Publish(ExportDeploymentContext context, ExportDeployment deployment)
		{
			var bytesRead = 0;
			var succeededFiles = 0;
			var url = deployment.Url;
			var buffLength = 32768;
			byte[] buff = new byte[buffLength];
			var deploymentFiles = context.DeploymentFiles.ToList();
			var lastIndex = (deploymentFiles.Count - 1);

			if (!url.StartsWith("ftp://", StringComparison.InvariantCultureIgnoreCase))
				url = "ftp://" + url;

			foreach (var path in deploymentFiles)
			{
				var fileUrl = url.EnsureEndsWith("/") + Path.GetFileName(path);

				var request = (FtpWebRequest)System.Net.WebRequest.Create(fileUrl);
				request.Method = WebRequestMethods.Ftp.UploadFile;
				request.KeepAlive = (deploymentFiles.IndexOf(path) != lastIndex);
				request.UseBinary = true;
				request.Proxy = null;
				request.UsePassive = deployment.PassiveMode;
				request.EnableSsl = deployment.UseSsl;

				if (deployment.Username.HasValue())
					request.Credentials = new NetworkCredential(deployment.Username, deployment.Password);

				request.ContentLength = (new FileInfo(path)).Length;

				var requestStream = request.GetRequestStream();

				using (var stream = new FileStream(path, FileMode.Open))
				{
					while ((bytesRead = stream.Read(buff, 0, buffLength)) != 0)
					{
						requestStream.Write(buff, 0, bytesRead);
					}
				}

				requestStream.Close();

				using (var response = (FtpWebResponse)request.GetResponse())
				{
					var statusCode = (int)response.StatusCode;

					if (statusCode >= 200 && statusCode <= 299)
					{
						++succeededFiles;
					}
					else
					{
						var msg = "The FTP transfer might fail. {0} ({1}), {2}. File {3}".FormatInvariant(
							response.StatusCode.ToString(), statusCode, response.StatusDescription.NaIfEmpty(), path);

						context.Log.Error(msg);
					}
				}
			}

			context.Log.Information("{0} file(s) successfully uploaded via FTP.".FormatInvariant(succeededFiles));
		}
	}
}
