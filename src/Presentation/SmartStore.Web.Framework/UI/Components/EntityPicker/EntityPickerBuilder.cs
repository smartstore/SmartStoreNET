using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
    public class EntityPickerBuilder : ComponentBuilder<EntityPicker, EntityPickerBuilder>
    {
        public EntityPickerBuilder(EntityPicker component, HtmlHelper htmlHelper)
            : base(component, htmlHelper)
        {
			WithRenderer(new ViewBasedComponentRenderer<EntityPicker>("EntityPicker"));
			DialogUrl(UrlHelper.GenerateUrl(
				null,
				"Picker",
				"Entity",
				new RouteValueDictionary { { "area", "" } },
				RouteTable.Routes,
				htmlHelper.ViewContext.RequestContext,
				false));
		}

		public EntityPickerBuilder Tooltip(string value)
		{
			base.Component.HtmlAttributes["title"] = value;
			return this;
		}

		public EntityPickerBuilder DialogTitle(string value)
		{
			base.Component.DialogTitle = value;
			return this;
		}

		public EntityPickerBuilder DialogUrl(string value)
		{
			base.Component.DialogUrl = value;
			return this;
		}
	}
}
