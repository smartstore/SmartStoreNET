using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
   
    public class WindowRenderer : ComponentRenderer<Window>
    {

        protected override void WriteHtmlCore(HtmlTextWriter writer)
        {
            var win = base.Component;

            win.HtmlAttributes.AppendCssClass("modal");
            win.HtmlAttributes["role"] = "dialog";
            win.HtmlAttributes["tabindex"] = "-1";
            win.HtmlAttributes["aria-labelledby"] = win.Id + "Label";

            if (win.Width.GetValueOrDefault() > 0)
            {
                win.HtmlAttributes["style"] = "width:{0}px; margin-left:-{1}px".FormatInvariant(win.Width.Value, Math.Ceiling((double)(win.Width.Value / 2)));
            }

            if (!win.Visible)
            {
                win.HtmlAttributes["aria-hidden"] = "true";
                win.HtmlAttributes.AppendCssClass("hide");
            }
            else
            {
                win.HtmlAttributes["aria-hidden"] = "false";
            }

            if (win.Fade)
            {
                win.HtmlAttributes.AppendCssClass("fade");
                if (win.Visible)
                    win.HtmlAttributes.AppendCssClass("in");
            }

            // other options
            win.HtmlAttributes["data-backdrop"] = win.BackDrop.ToString().ToLower();
            win.HtmlAttributes["data-keyboard"] = win.CloseOnEscapePress.ToString().ToLower();
            //win.HtmlAttributes["data-show"] = win.Visible.ToString().ToLower();
            if (win.ContentUrl.HasValue())
            {
                win.HtmlAttributes["data-remote"] = win.ContentUrl;
            }

            writer.AddAttributes(win.HtmlAttributes);
            writer.RenderBeginTag("div"); // root div

            // HEADER
            if (win.ShowClose && win.Title.HasValue())
            {
                this.RenderHeader(writer);
            }

            // BODY
            this.RenderBody(writer);

            // FOOTER
            if (win.FooterContent != null)
            {
                this.RenderFooter(writer);
            }

            writer.RenderEndTag(); // div.modal
            
        }

        protected virtual void RenderHeader(HtmlTextWriter writer)
        {
            var win = base.Component;

            writer.AddAttribute("class", "modal-header");
            writer.RenderBeginTag("div");

            if (win.ShowClose)
            {
                writer.Write("<button type='button' class='close' data-dismiss='modal' aria-hidden='true'>×</button>");
            }

            if (win.Title.HasValue())
            {
                writer.Write("<h3 id='{0}'>{1}</h3>".FormatCurrent(win.Id + "Label", win.Title));
            }

            writer.RenderEndTag(); // div.modal-header
        }

        protected virtual void RenderBody(HtmlTextWriter writer)
        {
            var win = base.Component;

            writer.AddAttribute("class", "modal-body");
            if (win.Height.GetValueOrDefault() > 0)
            {
                writer.AddAttribute("style", "max-height:{0}px;".FormatInvariant(win.Height.Value));
            }
            writer.RenderBeginTag("div");

            if (win.ContentUrl.IsEmpty() && win.Content != null)
            {
                win.Content.WriteTo(writer);
            }

            writer.RenderEndTag(); // div.modal-body
        }

        protected virtual void RenderFooter(HtmlTextWriter writer)
        {
            var win = base.Component;

            writer.AddAttribute("class", "modal-footer");
            writer.RenderBeginTag("div");

            //writer.WriteLine(win.FooterContent.ToString());
            win.FooterContent.WriteTo(writer);

            writer.RenderEndTag(); // div.modal-body
        }


    }

}
