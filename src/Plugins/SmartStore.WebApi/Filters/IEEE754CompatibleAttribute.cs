using System;
using System.Linq;
using System.Web.Http.Filters;

namespace SmartStore.WebApi
{
    /// <summary>
    /// https://github.com/smartstore/Smartstore/issues/429.
    /// https://stackoverflow.com/a/58844768.
    /// </summary>
    public class IEEE754CompatibleAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            var parameter = actionExecutedContext?.Request?.Headers?.Accept
                .SelectMany(h => h.Parameters.Where(p =>
                    p.Name.Equals("IEEE754Compatible", StringComparison.OrdinalIgnoreCase) &&
                    p.Value.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();

            if (parameter != null)
            {
                actionExecutedContext.Response.Content?.Headers.ContentType.Parameters.Add(parameter);
            }
        }
    }
}