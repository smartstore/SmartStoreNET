using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{

    public static class HtmlHelperExtensions
    {

        public static ComponentFactory SmartStore(this HtmlHelper helper)
        {
            return new ComponentFactory(helper);
        }

    }

}
