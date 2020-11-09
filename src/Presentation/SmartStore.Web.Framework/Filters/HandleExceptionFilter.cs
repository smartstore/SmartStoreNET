using System;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Logging;

namespace SmartStore.Web.Framework.Filters
{
    public class HandleExceptionFilter : IActionFilter, IExceptionFilter
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly Lazy<IWorkContext> _workContext;

        public HandleExceptionFilter(ILoggerFactory loggerFactory, Lazy<IWorkContext> workContext)
        {
            _loggerFactory = loggerFactory;
            _workContext = workContext;
        }

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
                return;

            if (filterContext.Result is HttpStatusCodeResult statusCodeResult && statusCodeResult.StatusCode == 404)
            {
                // Handle not found (404) from within the MVC pipeline (only called when HttpNotFoundResult is returned from actions)
                filterContext.Result = Create404Result(filterContext);

                _workContext.Value.IsAdmin = false;

            }
        }

        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
                return;

            var exception = filterContext.Exception;

            if (!exception.IsFatal())
            {
                LogException(exception, filterContext);
            }

            if (!filterContext.IsChildAction)
            {
                var controllerName = filterContext.RouteData.GetRequiredString("controller");
                var actionName = filterContext.RouteData.GetRequiredString("action");

                // if the request is AJAX return JSON data
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            error = true,
                            controller = controllerName,
                            action = actionName,
                            message = exception.Message
                        }
                    };
                }
                else
                {
                    filterContext.Result = new ViewResult
                    {
                        ViewName = "Error",
                        MasterName = (string)null,
                        ViewData = new ViewDataDictionary<HandleErrorInfo>(new HandleErrorInfo(exception, controllerName, actionName)),
                        TempData = filterContext.Controller.TempData
                    };

                    _workContext.Value.IsAdmin = false;
                }

                filterContext.ExceptionHandled = true;

                var response = filterContext.RequestContext.HttpContext.Response;

                response.Clear();
                response.StatusCode = 500;
                // Truncate message to avoid ArgumentOutOfRangeException.
                response.StatusDescription = exception.Message?.Truncate(512);
                response.TrySkipIisCustomErrors = true;
            }
        }

        protected internal static ActionResult Create404Result(ControllerContext context)
        {
            var controllerName = context.RouteData.GetRequiredString("controller");
            var actionName = context.RouteData.GetRequiredString("action");

            var response = context.RequestContext.HttpContext.Response;

            //response.Clear();
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.TrySkipIisCustomErrors = true;

            return new ViewResult
            {
                ViewName = "NotFound",
                MasterName = (string)null,
                ViewData = new ViewDataDictionary<HandleErrorInfo>(new HandleErrorInfo(new HttpException(404, "The resource does not exist."), actionName, controllerName)),
                TempData = context.Controller.TempData
            };
        }

        protected void LogException(Exception exception, ControllerContext context)
        {
            if (exception == null)
                return;

            if (!DataSettings.DatabaseIsInstalled())
                return;

            //// ignore 404 HTTP errors
            //var httpException = exception as HttpException;
            //if (httpException != null && httpException.GetHttpCode() == 404)
            //	return;

            try
            {
                var logger = _loggerFactory.GetLogger(context.Controller.GetType());
                logger.Error(exception);
            }
            catch
            {
                // don't throw new exception
            }
        }
    }

}
