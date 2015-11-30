using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
    
    public class TabFactory : IHideObjectMembers
    {
        private readonly IList<Tab> _items;

        public TabFactory(IList<Tab> items)
        {
            _items = items;
        }

        public virtual TabBuilder Add()
        {
            var item = new Tab();
            _items.Add(item);
            return new TabBuilder(item);
        }


    }

}
