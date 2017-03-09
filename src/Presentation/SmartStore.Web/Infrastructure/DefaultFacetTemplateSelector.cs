using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Search.Rendering;

namespace SmartStore.Web.Infrastructure
{
	public class DefaultFacetTemplateSelector : IFacetTemplateSelector
	{
		public RouteInfo GetTemplateRoute(FacetGroup facetGroup)
		{
			var templateName = GetTemplateName(facetGroup);
			if (templateName.IsEmpty())
			{
				return null;
			}

			string controller = "Search";
			string action = "FacetGroup";
			var routeValues = new RouteValueDictionary(new
			{
				area = "",
				facetGroup = facetGroup,
				templateName = templateName
			});

			return new RouteInfo(action, controller, routeValues);
		}

		private string GetTemplateName(FacetGroup group)
		{
			var prefix = "FacetTemplates/";

			switch (group.Kind)
			{
				case FacetGroupKind.DeliveryTime:
				case FacetGroupKind.Brand:
					return prefix + (group.IsMultiSelect ? "MultiSelect" : "SingleSelect");
				case FacetGroupKind.Category:
					return prefix + "MultiSelect"; // TODO: (mc) change this
				case FacetGroupKind.Price:
					return prefix + "Price";
				case FacetGroupKind.Rating:
					return prefix + "SingleSelect";
			}

			return null;
		}

		public int Ordinal
		{
			get { return -100; }
		}
	}
}
