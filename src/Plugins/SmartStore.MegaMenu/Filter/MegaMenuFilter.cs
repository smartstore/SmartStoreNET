using SmartStore.MegaMenu.Services;
using SmartStore.Web.Controllers;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartStore.MegaMenu.Filters
{
	public class MegaMenuFilter : IActionFilter
	{
		private readonly IMegaMenuService _megaMenuService;
        private readonly CatalogHelper _helper;

        public MegaMenuFilter(CatalogHelper helper /*IMegaMenuService megaMenuService*/)
		{
            //_megaMenuService = megaMenuService;
            _helper = helper;
        }
        
		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
		}

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext == null || filterContext.ActionDescriptor == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            //var routeValues = new RouteValueDictionary(new { action = actionName, controller = "AmazonPayCheckout" });

            //var model = _helper.PrepareCategoryNavigationModel(currentCategoryId, currentProductId);

            var model = _helper.PrepareCategoryNavigationModel(1, 0);

            filterContext.Controller.ViewData.Model = model;
            filterContext.Result = new PartialViewResult { ViewName = "~/Plugins/SmartStore.MegaMenu/Views/MegaMenu/MegaMenu.cshtml",
                ViewData = new ViewDataDictionary(model) };
            
            //var routeValues = new RouteValueDictionary(new { action = "MegaMenu", controller = "MegaMenu", area = "SmartStore.MegaMenu", model = model });
            //filterContext.Result = new RedirectToRouteResult("SmartStore.MegaMenu", routeValues);
        }
    }
}