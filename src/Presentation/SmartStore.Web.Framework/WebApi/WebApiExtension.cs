using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SmartStore.Web.Framework.WebApi
{
	public static class WebApiExtension
	{
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
	}
}
