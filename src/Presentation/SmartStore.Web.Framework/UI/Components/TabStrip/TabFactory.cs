using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
    public class TabFactory : IHideObjectMembers
    {
        private readonly IList<Tab> _items;
        private readonly HtmlHelper _htmlHelper;

        public TabFactory(IList<Tab> items, HtmlHelper htmlHelper)
        {
            Guard.NotNull(htmlHelper, nameof(htmlHelper));

            _items = items;
            _htmlHelper = htmlHelper;
        }

        public virtual TabBuilder Add()
        {
            var item = new Tab();
            _items.Add(item);
            return new TabBuilder(item, _htmlHelper);
        }

        public virtual TabBuilder Insert(int index)
        {
            var item = new Tab();

            if (_items.Count > index)
            {
                _items.Insert(index, item);
            }
            else
            {
                _items.Add(item);
            }

            return new TabBuilder(item, _htmlHelper);
        }
    }
}
