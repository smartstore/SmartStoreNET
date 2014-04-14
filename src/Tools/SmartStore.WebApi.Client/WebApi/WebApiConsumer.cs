using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace SmartStore.Net.WebApi
{
	public class ApiConsumer : HmacAuthentication
	{
		private const string _consumerName = "My shopping data consumer v.1.0";

		public static string XmlAcceptType { get { return "application/atom+xml,application/atomsvc+xml,application/xml"; } }
		public static string JsonAcceptType { get { return "application/json, text/javascript, */*"; } }

		private void SetTimeout(HttpWebRequest webRequest)
		{
#if DEBUG
			webRequest.Timeout = 1000 * 60 * 5;	// just for debugging
#endif
		}
		private void GetResponse(HttpWebResponse webResponse, WebApiConsumerResponse response)
		{
			if (webResponse == null)
				return;

			response.Status = string.Format("{0} {1}", (int)webResponse.StatusCode, webResponse.StatusDescription);
			response.Headers = webResponse.Headers.ToString();

			using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
			{
				response.Content = reader.ReadToEnd();
			}
		}
		
		public static bool BodySupported(string method)
		{
			if (!string.IsNullOrWhiteSpace(method) && string.Compare(method, "GET", true) != 0 && string.Compare(method, "DELETE", true) != 0)
				return true;

			return false;
		}
		public HttpWebRequest StartRequest(WebApiRequestContext context, string content, StringBuilder requestContent)
		{
			if (context == null || !context.IsValid)
				return null;

			// client system time must not be too far away from api server time! check response header.
			// ISO-8601 utc timestamp with milliseconds (e.g. 2013-09-23T09:24:43.5395441Z)
			string timestamp = DateTime.UtcNow.ToString("o");

			byte[] data = null;
			string contentMd5Hash = "";

			var request = (HttpWebRequest)WebRequest.Create(context.Url);
			SetTimeout(request);

			request.UserAgent = _consumerName;		// optional
			request.Method = context.HttpMethod;

			request.Headers.Add(HttpRequestHeader.Pragma, "no-cache");
			request.Headers.Add(HttpRequestHeader.CacheControl, "no-cache, no-store");

			request.Accept = context.HttpAcceptType;
			request.Headers.Add(HttpRequestHeader.AcceptCharset, "UTF-8");

			request.Headers.Add(WebApiGlobal.HeaderName.PublicKey, context.PublicKey);
			request.Headers.Add(WebApiGlobal.HeaderName.Date, timestamp);

			if (!string.IsNullOrWhiteSpace(content) && BodySupported(request.Method))
			{
				data = Encoding.UTF8.GetBytes(content);
				
				request.ContentLength = data.Length;
				request.ContentType = "application/json; charset=utf-8";

				contentMd5Hash = CreateContentMd5Hash(data);

				// optional... provider returns HmacResult.ContentMd5NotMatching if there's no match
				request.Headers.Add(HttpRequestHeader.ContentMd5, contentMd5Hash);
			}
			else if (BodySupported(request.Method))
			{
				request.ContentLength = 0;
			}

			string messageRepresentation = CreateMessageRepresentation(context, contentMd5Hash, timestamp);
			//Debug.WriteLine(messageRepresentation);
			string signature = CreateSignature(context.SecretKey, messageRepresentation);

			request.Headers.Add(HttpRequestHeader.Authorization, CreateAuthorizationHeader(signature));

			if (data != null)
			{
				using (var stream = request.GetRequestStream())
				{
					stream.Write(data, 0, data.Length);
				}
				requestContent.Append(content);
			}

			requestContent.Insert(0, request.Headers.ToString());

			return request;
		}
		public bool ProcessResponse(HttpWebRequest webRequest, WebApiConsumerResponse response)
		{
			if (webRequest == null)
				return false;

			bool result = true;
			HttpWebResponse webResponse = null;

			try
			{
				webResponse = webRequest.GetResponse() as HttpWebResponse;
				GetResponse(webResponse, response);
			}
			catch (WebException wexc)
			{
				result = false;
				webResponse = wexc.Response as HttpWebResponse;
				GetResponse(webResponse, response);
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
		
		/// <remarks>
		/// http://weblog.west-wind.com/posts/2012/Aug/30/Using-JSONNET-for-dynamic-JSON-parsing
		/// http://james.newtonking.com/json/help/index.html?topic=html/QueryJsonDynamic.htm
		/// http://james.newtonking.com/json/help/index.html?topic=html/LINQtoJSON.htm
		/// </remarks>
		public List<Customer> TryParseCustomers(WebApiConsumerResponse response)
		{
			if (response == null || string.IsNullOrWhiteSpace(response.Content))
				return null;

			//dynamic dynamicJson = JObject.Parse(response.Content);

			//foreach (dynamic customer in dynamicJson.value)
			//{
			//	string str = string.Format("{0} {1} {2}", customer.Id, customer.CustomerGuid, customer.Email);
			//	Debug.WriteLine(str);
			//}

			var json = JObject.Parse(response.Content);
			string metadata = (string)json["odata.metadata"];

			if (!string.IsNullOrWhiteSpace(metadata) && metadata.EndsWith("#Customers"))
			{
				var customers = json["value"].Select(x => x.ToObject<Customer>()).ToList();

				return customers;
			}
			return null;
		}
	}
}
