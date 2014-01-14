using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.OData.Routing;

namespace SmartStore.Web.Framework.WebApi
{
	public static class WebApiExtension
	{
		private static MethodInfo _createResponse = InitCreateResponse();

		private static MethodInfo InitCreateResponse()
		{
			Expression<Func<HttpRequestMessage, HttpResponseMessage>> expr = (request) => request.CreateResponse<object>(HttpStatusCode.OK, default(object));

			return (expr.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
		}

		/// <remarks>https://gist.github.com/raghuramn/5084608</remarks>
		public static HttpResponseMessage CreateResponse(this HttpRequestMessage request, HttpStatusCode status, Type type, object value)
		{
			return _createResponse.MakeGenericMethod(type).Invoke(null, new[] { request, status, value }) as HttpResponseMessage;
		}

		public static HttpResponseException ExceptionInvalidModelState(this ApiController apiController)
		{
			return new HttpResponseException(apiController.Request.CreateErrorResponse(HttpStatusCode.BadRequest, apiController.ModelState));
		}
		public static HttpResponseException ExceptionBadRequest(this ApiController apiController, string message)
		{
			return new HttpResponseException(apiController.Request.CreateErrorResponse(HttpStatusCode.BadRequest, message));
		}
		public static HttpResponseException ExceptionUnprocessableEntity(this ApiController apiController, string message)
		{
			return new HttpResponseException(apiController.Request.CreateErrorResponse((HttpStatusCode)422, message));
		}
		public static HttpResponseException ExceptionNotImplemented(this ApiController apiController)
		{
			return new HttpResponseException(HttpStatusCode.NotImplemented);
		}
		public static HttpResponseException ExceptionForbidden(this ApiController apiController)
		{
			return new HttpResponseException(HttpStatusCode.Forbidden);
		}

		/// <summary>
		/// Further entity processing typically used by OData actions.
		/// </summary>
		/// <param name="process">Return an error string or null if your processing succeeded.</param>
		public static void ProcessEntity(this ApiController apiController, Func<string> process)
		{
			string error = null;

			try
			{
				error = process();
			}
			catch (Exception exc)
			{
				error = exc.Message;
			}

			if (error.HasValue())
				throw apiController.ExceptionUnprocessableEntity(error);
		}
		public static bool GetNormalizedKey(this ODataPath odataPath, int segmentIndex, out int key)
		{
			if (odataPath.Segments.Count > segmentIndex)
			{
				string rawKey = (odataPath.Segments[segmentIndex] as KeyValuePathSegment).Value;
				if (rawKey.HasValue())
				{
					if (rawKey.StartsWith("'"))
						rawKey = rawKey.Substring(1, rawKey.Length - 2);

					if (int.TryParse(rawKey, out key))
						return true;
				}
			}
			key = 0;
			return false;
		}
	}
}
