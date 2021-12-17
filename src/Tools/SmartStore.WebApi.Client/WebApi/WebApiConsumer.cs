using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using SmartStore.WebApi.Client.Models;

namespace SmartStore.WebApi.Client
{
    public class ApiConsumer : HmacAuthentication
    {
        public static string XmlAcceptType => "application/atom+xml,application/xml";
        public static string JsonAcceptType => "application/json";

        private void SetTimeout(HttpWebRequest webRequest)
        {
#if DEBUG
            // Just for debugging.
            webRequest.Timeout = 1000 * 60 * 5;
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

                WriteToStream(stream, requestContent, "\r\n--" + boundary + "--\r\n");      // append final newline or not?

                stream.Position = 0;
                byte[] formData = new byte[stream.Length];
                stream.Read(formData, 0, formData.Length);

                return formData;
            }
        }

        private void GetResponse(HttpWebResponse webResponse, WebApiConsumerResponse response, FolderBrowserDialog dialog)
        {
            if (webResponse == null)
            {
                return;
            }

            response.Status = string.Format("{0} {1}", (int)webResponse.StatusCode, webResponse.StatusDescription);
            response.Headers = webResponse.Headers.ToString();
            response.ContentType = webResponse.ContentType;
            response.ContentLength = webResponse.ContentLength;

            var ct = response.ContentType;

            if (ct.HasValue() && (ct.StartsWith("image/") || ct.StartsWith("video/") || ct == "application/pdf"))
            {
                dialog.Description = "Please select a folder to save the file return by Web API.";

                var dialogResult = dialog.ShowDialog();
                if (dialogResult == DialogResult.OK)
                {
                    string fileName = null;
                    if (webResponse.Headers["Content-Disposition"] != null)
                    {
                        fileName = webResponse.Headers["Content-Disposition"].Replace("inline; filename=", "").Replace("\"", "");
                    }
                    if (fileName.IsEmpty())
                    {
                        fileName = "web-api-response";
                    }

                    var path = Path.Combine(dialog.SelectedPath, fileName);

                    using (var stream = File.Create(path))
                    {
                        webResponse.GetResponseStream().CopyTo(stream);
                    }

                    System.Diagnostics.Process.Start(path);
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
            {
                return true;
            }

            return false;
        }

        public static bool MultipartSupported(string method)
        {
            // At the moment all API methods with multipart support are POST methods.
            return string.Compare(method, "POST", true) == 0;
        }

        public Dictionary<string, object> CreateMultipartData(FileUploadModel model)
        {
            if (!(model?.Files?.Any() ?? false))
            {
                return null;
            }

            var result = new Dictionary<string, object>();
            var isValid = false;
            var count = 0;

            // Identify entity by its identifier.
            if (model.Id != 0)
            {
                result.Add("Id", model.Id);
            }

            // Custom properties like SKU etc.
            foreach (var kvp in model.CustomProperties)
            {
                if (kvp.Key.HasValue() && kvp.Value != null)
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }

            // File data.
            foreach (var file in model.Files)
            {
                if (File.Exists(file.LocalPath))
                {
                    using (var fstream = new FileStream(file.LocalPath, FileMode.Open, FileAccess.Read))
                    {
                        byte[] data = new byte[fstream.Length];
                        fstream.Read(data, 0, data.Length);

                        var name = Path.GetFileName(file.LocalPath);
                        var id = string.Format("my-file-{0}", ++count);
                        var apiFile = new ApiFileParameter(data, name, MimeMapping.GetMimeMapping(name));

                        // Add file parameters. Omit default values (let the server apply them).
                        if (file.Id != 0)
                        {
                            apiFile.Parameters.Add("PictureId", file.Id.ToString());
                        }
                        if (file.Path.HasValue())
                        {
                            apiFile.Parameters.Add("Path", file.Path);
                        }
                        if (!file.IsTransient)
                        {
                            apiFile.Parameters.Add("IsTransient", file.IsTransient.ToString());
                        }
                        if (file.DuplicateFileHandling != DuplicateFileHandling.ThrowError)
                        {
                            apiFile.Parameters.Add("DuplicateFileHandling", ((int)file.DuplicateFileHandling).ToString());
                        }

                        // Test pass through of custom parameters but the API ignores them anyway.
                        //apiFile.Parameters.Add("CustomValue1", string.Format("{0:N}", Guid.NewGuid()));
                        //apiFile.Parameters.Add("CustomValue2", string.Format("say hello to {0}", id));

                        result.Add(id, apiFile);
                        isValid = true;
                        fstream.Close();
                    }
                }
            }

            if (!isValid)
            {
                return null;
            }

            return result;
        }

        //public void AddApiFileParameter(Dictionary<string, object> multipartData, string filePath, int pictureId)
        //{
        //	var count = 0;
        //	var paths = filePath.Contains(";") ? filePath.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string> { filePath };

        //	foreach (var path in paths)
        //	{
        //		using (var fstream = new FileStream(path, FileMode.Open, FileAccess.Read))
        //		{
        //			byte[] data = new byte[fstream.Length];
        //			fstream.Read(data, 0, data.Length);

        //			var name = Path.GetFileName(path);
        //			var id = string.Format("my-file-{0}", ++count);
        //			var apiFile = new ApiFileParameter(data, name, MimeMapping.GetMimeMapping(name));

        //			if (pictureId != 0)
        //			{
        //				apiFile.Parameters.Add("PictureId", pictureId.ToString());
        //			}

        //			// Test pass through of custom parameters
        //			apiFile.Parameters.Add("CustomValue1", string.Format("{0:N}", Guid.NewGuid()));
        //			apiFile.Parameters.Add("CustomValue2", string.Format("say hello to {0}", id));

        //			multipartData.Add(id, apiFile);

        //			fstream.Close();
        //		}
        //	}
        //}

        public HttpWebRequest StartRequest(WebApiRequestContext context, string content, Dictionary<string, object> multipartData, out StringBuilder requestContent)
        {
            requestContent = new StringBuilder();

            if (context == null || !context.IsValid)
            {
                return null;
            }

            // Client system time must not be too far away from APPI server time! Check response header.
            // ISO-8601 utc timestamp with milliseconds (e.g. 2013-09-23T09:24:43.5395441Z).
            string timestamp = DateTime.UtcNow.ToString("o");

            byte[] data = null;
            var contentMd5Hash = string.Empty;

            var request = (HttpWebRequest)WebRequest.Create(context.Url);
            SetTimeout(request);

            // Optional.
            request.UserAgent = Program.ConsumerName;
            request.Method = context.HttpMethod;

            request.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
            request.Headers.Add(HttpRequestHeader.CacheControl, "no-cache, no-store");

            request.Accept = context.HttpAcceptType;
            request.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");

            request.Headers.Add(WebApiGlobal.HeaderName.PublicKey, context.PublicKey);
            request.Headers.Add(WebApiGlobal.HeaderName.Date, timestamp);

            // Additional headers.
            if (context.AdditionalHeaders.HasValue())
            {
                var jsonHeaders = JObject.Parse(context.AdditionalHeaders);
                foreach (var item in jsonHeaders)
                {
                    var value = item.Value?.ToString();
                    if (item.Key.HasValue() && value.HasValue())
                    {
                        request.Headers.Add(item.Key, value);
                    }
                }
            }

            if (BodySupported(context.HttpMethod))
            {
                if (MultipartSupported(context.HttpMethod) && (multipartData?.Any() ?? false))
                {
                    var formDataBoundary = string.Format("----------{0:N}", Guid.NewGuid());

                    data = GetMultipartFormData(multipartData, formDataBoundary, requestContent);
                    contentMd5Hash = CreateContentMd5Hash(data);

                    request.ContentLength = data.Length;
                    request.ContentType = "multipart/form-data; boundary=" + formDataBoundary;
                }
                else if (!string.IsNullOrWhiteSpace(content))
                {
                    requestContent.Append(content);
                    data = Encoding.UTF8.GetBytes(content);
                    contentMd5Hash = CreateContentMd5Hash(data);

                    request.ContentLength = data.Length;
                    request.ContentType = "application/json; charset=utf-8";
                }
                else
                {
                    request.ContentLength = 0;
                }
            }

            if (!string.IsNullOrEmpty(contentMd5Hash))
            {
                // Optional. Provider returns HmacResult.ContentMd5NotMatching if there's no match.
                request.Headers.Add(HttpRequestHeader.ContentMd5, contentMd5Hash);
            }

            // API behind a reverse proxy?
            if (context.ProxyPort > 0)
            {
                context.Url = new UriBuilder(context.Url) { Port = context.ProxyPort }.Uri.ToString();
            }

            var messageRepresentation = CreateMessageRepresentation(context, contentMd5Hash, timestamp, true);
            //Debug.WriteLine(messageRepresentation);
            var signature = CreateSignature(context.SecretKey, messageRepresentation);

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
            {
                return false;
            }

            var result = true;
            HttpWebResponse webResponse = null;

            try
            {
                webResponse = webRequest.GetResponse() as HttpWebResponse;
                GetResponse(webResponse, response, folderBrowserDialog);
            }
            catch (WebException wex)
            {
                result = false;
                webResponse = wex.Response as HttpWebResponse;
                GetResponse(webResponse, response, folderBrowserDialog);
            }
            catch (Exception ex)
            {
                result = false;
                response.Content = string.Format("{0}\r\n{1}", ex.Message, ex.StackTrace);
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
}
