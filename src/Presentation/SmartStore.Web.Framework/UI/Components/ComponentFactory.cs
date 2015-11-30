using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ComponentModel;
using SmartStore.Core;

namespace SmartStore.Web.Framework.UI
{
    
    public class ComponentFactory : IHideObjectMembers
    {

        public ComponentFactory(HtmlHelper helper)
        {
            this.HtmlHelper = helper;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public HtmlHelper HtmlHelper
        {
            get;
            set;
        }

        #region Builders

        public virtual TabStripBuilder TabStrip()
        {
            return new TabStripBuilder(new TabStrip(), this.HtmlHelper);
        }

        public virtual WindowBuilder Window()
        {
            return new WindowBuilder(new Window(), this.HtmlHelper);
        }

        public virtual PagerBuilder Pager(string viewDataKey)
        {
            var dataSource = this.HtmlHelper.ViewContext.ViewData.Eval(viewDataKey) as IPageable;

            if (dataSource == null)
            {
                throw new InvalidOperationException(string.Format("Item in ViewData with key '{0}' is not an IPageable.",
                                                                  viewDataKey));
            }

            return Pager(dataSource);
        }

        public virtual PagerBuilder Pager(int pageIndex, int pageSize, int totalItemsCount)
        {
            return Pager(new PagedList(pageIndex, pageSize, totalItemsCount));
        }

        public virtual PagerBuilder Pager(int pageCount)
        {
            // for simple pagers without active state (e.g. forum topic mini pager)
            return Pager(new PagedList(0, 1, pageCount));
        }

        public virtual PagerBuilder Pager(IPageable model)
        {
            Guard.ArgumentNotNull(model, "model");
            return new PagerBuilder(new Pager(), this.HtmlHelper).Model(model);
        }

        #endregion

    }

}
