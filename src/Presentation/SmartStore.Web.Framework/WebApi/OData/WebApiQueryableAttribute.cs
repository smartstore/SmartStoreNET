using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;

namespace SmartStore.Web.Framework.WebApi.OData
{
	public class WebApiQueryableAttribute : QueryableAttribute
	{
		public bool PagingOptional { get; set; }

		protected virtual bool MissingClientPaging(HttpActionExecutedContext actionExecutedContext)
		{
			if (PagingOptional)
				return false;

			try
			{
				var responseContent = actionExecutedContext.Response.Content as ObjectContent;
				bool singleResult = (responseContent != null && responseContent.Value is SingleResult);

				if (singleResult)
					return false;	// 'true' would result in a 500 'internal server error'

				var query = actionExecutedContext.Request.RequestUri.Query;

				bool missingClientPaging = query.IsEmpty() || !query.Contains("$top=");

				if (missingClientPaging)
				{
					actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
						"Missing client paging. Please specify odata $top query option. Maximum value is {0}.".FormatWith(WebApiGlobal.MaxTop));

					return true;
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return false;
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			if (MissingClientPaging(actionExecutedContext))
				return;

			base.OnActionExecuted(actionExecutedContext);
		}
	}
}
