using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
   
    public class TabStripRenderer : ComponentRenderer<TabStrip>
    {

        public TabStripRenderer()
        { 
        }

        protected override void WriteHtmlCore(HtmlTextWriter writer)
        {
            var tab = base.Component;
            var hasContent = tab.Items.Any(x => x.Content != null);
            var isTabbable = tab.Position != TabsPosition.Top;

            tab.HtmlAttributes.AppendCssClass("tabbable");

            if (isTabbable)
            {
                tab.HtmlAttributes.AppendCssClass("tabs-{0}".FormatInvariant(tab.Position.ToString().ToLower()));
            }

            writer.AddAttributes(tab.HtmlAttributes);
            writer.RenderBeginTag("div"); // root div
            
            if (tab.SmartTabSelection)
            {
                this.WriteSelectionScript(writer);
            }

            if (tab.Position == TabsPosition.Below && hasContent)
                RenderTabContent(writer);

            // Tabs
            var ulAttrs = new Dictionary<string, object>();
            ulAttrs.AppendCssClass("nav nav-{0}".FormatInvariant(tab.Style.ToString().ToLower()));
            if (tab.Stacked)
            {
                ulAttrs.AppendCssClass("nav-stacked");
            }
            writer.AddAttributes(ulAttrs);
            writer.RenderBeginTag("ul");

            int i = 1;
            foreach (var item in tab.Items)
            {
                this.RenderItemLink(writer, item, i);
                i++;
            }

            writer.RenderEndTag(); // ul

            if (tab.Position != TabsPosition.Below && hasContent)
                RenderTabContent(writer);

            writer.RenderEndTag(); // div.tabbable
            
        }

        protected virtual void WriteSelectionScript(HtmlTextWriter writer)
        {
            var onReady = "<script>$(function() {{ {0} }});</script>";
            var script = "TabSelector.selectTab('#{0}'); $('#{0} > ul.nav a[data-toggle=tab]').on('shown', TabSelector.shownHandler);".FormatInvariant(this.Component.Id);

            writer.Write("<input type='hidden' id='selectedTab' name='selectedTab' value='{0}' />".FormatInvariant(base.ViewContext.ViewData["SelectedTab"]));

            writer.Write(onReady.FormatInvariant(script));
        }

        protected virtual void RenderTabContent(HtmlTextWriter writer)
        {
            var tab = base.Component;
            
            writer.AddAttribute("class", "tab-content");
            writer.RenderBeginTag("div");
            int i = 1;
            foreach (var item in tab.Items)
            {
                this.RenderItemContent(writer, item, i);
                i++;
            }
            writer.RenderEndTag(); // div
        }

        protected virtual void RenderItemLink(HtmlTextWriter writer, Tab item, int index)
        {
            string temp = "";

            // <li [class="active [hide]"]><a href="#{id}" data-toggle="tab">{text}</a></li>
            if (item.Selected)
            {
                item.HtmlAttributes.AppendCssClass("active");
            }
            else
            {
                if (!item.Visible)
                {
                    item.HtmlAttributes.AppendCssClass("hide");
                }
            }
            
            if (item.Pull == TabPull.Right)
            {
                item.HtmlAttributes.AppendCssClass("pull-right");
            }

            writer.AddAttributes(item.HtmlAttributes);

            writer.RenderBeginTag("li");
            

                writer.AddAttribute("href", "#" + BuildItemId(item, index));
                if (item.Content != null)
                {
                    writer.AddAttribute("data-toggle", "tab");
                }
                if (item.BadgeText.HasValue())
                {
                    item.LinkHtmlAttributes.AppendCssClass("clearfix");
                }
                writer.AddAttributes(item.LinkHtmlAttributes);
                writer.RenderBeginTag("a");
                
                // Tab Icon
                if (item.Icon.HasValue())
                {
                    writer.AddAttribute("class", item.Icon);
                    writer.RenderBeginTag("i");
                    writer.RenderEndTag(); // i
                }
                else if (item.ImageUrl.HasValue())
                {
                    var url = new UrlHelper(this.ViewContext.RequestContext);
                    writer.AddAttribute("src", url.Content(item.ImageUrl));
                    writer.AddAttribute("alt", "Icon");
                    writer.RenderBeginTag("img");
                    writer.RenderEndTag(); // img
                }
                //writer.WriteEncodedText(item.Text);
                
                // Badge
                if (item.BadgeText.HasValue())
                {
                    //writer.Write("&nbsp;");

                    // caption
                    writer.AddAttribute("class", "tab-caption");
                    writer.RenderBeginTag("span");
                    writer.WriteEncodedText(item.Text);
                    writer.RenderEndTag(); // span > badge

                    // label
                    temp = "label";
                    if (item.BadgeStyle != BadgeStyle.Default)
                    {
                        temp += " label-" + item.BadgeStyle.ToString().ToLower();
                    }
                    if (base.Component.Position == TabsPosition.Left)
                    {
                        temp += " pull-right"; // looks nicer 
                    }
                    writer.AddAttribute("class", temp);
                    writer.RenderBeginTag("span");
                    writer.WriteEncodedText(item.BadgeText);
                    writer.RenderEndTag(); // span > badge
                }
                else
                {
                    writer.WriteEncodedText(item.Text);
                }

                writer.RenderEndTag(); // a

            writer.RenderEndTag(); // li
        }

        protected virtual void RenderItemContent(HtmlTextWriter writer, Tab item, int index)
        {
            // <div class="tab-pane fade in [active]" id="{id}">{content}</div>
            item.ContentHtmlAttributes.AppendCssClass("tab-pane");
            if (base.Component.Fade)
            {
                item.ContentHtmlAttributes.AppendCssClass("fade");
            }
            if (item.Selected)
            {
                if (base.Component.Fade)
                {
                    item.ContentHtmlAttributes.AppendCssClass("in");
                }
               item.ContentHtmlAttributes.AppendCssClass("active");
            }
            writer.AddAttributes(item.ContentHtmlAttributes);
            writer.AddAttribute("id", BuildItemId(item, index));
            writer.RenderBeginTag("div");
            if (item.Content != null)
            {
                writer.WriteLine(item.Content.ToHtmlString()); 
            }
            writer.RenderEndTag(); // div
        }

        private string BuildItemId(Tab item, int index)
        {
            if (item.Name.HasValue())
            {
                return item.Name;
            }
            return "{0}-{1}".FormatInvariant(this.Component.Id, index);
        }


    }

}
