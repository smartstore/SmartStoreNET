using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SmartStore.Utilities;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{

    public class TabBuilder : NavigationItemtWithContentBuilder<Tab, TabBuilder>
    {

        public TabBuilder(Tab item)
            : base(item)
        {
        }

        public TabBuilder Name(string value)
        {
            this.Item.Name = value;
            return this;
        }

        public TabBuilder Pull(TabPull value)
        {
            this.Item.Pull = value;
            return this;
        }


    }

}
