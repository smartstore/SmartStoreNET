using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.DevTools.Filters
{
	public class SampleResultFilter : IResultFilter
	{
		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			Debug.WriteLine("OnResultExecuting");

			var result = filterContext.Result as ViewResultBase;
			if (result != null)
			{
				var model = result.Model as ModelBase;
				if (model != null)
				{
					// Do something with model here!
					// If you want to use view models from SmartStore.Web
					// or SmartStore.Admin make sure you have added
					// the project reference first.
				}
			}
		}
		
		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
			Debug.WriteLine("OnResultExecuted");
		}

	}
}
