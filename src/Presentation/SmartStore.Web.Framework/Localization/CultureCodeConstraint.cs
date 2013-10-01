using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Web.Routing;

namespace SmartStore.Web.Framework.Localization
{
    
    /// <summary>
    /// A route constraint that only matches routes with certain culture codes.
    /// </summary>
    public class CultureCodeConstraint : IRouteConstraint  
    {
        private readonly HashSet<string> _allowedCultureCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public CultureCodeConstraint(params string[] allowedCultureCodes)
        {
            _allowedCultureCodes.AddRange(allowedCultureCodes);
        }
        
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            // Gets the culture code from route data.
            // 'parameterName' actually is 'cultureCode'
            string value = values[parameterName].ToString();

            // Return true if the list of allowed cultures contains requested value
            return value == "default" || _allowedCultureCodes.Contains(value);
        }

        internal HashSet<string> AllowedCultureCodes
        {
            get
            {
                return _allowedCultureCodes;
            }
        }
    }

}
