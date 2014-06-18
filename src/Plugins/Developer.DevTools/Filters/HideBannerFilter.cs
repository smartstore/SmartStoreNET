using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Web.Models.Common;

namespace SmartStore.Plugin.Developer.DevTools.Filters
{
	public class HideBannerFilter : IResultFilter
	{

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }
		
		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			var result = filterContext.Result as ViewResultBase;
			if (result != null)
			{
				var model = result.Model as FooterModel;
				if (model != null)
				{
					model.SmartStoreHint = "";
				}
			}
		}
		
		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}

	}
}
