using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.OData;
using SmartStore.Web.Framework.WebApi.Caching;

namespace SmartStore.Web.Framework.WebApi.OData
{
	/// <summary>
	/// The [EnableQuery] attribute enables clients to modify the query, by using query options such as $filter, $sort, and $page.
	/// <see cref="https://docs.microsoft.com/de-de/aspnet/web-api/overview/odata-support-in-aspnet-web-api/supporting-odata-query-options"/>
	/// </summary>
	public class WebApiQueryableAttribute : EnableQueryAttribute
	{
		public bool PagingOptional { get; set; }

		protected virtual bool MissingClientPaging(HttpActionExecutedContext actionExecutedContext)
		{
			if (PagingOptional)
			{
				return false;
			}

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
			catch (Exception ex)
			{
				ex.Dump();
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
