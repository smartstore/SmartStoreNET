using System.Web.Mvc;
using SmartStore.Services;
using SmartStore.Web.Models.Catalog;

namespace SmartStore.DevTools.Filters
{
    public class SampleProductDetailActionFilter : IActionFilter
    {
        private readonly ICommonServices _services;
        private readonly UrlHelper _urlHelper;

        public SampleProductDetailActionFilter(ICommonServices services, UrlHelper urlHelper)
        {
            _services = services;
            _urlHelper = urlHelper;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {

        }

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
            model.ActionItems["dev"] = new ProductDetailsModel.ActionItemModel
            {
                Key = "dev",
                Title = _services.Localization.GetResource("Dev"),
                Tooltip = _services.Localization.GetResource("Dev.Hint"),
                CssClass = "action-dev x-ajax-cart-link",
                IconCssClass = "icm icm-code",
                //Href = _urlHelper.Action("MyOwnAction", "MyPlugin", new { id = model.Id })
                Href = "https://www.smartstore.com"
            }; ;
        }
    }
}
