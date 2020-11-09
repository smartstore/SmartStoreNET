using System;
using System.Collections.Generic;
using System.Web.Routing;
using System.Web.WebPages;
using Newtonsoft.Json;

namespace SmartStore.Web.Framework.UI
{
    [Serializable]
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
            this.HtmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            this.LinkHtmlAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            this.RouteValues = new RouteValueDictionary();
            this.ModifiedParam = new ModifiedParameter();
        }

        public IDictionary<string, object> HtmlAttributes { get; set; }

        public IDictionary<string, object> LinkHtmlAttributes { get; set; }

        /// <summary>
        /// Merges attributes of <see cref="HtmlAttributes"/> and <see cref="LinkHtmlAttributes"/> into one combined dictionary.
        /// </summary>
        /// <returns>New dictionary instance with combined attributes.</returns>
        public IDictionary<string, object> GetCombinedAttributes()
        {
            if (HtmlAttributes == null && LinkHtmlAttributes == null)
            {
                return null;
            }

            var combined = new RouteValueDictionary(HtmlAttributes ?? LinkHtmlAttributes);

            if (HtmlAttributes != null && LinkHtmlAttributes != null)
            {
                combined.Merge(LinkHtmlAttributes);
            }

            return combined;
        }

        public string ImageUrl { get; set; }

        public int? ImageId { get; set; }

        public string Icon { get; set; }

        public string Text { get; set; }

        public bool Rtl { get; set; }

        public string Summary { get; set; }

        public string BadgeText { get; set; }

        public BadgeStyle BadgeStyle { get; set; }

        public bool Visible { get; set; }

        public bool Encoded { get; set; }

        public bool Selected
        {
            get => _selected;
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
            get => _enabled;
            set
            {
                _enabled = value;
                if (!_enabled)
                {
                    _selected = false;
                }
            }
        }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ActionName
        {
            get => _actionName;
            set
            {
                if (_actionName != value)
                {
                    _actionName = value;
                    _routeName = (string)(_url = null);
                }
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ControllerName
        {
            get => _controllerName;
            set
            {
                if (_controllerName != value)
                {
                    _controllerName = value;
                    _routeName = (string)(_url = null);
                }
            }
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RouteName
        {
            get => _routeName;
            set
            {
                if (_routeName != value)
                {
                    _routeName = value;
                    _controllerName = _actionName = (string)(_url = null);
                }
            }
        }

        public RouteValueDictionary RouteValues { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Url
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    _routeName = _controllerName = (string)(_actionName = null);
                    this.RouteValues.Clear();
                }
            }
        }

        [JsonIgnore]
        public ModifiedParameter ModifiedParam
        {
            get;
            private set;
        }

        /// <summary>
        /// Checks whether action/controller or routeName or url has been specified.
        /// </summary>
		public bool HasRoute()
        {
            return _actionName != null || _routeName != null || _url != null;
        }

        /// <summary>
        /// Checks whether url has been specified with '#' or 'javascript:void()' or empty string.
        /// </summary>
        public bool IsVoid()
        {
            // Perf: order from most to least common
            return _url != null && (_url == "#" || _url.StartsWith("javascript:void") || _url == string.Empty || _url.IsWhiteSpace());
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

        public IDictionary<string, object> ContentHtmlAttributes { get; set; }

        public HelperResult Content { get; set; }
    }

}
