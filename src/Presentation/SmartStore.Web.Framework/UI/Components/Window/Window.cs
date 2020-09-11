using System.Collections.Generic;
using System.Web.Routing;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
    public enum WindowSize
    {
        Small,
        Medium,
        Large,
        Flex,
        FlexSmall
    }

    public class Window : Component
    {
        public Window()
        {
            this.Size = WindowSize.Medium;
            this.Fade = true;
            this.Focus = true;
            this.BackDrop = true;
            this.ShowClose = true;
            this.Show = true;
            this.CloseOnEscapePress = true;
            this.CloseOnBackdropClick = true;
            this.RenderAtPageEnd = true;
            this.BodyHtmlAttributes = new RouteValueDictionary();
        }

        public override bool NameIsRequired => true;

        public WindowSize Size { get; set; }

        public string Title { get; set; }

        public HelperResult Content { get; set; }

        public string ContentUrl { get; set; }

        public HelperResult FooterContent { get; set; }

        public bool Fade { get; set; }

        public bool Focus { get; set; }

        public bool Show { get; set; }

        public bool BackDrop { get; set; }

        public bool ShowClose { get; set; }

        public bool CenterVertically { get; set; }

        public bool CloseOnEscapePress { get; set; }

        public bool CloseOnBackdropClick { get; set; }

        public bool RenderAtPageEnd { get; set; }

        public IDictionary<string, object> BodyHtmlAttributes { get; set; }
    }
}
