using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Logging;

namespace SmartStore.Web.Framework.Filters
{
	public class HandleExceptionFilter : IActionFilter
    {
		private readonly ILoggerFactory _loggerFactory;
		private readonly Lazy<IEnumerable<IExceptionFilter>> _exceptionFilters;
		private readonly Lazy<IWorkContext> _workContext;

		public HandleExceptionFilter(
			ILoggerFactory loggerFactory, 
			Lazy<IEnumerable<IExceptionFilter>> exceptionFilters,
			Lazy<IWorkContext> workContext)
		{
			_loggerFactory = loggerFactory;
			_exceptionFilters = exceptionFilters;
			_workContext = workContext;
		}

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }
        
        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
			var descriptor = filterContext.ActionDescriptor;
			
			// handle server error
			// don't provide custom errors if the action has some custom code to handle exceptions
			if (!filterContext.ActionDescriptor.GetCustomAttributes(typeof(HandleErrorAttribute), false).Any())
			{
				if (!filterContext.ExceptionHandled && filterContext.Exception != null)
				{
					if (ShouldHandleException(filterContext.Exception))
					{
						LogException(filterContext.Exception, filterContext.ActionDescriptor.ControllerDescriptor);

						// inform exception filters of the exception that was suppressed
						var exceptionContext = new ExceptionContext(filterContext.Controller.ControllerContext, filterContext.Exception);
						foreach (var exceptionFilter in _exceptionFilters.Value)
						{
							exceptionFilter.OnException(exceptionContext);
						}

						if (exceptionContext.ExceptionHandled)
						{
							filterContext.ExceptionHandled = true;
							filterContext.Result = exceptionContext.Result;
						}
						else
						{
							if (!filterContext.IsChildAction)
							{
								var controllerName = descriptor.ControllerDescriptor.ControllerName;
								var actionName = descriptor.ActionName;

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
											message = filterContext.Exception.Message
										}
									};
								}
								else
								{
									filterContext.Result = new ViewResult
									{
										ViewName = "Error",
										MasterName = (string)null,
										ViewData = new ViewDataDictionary<HandleErrorInfo>(new HandleErrorInfo(filterContext.Exception, controllerName, actionName)),
										TempData = filterContext.Controller.TempData
									};
								}

								filterContext.ExceptionHandled = true;

								filterContext.RequestContext.HttpContext.Response.Clear();
								filterContext.RequestContext.HttpContext.Response.StatusCode = 500;

								// prevent IIS 7.0 classic mode from handling the 404/500 itself
								filterContext.RequestContext.HttpContext.Response.TrySkipIisCustomErrors = true;
							}
						}
					}
				}
			}

			if (filterContext.Result is HttpNotFoundResult && !filterContext.IsChildAction)
			{
				// handle not found (404) from within the MVC pipeline (only called when HttpNotFoundResult is returned from actions)
				var requestContext = filterContext.RequestContext;
				var url = requestContext.HttpContext.Request.RawUrl;

				filterContext.Result = new ViewResult
				{
					ViewName = "NotFound",
					MasterName = (string)null,
					ViewData = new ViewDataDictionary<HandleErrorInfo>(new HandleErrorInfo(new HttpException(404, "The resource does not exist."), descriptor.ActionName, descriptor.ControllerDescriptor.ControllerName)),
					TempData = filterContext.Controller.TempData
				};
				requestContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;

				// prevent IIS 7.0 classic mode from handling the 404/500 itself
				requestContext.HttpContext.Response.TrySkipIisCustomErrors = true;
			}
        }

		private bool ShouldHandleException(Exception exception)
		{
			return !(
				exception is StackOverflowException ||
				exception is OutOfMemoryException ||
				exception is AccessViolationException ||
				exception is AppDomainUnloadedException ||
				exception is ThreadAbortException ||
				exception is SecurityException ||
				exception is SEHException);
		}

		protected void LogException(Exception exception, ControllerDescriptor descriptor)
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
				var logger = _loggerFactory.GetLogger(descriptor.ControllerType);
				logger.Error(exception);
			}
			catch
			{
				// don't throw new exception
			}
		}

    }

}
