using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core;

namespace SmartStore.Web.Framework.UI
{

    public class PagerBuilder : NavigatableComponentBuilder<Pager, PagerBuilder>
    {

        public PagerBuilder(Pager component, HtmlHelper htmlHelper)
            : base(component, htmlHelper)
        {
        }

        public PagerBuilder Model(IPageable value)
        {
            base.Component.Model = value;
            return this;
        }

        public PagerBuilder Alignment(PagerAlignment value)
        {
            base.Component.Alignment = value;
            return this;
        }

        public PagerBuilder Size(PagerSize value)
        {
            base.Component.Size = value;
            return this;
        }

        public PagerBuilder Style(PagerStyle value)
        {
            base.Component.Style = value;
            return this;
        }

        public PagerBuilder ShowFirst(bool value)
        {
            base.Component.ShowFirst = value;
            return this;
        }

        public PagerBuilder ShowLast(bool value)
        {
            base.Component.ShowLast = value;
            return this;
        }

        public PagerBuilder ShowNext(bool value)
        {
            base.Component.ShowNext = value;
            return this;
        }

        public PagerBuilder ShowPrevious(bool value)
        {
            base.Component.ShowPrevious = value;
            return this;
        }

        public PagerBuilder ShowSummary(bool value)
        {
            base.Component.ShowSummary = value;
            return this;
        }

        public PagerBuilder ShowPaginator(bool value)
        {
            base.Component.ShowPaginator = value;
            return this;
        }

        public PagerBuilder MaxPagesToDisplay(int value)
        {
            base.Component.MaxPagesToDisplay = value;
            return this;
        }

        public PagerBuilder SkipActiveState(bool value)
        {
            base.Component.SkipActiveState = value;
            return this;
        }

        public PagerBuilder FirstButtonText(string value)
        {
            base.Component.FirstButtonText = value;
            return this;
        }

        public PagerBuilder LastButtonText(string value)
        {
            base.Component.LastButtonText = value;
            return this;
        }

        public PagerBuilder NextButtonText(string value)
        {
            base.Component.NextButtonText = value;
            return this;
        }

        public PagerBuilder PreviousButtonText(string value)
        {
            base.Component.PreviousButtonText = value;
            return this;
        }

        public PagerBuilder CurrentPageText(string value)
        {
            base.Component.CurrentPageText = value;
            return this;
        }

        public PagerBuilder ItemTitleFormatString(string value)
        {
            base.Component.ItemTitleFormatString = value;
            return this;
        }

    }

}
