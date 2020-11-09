using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace SmartStore.Web.Framework.WebApi
{
    public static class ApiControllerExtensions
    {
        public static HttpResponseException InvalidModelStateException(this ApiController apiController)
        {
            return new HttpResponseException(apiController.Request.CreateErrorResponse(HttpStatusCode.BadRequest, apiController.ModelState));
        }

        /// <summary>
        /// Further entity processing typically used by OData actions. 
        /// Is mainly used to capture errors of the service project and in that case generate a 422 status code instead of a 500.
        /// </summary>
        /// <param name="process">Action for entity processing.</param>
        public static void ProcessEntity(this ApiController apiController, Action process)
        {
            if (!apiController.ModelState.IsValid)
            {
                throw apiController.InvalidModelStateException();
            }

            try
            {
                process();
            }
            catch (HttpResponseException hrEx)
            {
                // Do not catch exceptions thrown within process action.
                throw hrEx;
            }
            catch (Exception ex)
            {
                // Capture exception because a 422 is more suitable.
                throw apiController.Request.UnprocessableEntityException(ex.Message);
            }
        }

        public static async Task ProcessEntityAsync(this ApiController apiController, Func<Task> process)
        {
            if (!apiController.ModelState.IsValid)
            {
                throw apiController.InvalidModelStateException();
            }

            try
            {
                await process();
            }
            catch (HttpResponseException hrEx)
            {
                throw hrEx;
            }
            catch (Exception ex)
            {
                throw apiController.Request.UnprocessableEntityException(ex.Message);
            }
        }

        /// <summary>
        /// Gets a query string value from API request URL.
        /// </summary>
        /// <remarks>
        /// Query string values are not part of the EDM and therefore do not appear in any auto generated documentation etc.
        /// </remarks>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="apiController">API controller.</param>
        /// <param name="name">Name of the query string value.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <returns>Query string value.</returns>
        public static T GetQueryStringValue<T>(this ApiController apiController, string name, T defaultValue = default)
        {
            Guard.NotEmpty(name, nameof(name));

            var queries = apiController?.Request?.RequestUri?.ParseQueryString();

            if (queries?.AllKeys?.Contains(name) ?? false)
            {
                return queries[name].Convert(defaultValue, CultureInfo.InvariantCulture);
            }

            return defaultValue;
        }
    }
}
