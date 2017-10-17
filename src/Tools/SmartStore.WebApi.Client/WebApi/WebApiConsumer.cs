using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Windows.Forms;
using SmartStoreNetWebApiClient;

namespace SmartStore.Net.WebApi
{
	public class ApiConsumer : HmacAuthentication
	{
		public static string XmlAcceptType { get { return "application/atom+xml,application/atomsvc+xml,application/xml"; } }
		public static string JsonAcceptType { get { return "application/json, text/javascript, */*"; } }

		private void SetTimeout(HttpWebRequest webRequest)
		{
#if DEBUG
			webRequest.Timeout = 1000 * 60 * 5;	// just for debugging
#endif
		}

		private void WriteToStream(MemoryStream stream, StringBuilder requestContent, string data)
		{
			stream.Write(Encoding.UTF8.GetBytes(data), 0, Encoding.UTF8.GetByteCount(data));
			requestContent.Append(data);
		}

		/// <see cref="http://stackoverflow.com/questions/219827/multipart-forms-from-c-sharp-client" />
		private byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary, StringBuilder requestContent)
		{
			var needsCLRF = false;
			var sb = new StringBuilder();

			using (var stream = new MemoryStream())
			{
				foreach (var param in postParameters)
				{
					if (needsCLRF)
					{
						WriteToStream(stream, requestContent, "\r\n");
					}

					needsCLRF = true;

					if (param.Value is ApiFileParameter)
					{
						var file = (ApiFileParameter)param.Value;

						sb.Clear();
						sb.AppendFormat("--{0}\r\n", boundary);
						sb.AppendFormat("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"", param.Key, file.FileName ?? param.Key);

						foreach (var key in file.Parameters.AllKeys)
						{
							sb.AppendFormat("; {0}=\"{1}\"", key, file.Parameters[key].Replace('"', '\''));
						}

						sb.AppendFormat("\r\nContent-Type: {0}\r\n\r\n", file.ContentType ?? "application/octet-stream");

						WriteToStream(stream, requestContent, sb.ToString());

						stream.Write(file.Data, 0, file.Data.Length);
						requestContent.AppendFormat("<Binary file data here (length {0} bytes)...>", file.Data.Length);
					}
					else
					{
						string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
							boundary,
							param.Key,
							param.Value);

						WriteToStream(stream, requestContent, postData);
					}
				}

				WriteToStream(stream, requestContent, "\r\n--" + boundary + "--\r\n");		// append final newline or not?

				stream.Position = 0;
				byte[] formData = new byte[stream.Length];
				stream.Read(formData, 0, formData.Length);

				return formData;
			}
		}

