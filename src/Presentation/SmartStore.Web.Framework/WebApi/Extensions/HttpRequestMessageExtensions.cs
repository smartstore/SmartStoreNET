using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;

namespace SmartStore.Web.Framework.WebApi
{
    public static class HttpRequestMessageExtensions
    {
        private static MethodInfo _createResponse = InitCreateResponse();

        private static MethodInfo InitCreateResponse()
        {
            Expression<Func<HttpRequestMessage, HttpResponseMessage>> expr = (request) => request.CreateResponse<object>(HttpStatusCode.OK, default(object));

            return (expr.Body as MethodCallExpression).Method.GetGenericMethodDefinition();
        }

        /// <see cref="https://gist.github.com/raghuramn/5084608"/>
        public static HttpResponseMessage CreateResponse(this HttpRequestMessage request, HttpStatusCode status, Type type, object value)
        {
            return _createResponse.MakeGenericMethod(type).Invoke(null, new[] { request, status, value }) as HttpResponseMessage;
        }

        public static HttpResponseException BadRequestException(this HttpRequestMessage request, string message)
        {
            return new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest, message));
        }

        public static HttpResponseException UnprocessableEntityException(this HttpRequestMessage request, string message)
        {
            return new HttpResponseException(request.CreateErrorResponse((HttpStatusCode)422, message));
        }

        public static HttpResponseException NotFoundException(this HttpRequestMessage request, string message)
        {
            return new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.NotFound, message));
        }

        public static HttpResponseException InternalServerErrorException(this HttpRequestMessage request, Exception ex)
        {
            return new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
        }
    }
}
