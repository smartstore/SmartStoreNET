using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using SmartStore.Core.Data;
using SmartStore.Services.Helpers;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.Filters
{
	public class JsonNetAttribute : FilterAttribute, IResultFilter
	{
		public Lazy<IDateTimeHelper> DateTimeHelper { get; set; }

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public virtual void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (!DataSettings.DatabaseIsInstalled())
				return;

			if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
				return;

			// don't apply filter to child methods
			if (filterContext.IsChildAction)
				return;

			// handle JsonResult only
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

		public virtual void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}
	}

}
