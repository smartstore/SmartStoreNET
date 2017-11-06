using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
    public class EntityPickerBuilder<TModel> : ComponentBuilder<EntityPicker, EntityPickerBuilder<TModel>, TModel>
    {
        public EntityPickerBuilder(EntityPicker component, HtmlHelper<TModel> htmlHelper)
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

		public EntityPickerBuilder<TModel> EntityType(string value)
		{
			base.Component.EntityType = value;
			return this;
		}

		public EntityPickerBuilder<TModel> Caption(string value)
		{
			base.Component.Caption = value;
			return this;
		}

		public EntityPickerBuilder<TModel> Tooltip(string value)
		{
			base.Component.HtmlAttributes["title"] = value;
			return this;
		}

		public EntityPickerBuilder<TModel> DialogTitle(string value)
		{
			base.Component.DialogTitle = value;
			return this;
		}

		public EntityPickerBuilder<TModel> DialogUrl(string value)
		{
			base.Component.DialogUrl = value;
			return this;
		}

		public EntityPickerBuilder<TModel> MaxItems(int value)
		{
			base.Component.MaxItems = value;
			return this;
		}
	}
}
