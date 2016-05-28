﻿using System.Web.Mvc;

namespace SmartStore.Web.Framework.Filters
{
    public class FormValueExistsAttribute : FilterAttribute, IActionFilter
    {
        private readonly string _name;
        private readonly string _value;
        private readonly string _actionParameterName;

        public FormValueExistsAttribute(string name, string value, string actionParameterName)
        {
            _name = name;
            _value = value;
            _actionParameterName = actionParameterName;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var formValue = filterContext.RequestContext.HttpContext.Request.Form[_name];
            filterContext.ActionParameters[_actionParameterName] = !string.IsNullOrEmpty(formValue) &&
                                                                   formValue.ToLower().Equals(_value.ToLower());
        }
    }
}
