using System;
using System.Web.Mvc;
using SmartStore.Core.Domain.Media;
using SmartStore.Services.Media;

namespace SmartStore.Web.Framework.Filters
{
    /// <summary>
    /// Filters a request by the size of uploaded files according to <see cref="MediaSettings.MaxUploadFileSize"/>.
    /// </summary>
    public class MaxMediaFileSizeAttribute : FilterAttribute, IActionFilter
    {
        public Lazy<MediaSettings> MediaSettings { get; set; }
        public Lazy<MediaExceptionFactory> ExceptionFactory { get; set; }

        public virtual void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext?.HttpContext?.Request == null || filterContext.IsChildAction)
            {
                return;
            }

            var request = filterContext.HttpContext.Request;
            if (request.Files.Count <= 0)
            {
                return;
            }

            long maxBytes = 1024 * MediaSettings.Value.MaxUploadFileSize;
            for (var i = 0; i < request.Files.Count; ++i)
            {
                var file = request.Files[i];
                if (file.ContentLength > maxBytes)
                {
                    throw ExceptionFactory.Value.MaxFileSizeExceeded(file.FileName, file.ContentLength, maxBytes);
                }
            }
        }

        public virtual void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}
