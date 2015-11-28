using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{

    public class Window : Component
    {

        public Window()
        {
            this.Fade = true;
            this.Modal = true;
            this.Visible = true;
            this.BackDrop = true;
            this.ShowClose = true;
            this.CloseOnEscapePress = true;
        }

        public override bool NameIsRequired
        {
            get { return true; }
        }

        public string Title { get; set; }

        public HelperResult Content { get; set; }

        public string ContentUrl { get; set; }

        public HelperResult FooterContent { get; set; }

        public bool Modal { get; set; }

        public bool Fade { get; set; }

        public bool BackDrop { get; set; }

        public bool Visible { get; set; }

        public bool ShowClose { get; set; }

        public bool CloseOnEscapePress { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

    }

}
