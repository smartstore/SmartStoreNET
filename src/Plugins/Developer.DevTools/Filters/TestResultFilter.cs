using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.Plugin.Developer.DevTools.Filters
{
	public class TestResultFilter : IResultFilter
	{
		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			Debug.WriteLine("OnResultExecuting");

			var result = filterContext.Result as ViewResultBase;
			if (result != null)
			{
				var model = result.Model as ProductDetailsModel;
				if (model != null)
				{
					model.ProductPrice.OldPrice = "999,99 EUR";
					model.ProductPrice.Price = "55,11 EUR";
					model.ProductPrice.PriceValue = 55.11M;
				}
			}
		}
		
		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
			Debug.WriteLine("OnResultExecuted");
		}

	}
}
