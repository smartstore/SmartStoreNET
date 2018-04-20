using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Search.Modelling;

namespace SmartStore.Services.Search.Extensions
{
	public class FacetUrlHelper
	{
		private readonly ICatalogSearchQueryAliasMapper _mapper;
		private readonly IWorkContext _workContext;
		private readonly HttpRequestBase _httpRequest;
		private readonly SearchSettings _searchSettings;

		private readonly int _languageId;
		private readonly string _url;
		private readonly QueryString _initialQuery;

		private readonly static IDictionary<FacetGroupKind, string> _queryNames = new Dictionary<FacetGroupKind, string>
		{
			{ FacetGroupKind.Brand, "m" },
			{ FacetGroupKind.Category, "c" },
			{ FacetGroupKind.Price, "p" },
			{ FacetGroupKind.Rating, "r" },
			{ FacetGroupKind.DeliveryTime, "d" },
			{ FacetGroupKind.Availability, "a" },
			{ FacetGroupKind.NewArrivals, "n" }
		};

		public FacetUrlHelper(
			ICatalogSearchQueryAliasMapper mapper,
			IWorkContext workContext,
			HttpRequestBase httpRequest,
			SearchSettings searchSettings)
		{
			_mapper = mapper;
			_workContext = workContext;
			_httpRequest = httpRequest;
			_searchSettings = searchSettings;

			_languageId = _workContext.WorkingLanguage.Id;
			_url = _httpRequest.CurrentExecutionFilePath;
			//_initialQuery = new QueryString().FillFromString(_httpRequest.QueryString.ToString(), false);
			_initialQuery = QueryString.CurrentUnvalidated;

			// Remove page index (i) from query string
			_initialQuery.Remove("i");
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
					var qsName = name;

					if (!qs.AllKeys.Contains(name))
					{
						// Query string does not contain that name. Try the unmapped name.
						switch (facet.FacetGroup.Kind)
						{
							case FacetGroupKind.Attribute:
								qsName = "attr" + facet.Value.ParentId;
								break;
							case FacetGroupKind.Variant:
								qsName = "vari" + facet.Value.ParentId;
								break;
							case FacetGroupKind.Category:
							case FacetGroupKind.Brand:
							case FacetGroupKind.Price:
							case FacetGroupKind.Rating:
							case FacetGroupKind.DeliveryTime:
							case FacetGroupKind.Availability:
							case FacetGroupKind.NewArrivals:
								qsName = _queryNames[facet.FacetGroup.Kind];
								break;
						}
					}

					string[] currentValues = null;

					// The query string value is not necessarily equal to the facet value.
					// We must skip subsequent lines here to not add the removed value again and again.
					if (facet.FacetGroup.Kind != FacetGroupKind.Price &&
						facet.FacetGroup.Kind != FacetGroupKind.Availability &&
						facet.FacetGroup.Kind != FacetGroupKind.NewArrivals)
					{
						currentValues = qs.Get(qsName)?.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
					}

					qs.Remove(qsName);

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

		public string GetQueryName(Facet facet)
		{
			var parts = GetQueryParts(facet);
			return parts.GetKey(0);
		}

		protected virtual NameValueCollection GetQueryParts(Facet facet)
		{
			string name = null;
			string value = null;
			int entityId;

			var group = facet.FacetGroup;
			var val = facet.Value;

			var result = new NameValueCollection(2);

			switch (group.Kind)
			{
				case FacetGroupKind.Attribute:
					if (facet.Value.TypeCode == IndexTypeCode.Double)
					{
						value = "{0}~{1}".FormatInvariant(
							val.Value != null ? ((double)val.Value).ToString(CultureInfo.InvariantCulture) : "",
							val.UpperValue != null ? ((double)val.UpperValue).ToString(CultureInfo.InvariantCulture) : "");
					}
					else
					{
						entityId = val.Value.Convert<int>();
						value = _mapper.GetAttributeOptionAliasById(entityId, _languageId) ?? "opt" + entityId;
					}
					name = _mapper.GetAttributeAliasById(val.ParentId, _languageId) ?? "attr" + val.ParentId;
					result.Add(name, value);
					break;
				case FacetGroupKind.Variant:
					entityId = val.Value.Convert<int>();
					name = _mapper.GetVariantAliasById(val.ParentId, _languageId) ?? "vari" + val.ParentId;
					value = _mapper.GetVariantOptionAliasById(entityId, _languageId) ?? "opt" + entityId;
					result.Add(name, value);
					break;
				case FacetGroupKind.Category:
				case FacetGroupKind.Brand:
				case FacetGroupKind.Price:
				case FacetGroupKind.Rating:
				case FacetGroupKind.DeliveryTime:
				case FacetGroupKind.Availability:
				case FacetGroupKind.NewArrivals:
					value = val.ToString();
					if (value.HasValue())
					{
						name = _mapper.GetCommonFacetAliasByGroupKind(group.Kind, _languageId) ?? _queryNames[group.Kind];
						result.Add(name, value);
					}

					break;
			}

			return result;
		}
	}
}
