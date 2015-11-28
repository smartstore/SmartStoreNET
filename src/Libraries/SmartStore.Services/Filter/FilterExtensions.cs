using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Filter
{
	public static class FilterExtensions
	{
		private static string FormatPrice(string value)
		{
			if (value.HasValue())
			{
				decimal d = 0;
				if (StringToPrice(value, out d))
					return EngineContext.Current.Resolve<IPriceFormatter>().FormatPrice(d, true, false);
			}
			return value;
		}

		public static string ToDescription(this FilterCriteria criteria)
		{
			var localize = EngineContext.Current.Resolve<ILocalizationService>();

			if (criteria == null || criteria.Value.IsEmpty())
				return localize.GetResource("Common.Unspecified");

			if (criteria.Operator == FilterOperator.RangeGreaterEqualLessEqual || criteria.Operator == FilterOperator.RangeGreaterEqualLess)
			{
				string valueLeft, valueRight;
				criteria.Value.SplitToPair(out valueLeft, out valueRight, "~");

				if (criteria.Entity == FilterService.ShortcutPrice)
					return "{0} - {1}".FormatWith(FormatPrice(valueLeft), FormatPrice(valueRight));

				return "{0} - {1}".FormatWith(valueLeft, valueRight);
			}

			string value = (criteria.ValueLocalized.HasValue() ? criteria.ValueLocalized : criteria.Value);

			if (criteria.Entity == FilterService.ShortcutPrice)
				value = FormatPrice(criteria.Value);

			if (criteria.Operator == FilterOperator.Unequal)
				return "≠ {0}".FormatWith(value);
			if (criteria.Operator == FilterOperator.Greater)
				return "> {0}".FormatWith(value);
			if (criteria.Operator == FilterOperator.GreaterEqual)
				return "≥ {0}".FormatWith(value);
			if (criteria.Operator == FilterOperator.Less)
				return "< {0}".FormatWith(value);
			if (criteria.Operator == FilterOperator.LessEqual)
				return "≤ {0}".FormatWith(value);
			if (criteria.Operator == FilterOperator.Contains)
				return "{0} {1}".FormatWith(localize.GetResource("Products.Filter.Contains"), value);
			if (criteria.Operator == FilterOperator.StartsWith)
				return "{0} {1}".FormatWith(localize.GetResource("Products.Filter.StartsWith"), value);
			if (criteria.Operator == FilterOperator.EndsWith)
				return "{0} {1}".FormatWith(localize.GetResource("Products.Filter.EndsWith"), value);

			return value;
		}

		public static FilterCriteria ParsePriceString(this string priceRange)
		{
			try
			{
				if (priceRange.HasValue())
				{
					priceRange = priceRange.Trim();

					if (priceRange.HasValue())
					{
						decimal from, to;
						string[] range = priceRange.Split(new char[] { '-' });
						bool hasFrom = range.StringToPrice(0, out from);
						bool hasTo = range.StringToPrice(1, out to);

						var criteria = new FilterCriteria
						{
							Name = "Price",
							Entity = FilterService.ShortcutPrice
						};

						// avoid overlapping
						if (hasFrom && hasTo)
						{
							//criteria.Operator = FilterOperator.RangeGreaterEqualLess;
							criteria.Operator = FilterOperator.RangeGreaterEqualLessEqual;
							criteria.Value = from.ToString(CultureInfo.InvariantCulture) + "~" + to.ToString(CultureInfo.InvariantCulture);
						}
						else if (hasFrom)
						{
							//criteria.Operator = FilterOperator.GreaterEqual;
							criteria.Operator = FilterOperator.Greater;
							criteria.Value = from.ToString(CultureInfo.InvariantCulture);
						}
						else if (hasTo)
						{
							//criteria.Operator = FilterOperator.LessEqual;
							criteria.Operator = FilterOperator.Less;
							criteria.Value = to.ToString(CultureInfo.InvariantCulture);
						}
						else
						{
							return null;
						}
						return criteria;
					}
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}
			return null;
		}

		public static bool StringToPrice(string value, out decimal result)
		{
			result = 0;
			return (value.HasValue() && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out result));
		}

		public static bool StringToPrice(this string[] range, int index, out decimal result)
		{
			result = 0;
			if (range != null && index < range.Length)
			{
				string value = range[index].Trim();
				return StringToPrice(value, out result);
			}
			return false;
		}

		public static string GetUrl(this FilterProductContext context, FilterCriteria criteriaAdd = null, FilterCriteria criteriaRemove = null)
		{
			string url = "{0}?pagesize={1}&viewmode={2}".FormatWith(context.Path, context.PageSize, context.ViewMode);

			if (context.OrderBy.HasValue)
			{
				url = "{0}&orderby={1}".FormatWith(url, context.OrderBy.Value);
			}

			try
			{
				if (criteriaAdd != null || criteriaRemove != null)
				{
					var criterias = new List<FilterCriteria>();

					if (context.Criteria != null && context.Criteria.Count > 0)
						criterias.AddRange(context.Criteria.Where(c => !c.IsInactive));

					if (criteriaAdd != null)
						criterias.Add(criteriaAdd);
					else
						criterias.RemoveAll(c => c.Entity == criteriaRemove.Entity && c.Name == criteriaRemove.Name && c.Value == criteriaRemove.Value);

					if (criterias.Count > 0)
						url = "{0}&filter={1}".FormatWith(url, HttpUtility.UrlEncode(JsonConvert.SerializeObject(criterias)));
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			return url;
		}

		public static bool IsActive(this FilterProductContext context, FilterCriteria criteria)
		{
			if (criteria != null && context.Criteria != null)
			{
				return (context.Criteria.FirstOrDefault(c => c.Entity == criteria.Entity && c.Name == criteria.Name && c.Value == criteria.Value && !c.IsInactive) != null);
			}
			return false;
		}
	}
}
