using System;
using System.Web.Mvc;
using System.ComponentModel;
using SmartStore.Core;

namespace SmartStore.Web.Framework.UI
{  
    public class ComponentFactory<TModel> : IHideObjectMembers
    {
        public ComponentFactory(HtmlHelper<TModel> helper)
        {
            this.HtmlHelper = helper;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public HtmlHelper<TModel> HtmlHelper
        {
            get;
            set;
        }

        #region Builders

        public virtual TabStripBuilder<TModel> TabStrip()
        {
            return new TabStripBuilder<TModel>(new TabStrip(), this.HtmlHelper);
        }

        public virtual WindowBuilder<TModel> Window()
        {
            return new WindowBuilder<TModel>(new Window(), this.HtmlHelper);
        }

        public virtual PagerBuilder<TModel> Pager(string viewDataKey)
        {
            var dataSource = this.HtmlHelper.ViewContext.ViewData.Eval(viewDataKey) as IPageable;

            if (dataSource == null)
            {
                throw new InvalidOperationException(string.Format("Item in ViewData with key '{0}' is not an IPageable.", viewDataKey));
            }

            return Pager(dataSource);
        }

        public virtual PagerBuilder<TModel> Pager(int pageIndex, int pageSize, int totalItemsCount)
        {
            return Pager(new PagedList(pageIndex, pageSize, totalItemsCount));
        }

        public virtual PagerBuilder<TModel> Pager(int pageCount)
        {
            // for simple pagers without active state (e.g. forum topic mini pager)
            return Pager(new PagedList(0, 1, pageCount));
        }

        public virtual PagerBuilder<TModel> Pager(IPageable model)
        {
            Guard.NotNull(model, "model");
            return new PagerBuilder<TModel>(new Pager(), this.HtmlHelper).Model(model);
        }

		public virtual EntityPickerBuilder<TModel> EntityPicker()
		{
			return new EntityPickerBuilder<TModel>(new EntityPicker(), this.HtmlHelper);
		}

		public virtual FileUploaderBuilder<TModel> FileUploader()
		{
			return new FileUploaderBuilder<TModel>(new FileUploader(), this.HtmlHelper);
		}

		#endregion
	}
}
