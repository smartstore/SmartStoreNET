using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{

    public abstract class NavigationItem : IHtmlAttributesContainer, INavigatable, IHideObjectMembers
    {
        private bool _selected;
        private bool _enabled;
        private string _actionName;
        private string _controllerName;
        private string _routeName;
        private string _url;

        public NavigationItem()
        {
            this.Visible = true;
            this.Encoded = true;
            this.Enabled = true;
            this.HtmlAttributes = new RouteValueDictionary();
            this.LinkHtmlAttributes = new RouteValueDictionary();
            this.RouteValues = new RouteValueDictionary();
            this.ModifiedParam = new ModifiedParameter();
        }

        public IDictionary<string, object> HtmlAttributes { get; private set; }

        public IDictionary<string, object> LinkHtmlAttributes { get; private set; }

        public string ImageUrl { get; set; }

        public string Icon { get; set; }

        public string Text { get; set; }

        public string BadgeText { get; set; }

        public BadgeStyle BadgeStyle { get; set; }

        public bool Visible { get; set; }

        public bool Encoded { get; set; }

        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                _selected = value;
                if (_selected)
                {
                    _enabled = true;
                }
            }
        }

        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;
                if (!_enabled)
                {
                    _selected = false;
                }
            }
        }


        public string ActionName
        {
            get
            {
                return _actionName;
            }
            set
            {
                _actionName = value;
                _routeName = (string)(_url = null);
            }
        }

        public string ControllerName
        {
            get
            {
                return _controllerName;
            }
            set
            {
                _controllerName = value;
                _routeName = (string)(_url = null);
            }
        }

        public string RouteName
        {
            get
            {
                return _routeName;
            }
            set
            {
                _routeName = value;
                _controllerName = _actionName = (string)(_url = null);
            }
        }

        public RouteValueDictionary RouteValues { get; set; }

        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                _routeName = _controllerName = (string)(_actionName = null);
                this.RouteValues.Clear();

            }
        }

        public ModifiedParameter ModifiedParam
        {
            get;
            private set;
        }

		public bool HasRoute()
		{
			return _actionName != null || _routeName != null || _url != null;
		}

        public override string ToString()
        {
            if (this.Text.HasValue())
            {
                return this.Text;
            }
            return base.ToString();
        }

    }

    public abstract class NavigationItemWithContent : NavigationItem, IContentContainer, IHideObjectMembers
    {

        public NavigationItemWithContent()
        {
            this.ContentHtmlAttributes = new RouteValueDictionary();
        }

		public bool Ajax { get; set; }

        public IDictionary<string, object> ContentHtmlAttributes { get; private set; }

        public HelperResult Content { get; set; }

    }

}
