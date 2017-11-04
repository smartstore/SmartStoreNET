using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.UI
{
    public enum PagerSize
    {
		Mini,
		Small,
        Medium,
        Large
    }

    public enum PagerAlignment
    {
        Left,
        Centered,
        Right
    }

    public enum PagerStyle
    {
        Pagination,
        Blog
    }

    public class Pager : Component, INavigatable
    {
        private ILocalizationService _localizationService;

        private string _actionName;
        private string _controllerName;
        private string _routeName;
        private string _url;

        private string _firstButtonText;
        private string _lastButtonText;
        private string _nextButtonText;
        private string _previousButtonText;
        private string _currentPageText;

        public Pager()
            : this(EngineContext.Current.Resolve<ILocalizationService>())
        {
        }

        public Pager(ILocalizationService localizationService)
        {
            _localizationService = localizationService;

            this.ShowSummary = false;
            this.ShowPaginator = true;
            this.ShowPrevious = true;
            this.ShowNext = true;
            this.ShowFirst = false;
            this.ShowLast = false;
            this.MaxPagesToDisplay = 8;
            this.Alignment = PagerAlignment.Centered;
            this.Size = PagerSize.Medium;
            this.RouteValues = new RouteValueDictionary();
            this.ModifiedParam = new ModifiedParameter("i");
        }

        public IPageable Model { get; internal set; }

        public bool ShowSummary { get; set; }
        public bool ShowPaginator { get; set; }
        public bool ShowFirst { get; set; }
        public bool ShowPrevious { get; set; }
        public bool ShowNext { get; set; }
        public bool ShowLast { get; set; }
        public int MaxPagesToDisplay { get; set; }
        public bool SkipActiveState { get; set; }
        public PagerSize Size { get; set; }
        public PagerAlignment Alignment { get; set; }
        public PagerStyle Style { get; set; }
        public string ItemTitleFormatString { get; set; }

        /// <summary>
        /// Gets or sets the first button text
        /// </summary>
        public string FirstButtonText
        {
            get
            {
                return (!string.IsNullOrEmpty(_firstButtonText)) ?
                    _firstButtonText :
                    _localizationService.GetResource("Pager.First");
            }
            set
            {
                _firstButtonText = value;
            }
        }

        /// <summary>
        /// Gets or sets the last button text
        /// </summary>
        public string LastButtonText
        {
            get
            {
                return (!string.IsNullOrEmpty(_lastButtonText)) ?
                    _lastButtonText :
                    _localizationService.GetResource("Pager.Last");
            }
            set
            {
                _lastButtonText = value;
            }
        }

        /// <summary>
        /// Gets or sets the next button text
        /// </summary>
        public string NextButtonText
        {
            get
            {
                return (!string.IsNullOrEmpty(_nextButtonText)) ?
                    _nextButtonText :
                    _localizationService.GetResource("Pager.Next");
            }
            set
            {
                _nextButtonText = value;
            }
        }

        /// <summary>
        /// Gets or sets the previous button text
        /// </summary>
        public string PreviousButtonText
        {
            get
            {
                return (!string.IsNullOrEmpty(_previousButtonText)) ?
                    _previousButtonText :
                    _localizationService.GetResource("Pager.Previous");
            }
            set
            {
                _previousButtonText = value;
            }
        }

        /// <summary>
        /// Gets or sets the current page text
        /// </summary>
        public string CurrentPageText
        {
            get
            {
                return (!string.IsNullOrEmpty(_currentPageText)) ?
                    _currentPageText :
                    _localizationService.GetResource("Pager.CurrentPage");
            }
            set
            {
                _currentPageText = value;
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

        public RouteValueDictionary RouteValues
        {
            get;
            set;
        }

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
    }
}
