using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.UI;

namespace SmartStore.Web.Framework.UI
{
    
    public class MobilePagerRenderer : PagerRenderer
    {

        // TODO: (mc) Public util machen
        private void AppendCssClass(IDictionary<string, object> attributes, string @class)
        {
            attributes.AppendInValue("class", " ", @class);
        }

        protected override void RenderItem(HtmlTextWriter writer, PagerItem item)
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


        protected override void RenderItemInnerContent(HtmlTextWriter writer, PagerItem item)
        {
            var type = item.Type;

            switch (type)
            {
                case PagerItemType.FirstPage:
                    writer.AddAttribute("class", "fa fa-step-backward");
                    break;
                case PagerItemType.PreviousPage:
                    writer.AddAttribute("class", "fa fa-chevron-left");
                    writer.WriteEncodedText(item.Text);
                    break;
                case PagerItemType.NextPage:
					writer.AddAttribute("class", "fa fa-chevron-right");
                    writer.WriteEncodedText(item.Text);
                    break;
                case PagerItemType.LastPage:
					writer.AddAttribute("class", "fa fa-step-forward");
                    break;
                default:
                    writer.WriteEncodedText(item.Text);
                    break;
            }
        }

    }
}
