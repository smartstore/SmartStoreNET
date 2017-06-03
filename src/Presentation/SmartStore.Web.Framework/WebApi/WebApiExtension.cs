using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.OData.Routing;
using SmartStore.Utilities;

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
		public static HttpResponseMessage CreateResponseForEntity(this HttpRequestMessage request, object entity, int key)
		{
			if (entity == null)
				return request.CreateResponse(HttpStatusCode.NotFound, WebApiGlobal.Error.EntityNotFound.FormatInvariant(key));

			return request.CreateResponse(HttpStatusCode.OK, entity.GetType(), entity);
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
		public static HttpResponseException ExceptionForbidden(this ApiController apiController, string message)
		{
			return new HttpResponseException(apiController.Request.CreateErrorResponse(HttpStatusCode.Forbidden, message));
		}
		public static HttpResponseException ExceptionUnsupportedMediaType(this ApiController apiController)
		{
			return new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
		}
		public static HttpResponseException ExceptionNotFound(this ApiController apiController, string message)
		{
			return new HttpResponseException(apiController.Request.CreateErrorResponse(HttpStatusCode.NotFound, message));
		}
		public static HttpResponseException ExceptionInternalServerError(this ApiController apiController, Exception exc)
		{
			return new HttpResponseException(apiController.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc));
		}

		/// <summary>
		/// Further entity processing typically used by OData actions.
		/// </summary>
		/// <param name="process">Action for entity processing.</param>
		public static void ProcessEntity(this ApiController apiController, Action process)
		{
			try
			{
				process();
			}
			catch (Exception exception)
			{
				throw apiController.ExceptionUnprocessableEntity(exception.Message);
			}
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

		public static string GetNavigation(this ODataPath odataPath, int segmentIndex)
		{
			if (odataPath.Segments.Count > segmentIndex)
			{
				string navigationProperty = (odataPath.Segments[segmentIndex] as NavigationPathSegment).NavigationPropertyName;

				return navigationProperty;
			}
			return null;
		}

		public static void DeleteLocalFiles(this MultipartFormDataStreamProvider provider)
		{
			try
			{
				foreach (var file in provider.FileData)
					FileSystemHelper.Delete(file.LocalFileName);
			}
			catch { }
		}

		public static T GetService<T>(this IDependencyScope dependencyScope)
		{
			return (T)dependencyScope.GetService(typeof(T));
		}
	}
}
