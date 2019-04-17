using System;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI
{
	public abstract class NavigatableComponent : Component, INavigatable
	{
		private string _actionName;
		private string _controllerName;
		private string _routeName;
		private string _url;

		protected NavigatableComponent()
		{
			this.RouteValues = new RouteValueDictionary();
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
			protected set;
		}
	}
}
