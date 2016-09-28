using System;
using System.IO;
using System.Net;
using System.Net.Http;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Logging;

namespace SmartStore.Services.DataExchange.Export.Deployment
{
	public class HttpFilePublisher : IFilePublisher
	{
		public virtual void Publish(ExportDeploymentContext context, ExportDeployment deployment)
		{
			var succeededFiles = 0;
			var url = deployment.Url;

			if (!url.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) && !url.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
				url = "http://" + url;

			if (deployment.HttpTransmissionType == ExportHttpTransmissionType.MultipartFormDataPost)
			{
				var countFiles = 0;
				ICredentials credentials = null;

				if (deployment.Username.HasValue())
					credentials = new NetworkCredential(deployment.Username, deployment.Password);

				using (var handler = new HttpClientHandler { Credentials = credentials })
				using (var client = new HttpClient(handler))
				using (var formData = new MultipartFormDataContent())
				{
					foreach (var path in context.GetDeploymentFiles())
					{
						byte[] fileData = File.ReadAllBytes(path);
						formData.Add(new ByteArrayContent(fileData), "file {0}".FormatInvariant(++countFiles), Path.GetFileName(path));
					}

					var response = client.PostAsync(url, formData).Result;

					if (response.IsSuccessStatusCode)
					{
						succeededFiles = countFiles;
					}
					else if (response.Content != null)
					{
						context.Result.LastError = context.T("Admin.Common.HttpStatus", (int)response.StatusCode, response.StatusCode.ToString());

						var content = response.Content.ReadAsStringAsync().Result;

						var msg = "Multipart form data upload failed. HTTP status {0} ({1}). Response: {2}".FormatInvariant(
							(int)response.StatusCode, response.StatusCode.ToString(), content.NaIfEmpty().Truncate(2000, "..."));

						context.Log.Error(msg);
					}
				}
			}
			else
			{
				using (var webClient = new WebClient())
				{
					if (deployment.Username.HasValue())
						webClient.Credentials = new NetworkCredential(deployment.Username, deployment.Password);

					foreach (var path in context.GetDeploymentFiles())
					{
						webClient.UploadFile(url, path);

						++succeededFiles;
					}
				}
			}

			context.Log.Info("{0} file(s) successfully uploaded via HTTP ({1}).".FormatInvariant(succeededFiles, deployment.HttpTransmissionType.ToString()));
		}
	}
}
