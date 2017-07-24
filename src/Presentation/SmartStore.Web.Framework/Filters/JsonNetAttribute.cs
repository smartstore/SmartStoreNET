using System;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Core.Data;
using SmartStore.Services.Helpers;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.Filters
{
	public class JsonNetAttribute : FilterAttribute, IActionFilter
	{
		public Lazy<IDateTimeHelper> DateTimeHelper { get; set; }

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (!DataSettings.DatabaseIsInstalled())
				return;

			if (filterContext == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
				return;

			// Don't apply filter to child methods.
			if (filterContext.IsChildAction)
				return;

			// Hndle JsonResult only.
			if (filterContext.Result.GetType() != typeof(JsonResult))
				return;

			var jsonResult = filterContext.Result as JsonResult;
			var settings = new JsonSerializerSettings
			{
				MissingMemberHandling = MissingMemberHandling.Ignore,
				TypeNameHandling = TypeNameHandling.Objects,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
				DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
				DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,

				// We cannot ignore null. Client template of several Telerik grids would fail.
				//NullValueHandling = NullValueHandling.Ignore,

				MaxDepth = 32
			};

			filterContext.Result = new JsonNetResult(DateTimeHelper.Value, settings)
			{
				Data = jsonResult.Data,
				ContentType = jsonResult.ContentType,
				ContentEncoding = jsonResult.ContentEncoding,
				JsonRequestBehavior = jsonResult.JsonRequestBehavior,
				MaxJsonLength = jsonResult.MaxJsonLength,
				RecursionLimit = jsonResult.RecursionLimit
			};
		}
	}

}
