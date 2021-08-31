using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.ConfigurableExportTest.Filters
{
    public class SampleActionFilter : IActionFilter
    {
        /// <summary>
        /// Will be called before original action method is called
        /// </summary>
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Debug.WriteLine("OnActionExecuting");
        }

        /// <summary>
        /// Will be called after original action method was called
        /// </summary>
        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var result = filterContext.Result as ViewResultBase;
            if (result == null)
            {
                // The controller action didn't return a view result 
                // => no need to continue any further
                return;
            }

            var model = result.Model as ProductDetailsModel;
            if (model == null)
            {
                // there's no model or the model was not of the expected type 
                // => no need to continue any further
                return;
            }

            // modify some property value
            // model...
        }
    }
}
