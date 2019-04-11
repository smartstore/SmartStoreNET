using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
    public class UserMenuFilter : IActionFilter, IResultFilter
    {
        private readonly IMenuService _menuService;

        public UserMenuFilter(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext.IsChildAction || !filterContext.Result.IsHtmlViewResult())
            {
                return;
            }

            _menuService.ProcessMenus();
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}
