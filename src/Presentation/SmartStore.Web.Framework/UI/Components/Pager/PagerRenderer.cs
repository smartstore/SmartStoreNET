using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.Routing;

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

			// Alignment
			if (pager.Alignment == PagerAlignment.Right)
			{
				pager.HtmlAttributes.AppendCssClass("text-right");
			}
			else if (pager.Alignment == PagerAlignment.Centered)
			{
				pager.HtmlAttributes.AppendCssClass("text-center");
			}

			writer.AddAttributes(pager.HtmlAttributes);
            writer.AddAttribute("id", pager.Id);
			writer.AddAttribute("aria-label", "Page navigation");
			writer.RenderBeginTag("nav");

            if (pager.ShowSummary && pager.Model.TotalPages > 1)
            {
                writer.AddAttribute("class", "pagination-summary float-left");
                writer.RenderBeginTag("div");
                writer.WriteEncodedText(pager.CurrentPageText.FormatInvariant(pager.Model.PageNumber, pager.Model.TotalPages, pager.Model.TotalCount));
                writer.RenderEndTag(); // div
            }

			var ulAttrs = new RouteValueDictionary();

			ulAttrs.AppendCssClass(pager.Style == PagerStyle.Pagination ? "pagination" : "pagination" /* formerly "pager" */); // TODO: (mc) Seems there's no "pager" in BS4 anymore

			// Size
			if (pager.Size == PagerSize.Large)
			{
				ulAttrs.AppendCssClass("pagination-lg");
			}
			else if (pager.Size == PagerSize.Small)
			{
				ulAttrs.AppendCssClass("pagination-sm");
			}
			else if (pager.Size == PagerSize.Mini)
			{
				ulAttrs.AppendCssClass("pagination-xs");
			}

			// BS 4 alignment
			if (pager.Alignment == PagerAlignment.Centered)
			{
				ulAttrs.AppendCssClass("justify-content-center");
			}
			else if (pager.Alignment == PagerAlignment.Right)
			{
				ulAttrs.AppendCssClass("justify-content-end");
			}

			writer.AddAttributes(ulAttrs);
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
				attrs.AppendCssClass("disabled");
            }
            else if (item.State == PagerItemState.Selected)
            {
				attrs.AppendCssClass("active");
            }

            if (item.Type == PagerItemType.Text)
            {
				attrs.AppendCssClass("shrinked");
            }

            if (Component.Style == PagerStyle.Blog && item.IsNavButton)
            {
				// TODO: (mg) BS4 doesn't seem to support this anymore
				attrs.AppendCssClass((item.Type == PagerItemType.PreviousPage || item.Type == PagerItemType.FirstPage) ? "previous" : "next");
            }

			attrs.AppendCssClass("page-item");
            
            writer.AddAttributes(attrs);
            writer.RenderBeginTag("li");

            if (item.Type == PagerItemType.Page || item.IsNavButton)
            {
                // write anchor
                writer.AddAttribute("href", item.Url);
                if (item.IsNavButton)
                {
                    writer.AddAttribute("title", item.Text.AttributeEncode());
					writer.AddAttribute("aria-label", item.Text.AttributeEncode());
					writer.AddAttribute("tab-index", "-1");
					if (Component.Style != PagerStyle.Blog)
                    {
                        writer.AddAttribute("rel", "tooltip");
                        writer.AddAttribute("class", "page-link page-nav");
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

                writer.AddAttribute("class", "page-link");

                writer.RenderBeginTag("a");
            }
            else
            {
				// write span
				writer.AddAttribute("class", "page-link");
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
                    writer.AddAttribute("class", "fa fa-angle-double-left");
                    break;
                case PagerItemType.PreviousPage:
					writer.AddAttribute("class", "fa fa-angle-left");
                    break;
                case PagerItemType.NextPage:
					writer.AddAttribute("class", "fa fa-angle-right");
                    break;
                case PagerItemType.LastPage:
					writer.AddAttribute("class", "fa fa-angle-double-right");
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
    }

}
