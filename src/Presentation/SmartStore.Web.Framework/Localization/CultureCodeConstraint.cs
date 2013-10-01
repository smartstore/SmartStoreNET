using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Web.Routing;
using SmartStore.Web.Framework.Seo;

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
        
        public virtual bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.IncomingRequest)
            {
                
                // Gets the culture code from route data.
                // 'parameterName' actually is 'cultureCode'
                string value = values[parameterName].ToString();

                // Return true if the list of allowed cultures contains requested value
                return _allowedCultureCodes.Contains(value);
            }

            return true;
        }

        internal HashSet<string> AllowedCultureCodes
        {
            get
            {
                return _allowedCultureCodes;
            }
        }

    }

    public class GenericPathCultureCodeConstraint : CultureCodeConstraint
    {
        public GenericPathCultureCodeConstraint(params string[] allowedCultureCodes) : base(allowedCultureCodes)
        {
        }

        public override bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            var genericRoute = route as GenericPathRoute;
            if (genericRoute != null)
            {
                object result = null;
                string seName = null;
                if (values.TryGetValue("generic_se_name", out result))
                {
                    seName = result as string;
                }

                if (seName.IsEmpty())
                {
                    values["generic_se_name"] = values[LocalizedRoute.CULTURECODE_TOKEN];
                    values[LocalizedRoute.CULTURECODE_TOKEN] = genericRoute.Defaults[LocalizedRoute.CULTURECODE_TOKEN];
                }
            }
            
            return base.Match(httpContext, route, parameterName, values, routeDirection);
        }
    }

}
