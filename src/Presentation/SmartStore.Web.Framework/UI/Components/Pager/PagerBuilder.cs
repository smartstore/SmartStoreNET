using System.Web.Mvc;
using SmartStore.Core;

namespace SmartStore.Web.Framework.UI
{
    public class PagerBuilder<TModel> : NavigatableComponentBuilder<Pager, PagerBuilder<TModel>, TModel>
    {
        public PagerBuilder(Pager component, HtmlHelper<TModel> htmlHelper)
            : base(component, htmlHelper)
        {
        }

        public PagerBuilder<TModel> Model(IPageable value)
        {
            base.Component.Model = value;
            return this;
        }

        public PagerBuilder<TModel> Alignment(PagerAlignment value)
        {
            base.Component.Alignment = value;
            return this;
        }

        public PagerBuilder<TModel> Size(PagerSize value)
        {
            base.Component.Size = value;
            return this;
        }

        public PagerBuilder<TModel> Style(PagerStyle value)
        {
            base.Component.Style = value;
            return this;
        }

        public PagerBuilder<TModel> ShowFirst(bool value)
        {
            base.Component.ShowFirst = value;
            return this;
        }

        public PagerBuilder<TModel> ShowLast(bool value)
        {
            base.Component.ShowLast = value;
            return this;
        }

        public PagerBuilder<TModel> ShowNext(bool value)
        {
            base.Component.ShowNext = value;
            return this;
        }

        public PagerBuilder<TModel> ShowPrevious(bool value)
        {
            base.Component.ShowPrevious = value;
            return this;
        }

        public PagerBuilder<TModel> ShowSummary(bool value)
        {
            base.Component.ShowSummary = value;
            return this;
        }

        public PagerBuilder<TModel> ShowPaginator(bool value)
        {
            base.Component.ShowPaginator = value;
            return this;
        }

        public PagerBuilder<TModel> MaxPagesToDisplay(int value)
        {
            base.Component.MaxPagesToDisplay = value;
            return this;
        }

        public PagerBuilder<TModel> SkipActiveState(bool value)
        {
            base.Component.SkipActiveState = value;
            return this;
        }

        public PagerBuilder<TModel> FirstButtonText(string value)
        {
            base.Component.FirstButtonText = value;
            return this;
        }

        public PagerBuilder<TModel> LastButtonText(string value)
        {
            base.Component.LastButtonText = value;
            return this;
        }

        public PagerBuilder<TModel> NextButtonText(string value)
        {
            base.Component.NextButtonText = value;
            return this;
        }

        public PagerBuilder<TModel> PreviousButtonText(string value)
        {
            base.Component.PreviousButtonText = value;
            return this;
        }

        public PagerBuilder<TModel> CurrentPageText(string value)
        {
            base.Component.CurrentPageText = value;
            return this;
        }

        public PagerBuilder<TModel> ItemTitleFormatString(string value)
        {
            base.Component.ItemTitleFormatString = value;
            return this;
        }
    }
}
