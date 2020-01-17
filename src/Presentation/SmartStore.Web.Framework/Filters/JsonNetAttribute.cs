using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.ComponentModel;
using SmartStore.Core.Data;
using SmartStore.Services.Helpers;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.Filters
{
	public class JsonNetAttribute : FilterAttribute, IResultFilter
	{
		public Lazy<IDateTimeHelper> DateTimeHelper { get; set; }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            if (filterContext?.HttpContext?.Request == null)
                return;

            // Don't apply filter to child methods.
            if (filterContext.IsChildAction)
                return;

            // Hndle JsonResult only.
            if (filterContext.Result.GetType() != typeof(JsonResult))
                return;

            var jsonResult = filterContext.Result as JsonResult;

            filterContext.Result = new JsonNetResult(DateTimeHelper.Value)
            {
                Data = jsonResult.Data,
                ContentType = jsonResult.ContentType,
                ContentEncoding = jsonResult.ContentEncoding,
                JsonRequestBehavior = jsonResult.JsonRequestBehavior,
                MaxJsonLength = jsonResult.MaxJsonLength,
                RecursionLimit = jsonResult.RecursionLimit
            };
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }

}
