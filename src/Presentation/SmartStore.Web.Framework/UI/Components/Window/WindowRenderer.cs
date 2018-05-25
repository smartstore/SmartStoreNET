using System;
using System.Web.UI;

namespace SmartStore.Web.Framework.UI
{
	// TODO: (mc) make modal window renderer BS4 ready (after backend has been updated to BS4)
	public class WindowRenderer : ComponentRenderer<Window>
    {
		public override void Render()
		{
			if (Component.RenderAtPageEnd)
			{
				using (this.HtmlHelper.BeginZoneContent("end"))
				{
					base.Render();
				}
			}
			else
			{
				base.Render();
			}
		}

		protected override void WriteHtmlCore(HtmlTextWriter writer)
        {
            var win = base.Component;

            win.HtmlAttributes.AppendCssClass("modal");
			win.HtmlAttributes["role"] = "dialog";
			win.HtmlAttributes["tabindex"] = "-1";
			win.HtmlAttributes["aria-hidden"] = "true";
			win.HtmlAttributes["aria-labelledby"] = win.Id + "Label";

			if (win.Fade)
            {
                win.HtmlAttributes.AppendCssClass("fade");
            }

			// Other options
			win.HtmlAttributes["data-keyboard"] = win.CloseOnEscapePress.ToString().ToLower();
			win.HtmlAttributes["data-show"] = win.Show.ToString().ToLower();
			win.HtmlAttributes["data-focus"] = win.Focus.ToString().ToLower();
			win.HtmlAttributes["data-backdrop"] = win.BackDrop
				? (win.CloseOnBackdropClick ? "true" : "static")
				: "false";

            writer.AddAttributes(win.HtmlAttributes);

			writer.RenderBeginTag("div"); // div.modal
			{
				var className = "modal-dialog";
				switch (win.Size)
				{
					case WindowSize.Small:
						className += " modal-sm";
						break;
					case WindowSize.Large:
						className += " modal-lg";
						break;
					case WindowSize.Flex:
						className += " modal-flex";
						break;
					case WindowSize.FlexSmall:
						className += " modal-flex modal-flex-sm";
						break;
				}

				if (win.CenterVertically)
				{
					className += " modal-dialog-centered";
				}

				writer.AddAttribute("class", className);
				win.HtmlAttributes["role"] = "document";
				writer.RenderBeginTag("div"); // div.modal-dialog
				{
					writer.AddAttribute("class", "modal-content");
					writer.RenderBeginTag("div"); // div.modal-content
					{
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
					}
					writer.RenderEndTag(); // div.modal-content 
				}
				writer.RenderEndTag(); // div.modal-dialog 
			}
            writer.RenderEndTag(); // div.modal        
        }

        protected virtual void RenderHeader(HtmlTextWriter writer)
        {
            var win = base.Component;
			
            writer.AddAttribute("class", "modal-header");
            writer.RenderBeginTag("div");

			if (win.Title.HasValue())
			{
				writer.Write("<h5 class='modal-title' id='{0}'>{1}</h5>".FormatCurrent(win.Id + "Label", win.Title));
			}

			if (win.ShowClose)
            {
                writer.Write("<button type='button' class='close' data-dismiss='modal'><span aria-hidden='true'>&times;</span></button>");
            }

            writer.RenderEndTag(); // div.modal-header
        }

        protected virtual void RenderBody(HtmlTextWriter writer)
        {
            var win = base.Component;

            writer.AddAttribute("class", "modal-body");
            writer.RenderBeginTag("div");

			if (win.Content != null)
			{
				win.Content.WriteTo(writer);
			}
			else if (win.ContentUrl.HasValue())
			{
				writer.Write("<iframe class='modal-flex-fill-area' frameborder='0' src='{0}' />".FormatInvariant(win.ContentUrl));
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
