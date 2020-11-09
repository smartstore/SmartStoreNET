using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using System.Web.OData;
using SmartStore.Web.Framework.WebApi.Caching;

namespace SmartStore.Web.Framework.WebApi.OData
{
    /// <summary>
    /// The [EnableQuery] attribute enables clients to modify the query, by using query options such as $expand, $filter, $sort, and $page.
    /// <see cref="https://docs.microsoft.com/de-de/aspnet/web-api/overview/odata-support-in-aspnet-web-api/supporting-odata-query-options"/>
    /// </summary>
    /// <remarks>
    /// [AutoExpand] is ignored when [EnableQuery] is missing. Always required if navigation properties are to be expanded.
    /// </remarks>
    public class WebApiQueryableAttribute : EnableQueryAttribute
    {
        protected virtual void SetDefaultQueryOptions(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                if (MaxTop == 0)
                {
                    var controllingData = WebApiCachingControllingData.Data();

                    MaxTop = controllingData.MaxTop;
                    MaxExpansionDepth = controllingData.MaxExpansionDepth;
                }

                var content = actionExecutedContext?.Response?.Content as ObjectContent;
                if (content?.Value is HttpError || content?.Value is SingleResult)
                {
                    // Paging not required.
                    return;
                }

                var hasClientPaging = actionExecutedContext?.Request?.RequestUri?.Query?.Contains("$top=") ?? false;
                if (!hasClientPaging)
                {
                    // If paging is required and there is no $top sent by client then force the page size specified by merchant.
                    PageSize = MaxTop;
                }
            }
            catch (Exception ex)
            {
                ex.Dump();
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            SetDefaultQueryOptions(actionExecutedContext);

            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
