using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Web.Framework.UI
{
    public class TabStripRenderer : ComponentRenderer<TabStrip>
    {
        public TabStripRenderer()
        {
        }

        [SuppressMessage("ReSharper", "Mvc.AreaNotResolved")]
        protected override void WriteHtmlCore(HtmlTextWriter writer)
        {
            var tab = base.Component;

            MoveSpecialTabToEnd(tab.Items);

            var hasContent = tab.Items.Any(x => x.Content != null || x.Ajax);
            var isTabbable = tab.Position != TabsPosition.Top;
            var isStacked = tab.Position == TabsPosition.Left || tab.Position == TabsPosition.Right;
            var urlHelper = new UrlHelper(this.ViewContext.RequestContext);

            if (tab.Items.Count == 0)
                return;

            tab.HtmlAttributes.AppendCssClass("tabbable");

            if (isTabbable)
            {
                tab.HtmlAttributes.AppendCssClass("tabs-{0}".FormatInvariant(tab.Position.ToString().ToLower()));
            }

            if (tab.SmartTabSelection)
            {
                tab.HtmlAttributes.AppendCssClass("tabs-autoselect");
                tab.HtmlAttributes.Add("data-tabselector-href", urlHelper.Action("SetSelectedTab", "Common", new { area = "admin" }));
            }

            if (isStacked)
            {
                tab.HtmlAttributes.AppendCssClass("row");
            }

            if (tab.OnAjaxBegin.HasValue())
            {
                tab.HtmlAttributes.Add("data-ajax-onbegin", tab.OnAjaxBegin);
            }

            if (tab.OnAjaxSuccess.HasValue())
            {
                tab.HtmlAttributes.Add("data-ajax-onsuccess", tab.OnAjaxSuccess);
            }

            if (tab.OnAjaxFailure.HasValue())
            {
                tab.HtmlAttributes.Add("data-ajax-onfailure", tab.OnAjaxFailure);
            }

            if (tab.OnAjaxComplete.HasValue())
            {
                tab.HtmlAttributes.Add("data-ajax-oncomplete", tab.OnAjaxComplete);
            }

            if (tab.IsResponsive)
            {
                tab.HtmlAttributes.AppendCssClass("nav-responsive");
                tab.HtmlAttributes.Add("data-breakpoint", tab.Breakpoint);
            }

            writer.AddAttributes(tab.HtmlAttributes);

            writer.RenderBeginTag("div"); // root div
            {
                if (tab.Position == TabsPosition.Below && hasContent)
                    RenderTabContent(writer);

                if (isStacked)
                {
                    writer.AddAttribute("class", "col-lg-auto nav-aside");
                    writer.RenderBeginTag("aside"); // opening left/right tabs col
                }

                // Tabs
                var ulAttrs = new Dictionary<string, object>();
                ulAttrs.AppendCssClass("nav");

                if (tab.Style == TabsStyle.Tabs)
                {
                    ulAttrs.AppendCssClass("nav-tabs");
                }
                else if (tab.Style == TabsStyle.Pills)
                {
                    ulAttrs.AppendCssClass("nav-pills");
                }
                else if (tab.Style == TabsStyle.Material)
                {
                    ulAttrs.AppendCssClass("nav-tabs nav-tabs-line");
                }

                if (tab.HideSingleItem && tab.Items.Count == 1)
                {
                    ulAttrs.AppendCssClass("d-none");
                }

                if (isStacked)
                {
                    ulAttrs.AppendCssClass("flex-row flex-lg-column");
                }
                writer.AddAttributes(ulAttrs);

                string selector = null;
                var loadedTabs = new List<string>();
                writer.RenderBeginTag("ul");
                {
                    // enable smart tab selection
                    if (tab.SmartTabSelection)
                    {
                        selector = TrySelectRememberedTab();
                    }

                    int i = 1;
                    foreach (var item in tab.Items)
                    {
                        var loadedTabName = this.RenderItemLink(writer, item, i);
                        if (loadedTabName.HasValue())
                        {
                            loadedTabs.Add(loadedTabName);
                        }
                        i++;
                    }

                    writer.RenderEndTag(); // ul
                }

                if (isStacked)
                {
                    writer.RenderEndTag(); // closing left/right tabs col
                }

                // TODO: (mc) render right positioned tabs also
                if (tab.Position != TabsPosition.Below && hasContent)
                {
                    if (isStacked)
                    {
                        writer.AddAttribute("class", "col-lg nav-content");
                        writer.RenderBeginTag("div"); // opening left/right content col
                    }

                    RenderTabContent(writer);

                    if (isStacked)
                    {
                        writer.RenderEndTag(); // closing left/right content col
                    }
                }

                if (selector != null)
                {
                    writer.WriteLine(
@"<script>
	$(function() {{
		_.delay(function() {{
			$(""{0}"").trigger(""show"");
		}}, 100);
	}})
</script>".FormatInvariant(selector));
                }

                if (loadedTabs.Count > 0)
                {
                    foreach (var tabName in loadedTabs)
                    {
                        writer.WriteLine("<input type='hidden' class='loaded-tab-name' name='LoadedTabs' value='{0}' />", tabName);
                    }
                }

                writer.RenderEndTag(); // div.tabbable

                if (tab.IsResponsive /* && tab.TabContentHeaderContent != null*/)
                {
                    writer.WriteLine(@"<script>$(function() {{ $('#{0}').responsiveNav(); }})</script>".FormatInvariant(tab.Id));
                }
            }
        }

        private void MoveSpecialTabToEnd(List<Tab> tabs)
        {
            var idx = tabs.FindIndex(x => x.Name == "tab-special-plugin-widgets");
            if (idx > -1 && idx < (tabs.Count - 1))
            {
                var tab = tabs[idx];
                tabs.RemoveAt(idx);
                tabs.Add(tab);
            }
        }

        // returns a query selector
        private string TrySelectRememberedTab()
        {
            var tab = this.Component;

            if (tab.Id.IsEmpty())
                return null;

            if (ViewContext.ViewData.Model is EntityModelBase model && model.Id == 0)
            {
                // it's a "create" operation: don't select
                return null;
            }

            var rememberedTab = (SelectedTabInfo)ViewContext.TempData["SelectedTab." + tab.Id];
            if (rememberedTab != null && rememberedTab.Path.Equals(ViewContext.HttpContext.Request.RawUrl, StringComparison.OrdinalIgnoreCase))
            {
                // get tab to select
                var tabToSelect = GetTabById(rememberedTab.TabId);

                if (tabToSelect != null)
                {
                    // unselect former selected tab(s)
                    tab.Items.Each(x => x.Selected = false);

                    // select the new tab
                    tabToSelect.Selected = true;

                    // persist again for the next request
                    ViewContext.TempData["SelectedTab." + tab.Id] = rememberedTab;

                    if (tabToSelect.Ajax && tabToSelect.Content == null)
                    {
                        return ".nav a[data-ajax-url][href='#{0}']".FormatInvariant(rememberedTab.TabId);
                    }
                }
            }

            return null;
        }

        private Tab GetTabById(string tabId)
        {
            int i = 1;
            foreach (var item in Component.Items)
            {
                var id = BuildItemId(item, i);
                if (id == tabId)
                {
                    if (!item.Visible || !item.Enabled)
                        break;

                    return item;
                }
                i++;
            }

            return null;
        }

        protected virtual void RenderTabContent(HtmlTextWriter writer)
        {
            var tab = base.Component;

            writer.AddAttribute("class", "tab-content");
            writer.RenderBeginTag("div");

            // Tab content header
            if (tab.IsResponsive && tab.TabContentHeaderContent != null)
            {
                //writer.WriteLine(item.Content.ToHtmlString());
                writer.AddAttribute("class", "tab-content-header");
                writer.RenderBeginTag("div");
                {
                    writer.WriteLine(tab.TabContentHeaderContent.ToHtmlString());
                }
                writer.RenderEndTag(); // div.tab-content-header
            }

            // All tab items
            int i = 1;
            foreach (var item in tab.Items)
            {
                this.RenderItemContent(writer, item, i);
                i++;
            }

            writer.RenderEndTag(); // div.tab-content
        }

        protected virtual string RenderItemLink(HtmlTextWriter writer, Tab item, int index)
        {
            string temp = "";
            string loadedTabName = null;

            // <li [class="active [hide]"]><a href="#{id}" data-toggle="tab">{text}</a></li>
            item.HtmlAttributes.AppendCssClass("nav-item"); // .active for BS2

            if (!item.Selected && !item.Visible)
            {
                item.HtmlAttributes.AppendCssClass("d-none");
            }

            if (item.Pull == TabPull.Right)
            {
                item.HtmlAttributes.AppendCssClass("float-right");
            }

            writer.AddAttributes(item.HtmlAttributes);

            writer.RenderBeginTag("li");
            {
                var itemId = "#" + BuildItemId(item, index);

                writer.AddAttribute("class", "nav-link" + (item.Selected ? " active" : ""));

                if (item.Content != null)
                {
                    writer.AddAttribute("href", itemId);
                    writer.AddAttribute("data-toggle", "tab");
                    writer.AddAttribute("data-loaded", "true");
                    loadedTabName = GetTabName(item) ?? itemId;
                }
                else
                {
                    // no content, create real link instead
                    var url = item.GenerateUrl(base.ViewContext.RequestContext).NullEmpty();

                    if (url == null)
                    {
                        writer.AddAttribute("href", "#");
                    }
                    else
                    {
                        if (item.Ajax)
                        {
                            writer.AddAttribute("href", itemId);
                            writer.AddAttribute("data-ajax-url", url);
                            writer.AddAttribute("data-toggle", "tab");
                        }
                        else
                        {
                            writer.AddAttribute("href", url);
                        }
                    }
                }

                if (item.BadgeText.HasValue())
                {
                    item.LinkHtmlAttributes.AppendCssClass("clearfix");
                }

                writer.AddAttributes(item.LinkHtmlAttributes);
                writer.RenderBeginTag("a");
                {
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

                    // Caption
                    writer.AddAttribute("class", "tab-caption");
                    writer.RenderBeginTag("span");
                    writer.WriteEncodedText(item.Text);
                    writer.RenderEndTag();

                    // Badge
                    if (item.BadgeText.HasValue())
                    {
                        // Label/badge
                        temp = "ml-2 badge";
                        temp += " badge-" + item.BadgeStyle.ToString().ToLower();
                        if (base.Component.Position == TabsPosition.Left)
                        {
                            temp += " float-right"; // looks nicer 
                        }
                        writer.AddAttribute("class", temp);
                        writer.RenderBeginTag("span");
                        writer.WriteEncodedText(item.BadgeText);
                        writer.RenderEndTag(); // span > badge
                    }

                    // Nav link short summary for collapsed state
                    if (this.Component.IsResponsive && item.Summary.HasValue())
                    {
                        writer.AddAttribute("class", "nav-link-summary");
                        writer.RenderBeginTag("span");
                        writer.WriteEncodedText(item.Summary);
                        writer.RenderEndTag();
                    }

                    writer.RenderEndTag(); // a
                }
                writer.RenderEndTag(); // li
            }

            return loadedTabName;
        }

        private string GetTabName(Tab tab)
        {
            if (tab.LinkHtmlAttributes.TryGetValue("data-tab-name", out object value))
            {
                return value.ToString();
            }
            return null;
        }

        protected virtual void RenderItemContent(HtmlTextWriter writer, Tab item, int index)
        {
            // <div class="tab-pane fade in [active]" id="{id}">{content}</div>
            item.ContentHtmlAttributes.AppendCssClass("tab-pane");
            item.ContentHtmlAttributes.Add("role", "tabpanel");
            if (base.Component.Fade)
            {
                item.ContentHtmlAttributes.AppendCssClass("fade");
            }
            if (item.Selected)
            {
                if (base.Component.Fade)
                {
                    item.ContentHtmlAttributes.AppendCssClass("show");
                }
                item.ContentHtmlAttributes.AppendCssClass("active");
            }
            writer.AddAttributes(item.ContentHtmlAttributes);
            writer.AddAttribute("id", BuildItemId(item, index));
            writer.RenderBeginTag("div");
            {
                if (item.Content != null)
                {
                    writer.WriteLine(item.Content.ToHtmlString());
                }
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
