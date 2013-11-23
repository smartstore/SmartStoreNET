using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace SmartStore.Web.Framework.WebApi.OData
{
	public class WebApiQueryableAttribute : QueryableAttribute
	{
		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			var request = actionExecutedContext.Request;
			var response = actionExecutedContext.Response;

			if (request != null && request.RequestUri != null && response != null && response.IsSuccessStatusCode)
			{
				var responseContent = actionExecutedContext.Response.Content as ObjectContent;

				if (responseContent != null)
				{
					// TODO: check
					//responseContent.Value is SingleResult

					var query = request.RequestUri.Query;

					bool missingClientPaging = query.IsNullOrEmpty() || !query.Contains("$top=");

					if (missingClientPaging)
					{
						actionExecutedContext.Response = request.CreateErrorResponse(HttpStatusCode.BadRequest,
							"Missing client paging. Please specify odata $top query option. Maximum value is {0}.".FormatWith(WebApiGlobal.MaxTop));

						return;
					}
				}
			}

			base.OnActionExecuted(actionExecutedContext);
		}
	}
}
