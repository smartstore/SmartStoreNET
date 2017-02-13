using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using SmartStore.Web.Framework.WebApi.Caching;

namespace SmartStore.Web.Framework.WebApi.OData
{
	public class WebApiQueryableAttribute : EnableQueryAttribute
	{
		public bool PagingOptional { get; set; }

		protected virtual bool MissingClientPaging(HttpActionExecutedContext actionExecutedContext)
		{
			if (PagingOptional)
				return false;

			try
			{
				var content = actionExecutedContext.Response.Content as ObjectContent;

				if (MaxTop == 0)
				{
					var controllingData = WebApiCachingControllingData.Data();

					MaxTop = controllingData.MaxTop;
					MaxExpansionDepth = controllingData.MaxExpansionDepth;
				}

				if (content != null)
				{
					if (content.Value is HttpError)
						return false;

					if (content.Value is SingleResult)
						return false;	// 'true' would result in a 500 'internal server error'
				}

				var query = actionExecutedContext.Request.RequestUri.Query;
				var missingClientPaging = query.IsEmpty() || !query.Contains("$top=");

				if (missingClientPaging)
				{
					actionExecutedContext.Response = actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
						$"Missing client paging. Please specify odata $top query option. Maximum value is {MaxTop}.");

					return true;
				}
			}
			catch (Exception exception)
			{
				exception.Dump();
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
