using System.Collections.Generic;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI
{
    public class ModifiedParameter
    {
        public ModifiedParameter() : this(null)
        {
        }
        public ModifiedParameter(string name)
        {
            this.Name = name;
            this.BooleanParamNames = new List<string>();
        }

        public bool HasValue()
        {
            return !this.Name.IsEmpty();
        }

        public string Name { get; set; }
        public object Value { get; set; }
        // little hack here due to ugly MVC implementation
        // find more info here: http://www.mindstorminteractive.com/blog/topics/jquery-fix-asp-net-mvc-checkbox-truefalse-value/
        public IList<string> BooleanParamNames { get; private set; }
    }

    public interface INavigatable
    {

        string ControllerName
        {
            get;
            set;
        }

        string ActionName
        {
            get;
            set;
        }

        string RouteName
        {
            get;
            set;
        }

        RouteValueDictionary RouteValues
        {
            get;
        }

        ModifiedParameter ModifiedParam
        {
            get;
        }

        string Url
        {
            get;
            set;
        }
    }
}
