using System;
using System.Web.Mvc.Html;

namespace SmartStore.Web.Framework.UI
{
	public class ViewBasedComponentRenderer<TComponent> : ComponentRenderer<TComponent> where TComponent : Component
	{
		private readonly string _viewName;

		public ViewBasedComponentRenderer() 
			: this(typeof(TComponent).Name)
		{
		}

		public ViewBasedComponentRenderer(string viewName)
		{
			Guard.NotEmpty(viewName, nameof(viewName));

			_viewName = viewName;
		}

		public override void Render()
		{
			HtmlHelper.RenderPartial("Components/" + _viewName, base.Component);
		}

		public override string ToHtmlString()
		{
			return HtmlHelper.Partial("Components/" + _viewName, base.Component).ToString();
		}
	}
}