		private void GetResponse(HttpWebResponse webResponse, WebApiConsumerResponse response, FolderBrowserDialog folderBrowserDialog)
		{
			if (webResponse == null)
				return;

			response.Status = string.Format("{0} {1}", (int)webResponse.StatusCode, webResponse.StatusDescription);
			response.Headers = webResponse.Headers.ToString();
            response.ContentType = webResponse.ContentType;
            response.ContentLength = webResponse.ContentLength;

            if (string.Compare(response.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase) == 0)
            {
                folderBrowserDialog.Description = "Please select a folder to save the PDF file.";
                var dialogResult = folderBrowserDialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    string fileName = null;
                    if (webResponse.Headers["Content-Disposition"] != null)
                    {
                        fileName = webResponse.Headers["Content-Disposition"].Replace("inline; filename=", "").Replace("\"", "");
                    }
                    if (fileName.IsEmpty())
                    {
                        fileName = "web-api-response.pdf";
                    }

                    using (var stream = File.Create(Path.Combine(folderBrowserDialog.SelectedPath, fileName)))
                    {
                        webResponse.GetResponseStream().CopyTo(stream);
                    }
                }
            }
            else
            {
                using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
                {
                    // TODO: file uploads should use async and await keywords
                    response.Content = reader.ReadToEnd();
                }
            }
		}
		
		public static bool BodySupported(string method)
		{
			if (!string.IsNullOrWhiteSpace(method) && string.Compare(method, "GET", true) != 0 && string.Compare(method, "DELETE", true) != 0)
				return true;

			return false;
		}

		public void AddApiFileParameter(Dictionary<string, object> multipartData, string filePath, int pictureId)
		{
			var count = 0;
			var paths = (filePath.Contains(";") ? filePath.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string> { filePath });

			foreach (var path in paths)
			{
				using (var fstream = new FileStream(path, FileMode.Open, FileAccess.Read))
				{
					byte[] data = new byte[fstream.Length];
					fstream.Read(data, 0, data.Length);

					var name = Path.GetFileName(path);
					var id = string.Format("my-file-{0}", ++count);
					var apiFile = new ApiFileParameter(data, name, MimeMapping.GetMimeMapping(name));

					if (pictureId != 0)
					{
						apiFile.Parameters.Add("PictureId", pictureId.ToString());
					}

					// test pass through of custom parameters
					apiFile.Parameters.Add("CustomValue1", string.Format("{0:N}", Guid.NewGuid()));
					apiFile.Parameters.Add("CustomValue2", string.Format("say hello to {0}", id));

					multipartData.Add(id, apiFile);

					fstream.Close();
				}
			}
		}

		public HttpWebRequest StartRequest(WebApiRequestContext context, string content, Dictionary<string, object> multipartData, out StringBuilder requestContent)
		{
			requestContent = new StringBuilder();

			if (context == null || !context.IsValid)
				return null;

			// client system time must not be too far away from api server time! check response header.
			// ISO-8601 utc timestamp with milliseconds (e.g. 2013-09-23T09:24:43.5395441Z)
			string timestamp = DateTime.UtcNow.ToString("o");

			byte[] data = null;
			string contentMd5Hash = "";

			var request = (HttpWebRequest)WebRequest.Create(context.Url);
			SetTimeout(request);

			request.UserAgent = Program.ConsumerName;		// optional
			request.Method = context.HttpMethod;

			request.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
			request.Headers.Add(HttpRequestHeader.CacheControl, "no-cache, no-store");

			request.Accept = context.HttpAcceptType;
			request.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");

			request.Headers.Add(WebApiGlobal.HeaderName.PublicKey, context.PublicKey);
			request.Headers.Add(WebApiGlobal.HeaderName.Date, timestamp);

			if (multipartData != null && multipartData.Count > 0)
			{
				var formDataBoundary = string.Format("----------{0:N}", Guid.NewGuid());

				data = GetMultipartFormData(multipartData, formDataBoundary, requestContent);
				contentMd5Hash = CreateContentMd5Hash(data);

				request.ContentLength = data.Length;
				request.ContentType = "multipart/form-data; boundary=" + formDataBoundary;
			}
			else if (!string.IsNullOrWhiteSpace(content) && BodySupported(request.Method))
			{
				requestContent.Append(content);
				data = Encoding.UTF8.GetBytes(content);
				contentMd5Hash = CreateContentMd5Hash(data);
				
				request.ContentLength = data.Length;
				request.ContentType = "application/json; charset=utf-8";
			}
			else if (BodySupported(request.Method))
			{
				request.ContentLength = 0;
			}

			if (!string.IsNullOrEmpty(contentMd5Hash))
			{
				// optional... provider returns HmacResult.ContentMd5NotMatching if there's no match
				request.Headers.Add(HttpRequestHeader.ContentMd5, contentMd5Hash);
			}

			string messageRepresentation = CreateMessageRepresentation(context, contentMd5Hash, timestamp, true);
			//Debug.WriteLine(messageRepresentation);
			string signature = CreateSignature(context.SecretKey, messageRepresentation);

			request.Headers.Add(HttpRequestHeader.Authorization, CreateAuthorizationHeader(signature));

			if (data != null)
			{
				using (var stream = request.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}
			}

			requestContent.Insert(0, request.Headers.ToString());

			return request;
		}
		
		public bool ProcessResponse(HttpWebRequest webRequest, WebApiConsumerResponse response, FolderBrowserDialog folderBrowserDialog)
		{
			if (webRequest == null)
				return false;

			bool result = true;
			HttpWebResponse webResponse = null;

			try
			{
				webResponse = webRequest.GetResponse() as HttpWebResponse;
				GetResponse(webResponse, response, folderBrowserDialog);
			}
			catch (WebException wexc)
			{
				result = false;
				webResponse = wexc.Response as HttpWebResponse;
				GetResponse(webResponse, response, folderBrowserDialog);
			}
			catch (Exception exc)
			{
				result = false;
				response.Content = string.Format("{0}\r\n{1}", exc.Message, exc.StackTrace);
			}
			finally
			{
				if (webResponse != null)
				{
					webResponse.Close();
					webResponse.Dispose();
				}
			}
			return result;
		}		
	}


	public class ApiFileParameter
	{
		public ApiFileParameter(byte[] data)
			: this(data, null)
		{
		}
		public ApiFileParameter(byte[] data, string filename)
			: this(data, filename, null)
		{
		}
		public ApiFileParameter(byte[] data, string filename, string contenttype)
		{
			Data = data;
			FileName = filename;
			ContentType = contenttype;
			Parameters = new NameValueCollection();
		}

		public byte[] Data { get; set; }
		public string FileName { get; set; }
		public string ContentType { get; set; }

		public NameValueCollection Parameters { get; set; }
	}
}
