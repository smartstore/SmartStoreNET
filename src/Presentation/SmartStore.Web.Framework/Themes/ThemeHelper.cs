using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Web.Framework.Themes
{
    internal static class ThemeHelper
    {

        internal static bool PathListContainsThemeVars(IEnumerable<string> pathes)
        {
            Guard.ArgumentNotNull(() => pathes);

            return pathes.Any(x => PathIsThemeVars(x));
        }

        internal static bool PathIsThemeVars(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                return false;

            return virtualPath.ToLower().EndsWith("/.db/themevars.less");
        }

    }
}
