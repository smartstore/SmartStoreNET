using System;
using System.Web.WebPages;

namespace SmartStore.Web.Framework.UI
{
    public class EntityPicker : Component
    {
        public EntityPicker()
        {
			EntityType = "product";
			IconCssClass = "fa fa-search";
			HtmlAttributes["type"] = "button";
			HtmlAttributes.AppendCssClass("btn btn-secondary");
		}

		public string EntityType { get; set; }

		public string Caption { get; set; }

		public string DialogTitle { get; set; }

		public string DialogUrl { get; set; }

		public string IconCssClass { get; set; }

		public int MaxSelections { get; set; }
	}
}
