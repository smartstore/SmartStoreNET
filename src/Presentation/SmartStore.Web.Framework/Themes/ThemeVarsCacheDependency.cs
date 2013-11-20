using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Caching;

namespace SmartStore.Web.Framework.Themes
{
    
    public class ThemeVarsCacheDependency : CacheDependency
    {
        private readonly int _storeId;

        public ThemeVarsCacheDependency(int storeId)
        {
            _storeId = storeId;
        }
    }

}
