using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Search.Modelling;

namespace SmartStore.Services.Search.Extensions
{
	public class FacetUrlHelper
	{
		private readonly ICatalogSearchQueryAliasMapper _mapper;
		private readonly IWorkContext _workContext;
		private readonly HttpRequestBase _httpRequest;

		private readonly int _languageId;
		private readonly string _url;
		private readonly QueryString _initialQuery;

		public FacetUrlHelper(
			ICatalogSearchQueryAliasMapper mapper,
			IWorkContext workContext,
			HttpRequestBase httpRequest)
		{
			_mapper = mapper;
			_workContext = workContext;
			_httpRequest = httpRequest;

			_languageId = _workContext.WorkingLanguage.Id;
			_url = _httpRequest.CurrentExecutionFilePath;
			//_initialQuery = new QueryString().FillFromString(_httpRequest.QueryString.ToString(), false);
			_initialQuery = QueryString.Current;
		}

		public string Toggle(Facet facet)
		{
			if (facet.Value.IsSelected)
			{
				return Remove(facet);
			}
			else
			{
				return Add(facet);
			}
		}

		public string Add(params Facet[] facets)
		{
			var qs = new QueryString(_initialQuery);

			foreach (var facet in facets)
			{
				var parts = GetQueryParts(facet);
				foreach (var name in parts.AllKeys)
				{
					qs.Add(name, parts[name], !facet.FacetGroup.IsMultiSelect);
				}
			}

			return _url + qs.ToString(false);
		}

		public string Remove(params Facet[] facets)
		{
			var qs = new QueryString(_initialQuery);

			foreach (var facet in facets)
			{
				var parts = GetQueryParts(facet);
				foreach (var name in parts.AllKeys)
				{
					var currentValues = qs.Get(name).Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
					qs.Remove(name);

					if (currentValues != null)
					{
						var removeValues = parts.GetValues(name);
						var newValues = currentValues.Except(removeValues).ToArray();
						if (newValues.Length > 0)
						{
							newValues.Each(x => qs.Add(name, x, false));
						}
					}
				}
			}

			return _url + qs.ToString(false);
		}

		protected virtual NameValueCollection GetQueryParts(Facet facet)
		{
			var group = facet.FacetGroup;

			// TODO: (mc) > (mg) Implement alias mapping for common facet groups like category, price etc.

			string name = null;
			string value = null;
			int entityId;

			var result = new NameValueCollection(2);

			if (group.Kind == FacetGroupKind.Attribute)
			{
				// TODO: (mc) > (mh) Handle range type attributes also!
				entityId = facet.Value.Value.Convert<int>();
				name = _mapper.GetAttributeAliasById(facet.Value.ParentId, _languageId) ?? "attr" + facet.Value.ParentId;
				value = _mapper.GetAttributeOptionAliasById(entityId, _languageId) ?? "opt" + entityId;
				result.Add(name, value);
			}
			else if (group.Kind == FacetGroupKind.Variant)
			{
				entityId = facet.Value.Value.Convert<int>();
				name = _mapper.GetVariantAliasById(facet.Value.ParentId, _languageId) ?? "vari" + facet.Value.ParentId;
				value = _mapper.GetVariantOptionAliasById(entityId, _languageId) ?? "opt" + entityId;
				result.Add(name, value);
			}
			else if (group.Kind == FacetGroupKind.Category)
			{
				result.Add("c", facet.Value.Value.ToString());
			}
			else if (group.Kind == FacetGroupKind.Brand)
			{
				result.Add("m", facet.Value.Value.ToString());
			}
			else if (group.Kind == FacetGroupKind.Rating)
			{
				result.Add("r", facet.Value.Value.ToString());
			}
			else if (group.Kind == FacetGroupKind.DeliveryTime)
			{
				result.Add("d", facet.Value.Value.ToString());
			}
			else if (group.Kind == FacetGroupKind.Price)
			{
				// TODO: (mc) handle price properly
				result.Add("pt", facet.Value.UpperValue.ToString());
			}

			return result;
		}
	}
}
