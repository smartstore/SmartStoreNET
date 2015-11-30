using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Services.Localization;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.UI
{
   
    public class PagerRenderer : ComponentRenderer<Pager>
    {

        public PagerRenderer()
        {
        }

        protected List<PagerItem> CreateItemList()
        {
            var pager = base.Component;

            if (!pager.ShowPaginator || pager.Model.TotalPages <= 1)
                return new List<PagerItem>();

            var pageNumber = pager.Model.PageNumber;
            var pageCount = pager.Model.TotalPages;

            var results = new List<PagerItem>();

            PagerItem item;

            // first link
            if (pager.ShowFirst && pageNumber > 1)
            {
                item = new PagerItem(pager.FirstButtonText, GenerateUrl(1), PagerItemType.FirstPage);
                item.State = (pageNumber > 1) ? PagerItemState.Normal : PagerItemState.Disabled;
                results.Add(item);
            }

            // previous link
            if (pager.ShowPrevious && pageNumber > 1)
            {
                item = new PagerItem(pager.PreviousButtonText, GenerateUrl(pageNumber - 1), PagerItemType.PreviousPage);
                item.State = (pageNumber > 1) ? PagerItemState.Normal : PagerItemState.Disabled;
                results.Add(item);
            }

            // add the page number items
            if (pager.MaxPagesToDisplay > 0)
            {
                this.AddPageItemsList(results);
            }

            // next link
            var hasNext = false;
            if (pager.ShowNext && pageNumber < pageCount)
            {
                item = new PagerItem(pager.NextButtonText, GenerateUrl(pageNumber + 1), PagerItemType.NextPage);
                item.State = (pageNumber == pageCount) ? PagerItemState.Disabled : PagerItemState.Normal;
                results.Add(item);
                hasNext = true;
            }

            // last link
            if (pager.ShowLast && pageNumber < pageCount)
            {
                item = new PagerItem(pager.LastButtonText, GenerateUrl(pageCount), PagerItemType.LastPage);
                item.State = (pageNumber == pageCount) ? PagerItemState.Disabled : PagerItemState.Normal;
                if (pager.Style == PagerStyle.Pagination || !hasNext)
                {
                    results.Add(item);
                }
                else
                {
                    // BlogStyle Last-Item is right-aligned, so shift left
                    results.Insert(results.Count - 1, item);
                }
            }

            return results;
        }

        /// <summary>
        /// Can be overridden in a custom renderer in order to apply
        /// a custom numbering sequence.
        /// </summary>
        /// <param name="items"></param>
        protected virtual void AddPageItemsList(List<PagerItem> items)
        {
            var pager = base.Component;
            var pageNumber = pager.Model.PageNumber;
            var pageCount = pager.Model.TotalPages;

            int start = this.GetFirstPageIndex() + 1;
            int end = this.GetLastPageIndex() + 1;

            if (start > 3 && !pager.ShowFirst)
            {
                items.Add(new PagerItem("1", GenerateUrl(1)));
                items.Add(new PagerItem("...", "", PagerItemType.Text));
            }

            for (int i = start; i <= end; i++)
            {
                var item = new PagerItem(i.ToString(), GenerateUrl(i));
                if (i == pageNumber && !pager.SkipActiveState)
                {
                    item.State = PagerItemState.Selected;
                }
                items.Add(item);
            }

            if (end < (pageCount - 3) && !pager.ShowLast)
            {
                items.Add(new PagerItem("...", "", PagerItemType.Text));
                items.Add(new PagerItem(pageCount.ToString(), GenerateUrl(pageCount)));
            }
        }

        /// <summary>
        /// Gets first individual page index
        /// </summary>
        /// <returns>Page index</returns>
        protected virtual int GetFirstPageIndex()
        {
            var c = base.Component;

            if ((c.Model.TotalPages < c.MaxPagesToDisplay) ||
                ((c.Model.PageIndex - (c.MaxPagesToDisplay / 2)) < 0))
            {
                return 0;
            }
            if ((c.Model.PageIndex + (c.MaxPagesToDisplay / 2)) >= c.Model.TotalPages)
            {
                return (c.Model.TotalPages - c.MaxPagesToDisplay);
            }
            return (c.Model.PageIndex - (c.MaxPagesToDisplay / 2));
        }

        /// <summary>
        /// Get last individual page index
        /// </summary>
        /// <returns>Page index</returns>
        protected virtual int GetLastPageIndex()
        {
            var c = base.Component;

            int num = c.MaxPagesToDisplay / 2;
            if ((c.MaxPagesToDisplay % 2) == 0)
            {
                num--;
            }
            if ((c.Model.TotalPages < c.MaxPagesToDisplay) ||
                ((c.Model.PageIndex + num) >= c.Model.TotalPages))
            {
                return (c.Model.TotalPages - 1);
            }
            if ((c.Model.PageIndex - (c.MaxPagesToDisplay / 2)) < 0)
            {
                return (c.MaxPagesToDisplay - 1);
            }
            return (c.Model.PageIndex + num);
        }

        #region Other renderers

        ///// <summary>
        ///// Can be overridden in a custom renderer in order to apply
        ///// a custom numbering sequence.
        ///// </summary>
        ///// <param name="items"></param>
        //protected virtual void AddPageItemsList3(List<PagerItem> items)
        //{
        //    var pager = base.Component;
        //    var pageNumber = pager.Model.PageNumber;
        //    var pageCount = pager.Model.TotalPages;

        //    int start = 1;
        //    int end = pageCount;

        //    if (pageCount > pager.MaxPagesToDisplay)
        //    {
        //        int middle = (int)Math.Ceiling(pager.MaxPagesToDisplay / 2d) - 1; // 5
        //        int below = (pageNumber - middle); // 8-5=3
        //        int above = (pageNumber + middle); // 8+5=13

        //        if (below < 4)
        //        {
        //            above = pager.MaxPagesToDisplay; // 12
        //            below = 1;
        //        }
        //        else if (above > (pageCount - 4))
        //        {
        //            above = pageCount; // 20
        //            below = (pageCount - pager.MaxPagesToDisplay); // 8
        //        }

        //        start = below; // 1
        //        end = above; // 12
        //    }

        //    if (start > 3)
        //    {
        //        items.Add(new PagerItem("1", GenerateUrl(1)));
        //        items.Add(new PagerItem("2", GenerateUrl(2)));
        //        items.Add(new PagerItem("...", "", PagerItemType.Text));
        //    }

        //    for (int i = start; i <= end; i++)
        //    {
        //        var item = new PagerItem(i.ToString(), GenerateUrl(i));
        //        if (i == pageNumber)
        //        {
        //            item.State = PagerItemState.Selected;
        //        }
        //        items.Add(item);
        //    }

        //    if (end < (pageCount - 3))
        //    {
        //        items.Add(new PagerItem("...", "", PagerItemType.Text));
        //        items.Add(new PagerItem((pageCount - 1).ToString(), GenerateUrl(pageCount - 1)));
        //        items.Add(new PagerItem(pageCount.ToString(), GenerateUrl(pageCount)));
        //    }
        //}

        ///// <summary>
        ///// Can be overridden in a custom renderer in order to apply
        ///// a custom numbering sequence.
        ///// </summary>
        ///// <param name="items"></param>
        //protected virtual void AddPageItemsList2(List<PagerItem> items)
        //{
        //    var pager = base.Component;
        //    var pageNumber = pager.Model.PageNumber;
        //    var pageCount = pager.Model.TotalPages;

        //    List<PagerItem> newList = new List<PagerItem>();

        //    int p = 0;
        //    int q = 0;

        //    // the total magnitude ( e.g. 3 for 1.300 entries [ 10^3 = 1.000 ]
        //    int m = (int)Math.Log10(pageCount);

        //    // first item is always the selected currentIndex
        //    newList.Add(new PagerItem(pageNumber.ToString(), GenerateUrl(pageNumber), PagerItemType.Page, PagerItemState.Selected));

        //    // -2, -1, ..., +1, +2 from the current index
        //    for (int i = 1; i <= 3; i++)
        //    {
        //        p = pageNumber + i;
        //        if (p <= pageCount)
        //            newList.Add(new PagerItem(p.ToString(), GenerateUrl(p)));

        //        p = pageNumber - i;
        //        if (p > 0)
        //            newList.Insert(0, new PagerItem(p.ToString(), GenerateUrl(p)));
        //    }

        //    // ... -1.000, -500, -100, -50, -10, ..., +10, +50, +100, +500, +1.000 ..
        //    for (var i = 1; i <= m; i++)
        //    {
        //        q = (int)Math.Pow(10, i);
        //        if (i > 1)
        //        {
        //            // if > 10, do halfs .. (50, 500, 5000)
        //            p = pageNumber + q / 2;
        //            if (p < pageCount)
        //                newList.Add(new PagerItem(p.ToString(), GenerateUrl(p)));

        //            p = pageNumber - q / 2;
        //            if (p > 1)
        //                newList.Insert(0, new PagerItem(p.ToString(), GenerateUrl(p)));
        //        }

        //        // do full counts (10, 100, 1.000)
        //        p = pageNumber + q;
        //        if (p < pageCount)
        //            newList.Add(new PagerItem(p.ToString(), GenerateUrl(p)));

        //        p = pageNumber - q;
        //        if (p > 1)
        //            newList.Insert(0, new PagerItem(p.ToString(), GenerateUrl(p)));
        //    }

        //    items.AddRange(newList);
        //}

        #endregion


        protected virtual string GenerateUrl(int pageNumber)
        {
            var pager = base.Component;
            // paramName is "page" by default, but could also be "pagenumber"
            pager.ModifyParam(pageNumber);

            return pager.GenerateUrl(this.ViewContext.RequestContext);
        }

        protected override void WriteHtmlCore(HtmlTextWriter writer)
        {
            var pager = base.Component;
            if (pager.Model.TotalPages <= 1)
                return; // one page is no page.

            var items = CreateItemList();

            AppendCssClass(pager.HtmlAttributes, pager.Style == PagerStyle.Pagination ? "pagination" : "pager");
            // Size
            if (pager.Size != PagerSize.Medium)
            {
                AppendCssClass(pager.HtmlAttributes, "pagination-{0}".FormatInvariant(pager.Size.ToString().ToLower()));
            }
            // Alignment
            if (pager.Alignment != PagerAlignment.Left)
            {
                AppendCssClass(pager.HtmlAttributes, "pagination-{0}".FormatInvariant(pager.Alignment.ToString().ToLower()));
            }
            writer.AddAttributes(pager.HtmlAttributes);
            //writer.AddAttribute("id", pager.Id);
            writer.RenderBeginTag("div");

            if (pager.ShowSummary && pager.Model.TotalPages > 1)
            {
                writer.AddAttribute("class", "pagination-summary pull-left");
                writer.RenderBeginTag("div");
                writer.WriteEncodedText(pager.CurrentPageText.FormatInvariant(pager.Model.PageNumber, pager.Model.TotalPages, pager.Model.TotalCount));
                writer.RenderEndTag(); // div
            }

            writer.AddAttribute("class", "unstyled");
            writer.RenderBeginTag("ul");

            foreach (var item in items)
            {
                this.RenderItem(writer, item); // create li
            }

            writer.RenderEndTag(); // ul
            writer.RenderEndTag(); // div
            
        }

        protected virtual void RenderItem(HtmlTextWriter writer, PagerItem item)
        {
            var attrs = new RouteValueDictionary();

            if (item.State == PagerItemState.Disabled)
            {
                //writer.AddAttribute("class", "disabled");
                AppendCssClass(attrs, "disabled");
            }
            else if (item.State == PagerItemState.Selected)
            {
                //writer.AddAttribute("class", "active");
                AppendCssClass(attrs, "active");
            }

            if (item.Type == PagerItemType.Text)
            {
                AppendCssClass(attrs, "shrinked");
            }

            if (Component.Style == PagerStyle.Blog && item.IsNavButton)
            {
                AppendCssClass(attrs, (item.Type == PagerItemType.PreviousPage || item.Type == PagerItemType.FirstPage) ? "previous" : "next");
            }

            writer.AddAttributes(attrs);
            writer.RenderBeginTag("li");

            if (item.Type == PagerItemType.Page || item.IsNavButton)
            {
                // write anchor
                writer.AddAttribute("href", item.Url);
                if (item.IsNavButton)
                {
                    writer.AddAttribute("title", item.Text.AttributeEncode());
                    if (Component.Style != PagerStyle.Blog)
                    {
                        writer.AddAttribute("rel", "tooltip");
                        writer.AddAttribute("class", "pager-nav");
                    }
                }
                else
                {
                    var formatStr = Component.ItemTitleFormatString;
                    if (!string.IsNullOrEmpty(formatStr))
                    {
                        writer.AddAttribute("title", string.Format(formatStr, item.Text).AttributeEncode());
                        writer.AddAttribute("rel", "tooltip");
                    }
                }
                writer.RenderBeginTag("a");
            }
            else
            {
                // write span
                writer.RenderBeginTag("span");
            }

            this.RenderItemInnerContent(writer, item);

            writer.RenderEndTag(); // a || span

            writer.RenderEndTag(); // li
        }

        protected virtual void RenderItemInnerContent(HtmlTextWriter writer, PagerItem item)
        {
            var type = item.Type;

            switch (type)
            {
                case PagerItemType.FirstPage:
                    writer.AddAttribute("class", "fa fa-step-backward");
                    break;
                case PagerItemType.PreviousPage:
					writer.AddAttribute("class", "fa fa-chevron-left");
                    break;
                case PagerItemType.NextPage:
					writer.AddAttribute("class", "fa fa-chevron-right");
                    break;
                case PagerItemType.LastPage:
					writer.AddAttribute("class", "fa fa-step-forward");
                    break;
                default:
                    writer.WriteEncodedText(item.Text);
                    break;
            }

            bool isPrevButton = item.Type == PagerItemType.FirstPage || item.Type == PagerItemType.PreviousPage;

            if (item.IsNavButton)
            {
                if (Component.Style == PagerStyle.Blog && !isPrevButton)
                {
                    writer.WriteEncodedText(item.Text + writer.NewLine);
                }
                // appropriate classes were added above
                writer.RenderBeginTag("i");
                writer.RenderEndTag();
                if (Component.Style == PagerStyle.Blog && isPrevButton)
                {
                    writer.WriteEncodedText(writer.NewLine + item.Text);
                }
            }
        }

        // TODO: (mc) Public util machen
        private void AppendCssClass(IDictionary<string, object> attributes, string @class)
        {
            attributes.AppendInValue("class", " ", @class);
        }

        // TODO: (mc) Public util machen
        private void PrependCssClass(IDictionary<string, object> attributes, string @class)
        {
            attributes.PrependInValue("class", " ", @class);
        }


    }

}
