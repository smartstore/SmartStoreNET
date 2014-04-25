using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Catalog;
using System.Linq.Dynamic;
using SmartStore.Core.Data;
using System.Globalization;
using SmartStore.Core;
using SmartStore.Utilities;

namespace SmartStore.Services.Filter
{
	public partial class FilterService : IFilterService
	{
		private const string _defaultType = "String";

		private readonly IProductService _productService;
		private readonly ICategoryService _categoryService;
		private readonly IStoreContext _storeContext;
		private IQueryable<Product> _products;
		private bool? _includeFeatured;

		public FilterService(IProductService productService, ICategoryService categoryService, IStoreContext storeContext)
		{
			_productService = productService;
			_categoryService = categoryService;
			_storeContext = storeContext;
		}

		public static int MaxDisplayCriteria { get { return 4; } }

		public static string ShortcutPrice { get { return "_Price"; } }
		public static string ShortcutSpecAttribute { get { return "_SpecId"; } }

		public bool IncludeFeatured
		{
			get
			{
				if (_includeFeatured == null)
					_includeFeatured = EngineContext.Current.Resolve<CatalogSettings>().IncludeFeaturedProductsInNormalLists;
				return _includeFeatured ?? false;
			}
		}

		// helper
		private string ValidateValue(string value, string alternativeValue)
		{
			if (value.HasValue() && !value.IsCaseInsensitiveEqual("null"))
				return value;
			return alternativeValue;
		}
		private string FormatParameterIndex(ref int index)
		{
			//if (curlyBracketFormatting)
			//	return "{0}{1}{2}".FormatWith('{', index++, '}');

			return "@{0}".FormatWith(index++);
		}
		private object FilterValueToObject(string value, string type)
		{
			if (value == null)
				value = "";

			//if (value == "__UtcNow__")
			//	return DateTime.UtcNow;
			
			//if (value == "__Now__")
			//	return DateTime.Now;

			//if (curlyBracketFormatting)
			//	return value.FormatWith("\"{0}\"", value.Replace("\"", "\"\""));

			Type t = Type.GetType("System.{0}".FormatWith(ValidateValue(type, _defaultType)));

			var result = CommonHelper.GetTypeConverter(t).ConvertFromString(null, CultureInfo.InvariantCulture, value);

			return result;
		}
		private bool IsShortcut(FilterSql context, FilterCriteria itm, ref int index)
		{
			if (itm.Entity == ShortcutPrice)
			{
				if (itm.IsRange)
				{
					string valueLeft, valueRight;
					itm.Value.SplitToPair(out valueLeft, out valueRight, "~");

					context.WhereClause.AppendFormat("((Price >= {0} And Price {1} {2}) Or (SpecialPrice >= {0} And SpecialPrice {1} {2} And SpecialPriceStartDateTimeUtc <= {3} And SpecialPriceEndDateTimeUtc >= {3}))",
						FormatParameterIndex(ref index),
						itm.Operator == FilterOperator.RangeGreaterEqualLessEqual ? "<=" : "<",
						FormatParameterIndex(ref index),
						FormatParameterIndex(ref index)
					);

					context.Values.Add(FilterValueToObject(valueLeft, itm.Type ?? "Decimal"));
					context.Values.Add(FilterValueToObject(valueRight, itm.Type ?? "Decimal"));
					context.Values.Add(DateTime.UtcNow);
				}
				else
				{
					context.WhereClause.AppendFormat("(Price {0} {1} Or (SpecialPrice {0} {1} And SpecialPriceStartDateTimeUtc <= {2} And SpecialPriceEndDateTimeUtc >= {2}))",
						itm.Operator == null ? "=" : itm.Operator.ToString(),
						FormatParameterIndex(ref index),
						FormatParameterIndex(ref index));

					context.Values.Add(FilterValueToObject(itm.Value, itm.Type ?? "Decimal"));
					context.Values.Add(DateTime.UtcNow);
				}
			}
			else if (itm.Entity == ShortcutSpecAttribute)
			{
				context.WhereClause.AppendFormat("SpecificationAttributeOptionId {0} {1}", itm.Operator == null ? "=" : itm.Operator.ToString(), FormatParameterIndex(ref index));
				
				context.Values.Add(itm.ID ?? 0);
			}
			else
			{
				return false;
			}
			return true;
		}
		private void FilterParentheses(List<FilterCriteria> criteria)
		{
			// Logical or combine all criteria with same name.
			//
			// "The order of precedence for the logical operators is NOT (highest), followed by AND, followed by OR.
			// The order of evaluation at the same precedence level is from left to right."
			// http://www.databasedev.co.uk/sql-multiple-conditions.html

			if (criteria.Count > 0)
			{
				criteria.Sort();
				criteria.ForEach(c => { c.Open = null; c.Or = false; });

				var data = (
					from c in criteria
					group c by c.Entity).Where(g => g.Count() > 1);
					//group c by c.Name).Where(g => g.Count() > 1);

				foreach (var grp in data)
				{
					grp.ToList().ForEach(f => f.Or = true);
					grp.First().Or = false;
					grp.First().Open = true;
					grp.Last().Open = false;
				}
			}
		}
		private IQueryable<Product> AllProducts(List<int> categoryIds)
		{
			if (_products == null)
			{
				var searchContext = new ProductSearchContext()
				{
					CategoryIds = categoryIds,
					FeaturedProducts = IncludeFeatured,
					StoreId = _storeContext.CurrentStoreIdIfMultiStoreMode,
					VisibleIndividuallyOnly = true
				};

				_products = _productService.PrepareProductSearchQuery(searchContext);
			}
			return _products;
		}

		private List<FilterCriteria> ProductFilterablePrices(FilterProductContext context)
		{
			var result = new List<FilterCriteria>();
			FilterCriteria criteria;
			Category category;

			var tmp = new FilterProductContext
			{
				ParentCategoryID = context.ParentCategoryID,
				CategoryIds = context.CategoryIds,
				Criteria = new List<FilterCriteria>()
			};

			if (context.ParentCategoryID != 0 && (category = _categoryService.GetCategoryById(context.ParentCategoryID)) != null && category.PriceRanges.HasValue())
			{
				string[] ranges = category.PriceRanges.SplitSafe(";");

				foreach (string range in ranges)
				{
					if ((criteria = range.ParsePriceString()) != null)
					{
						tmp.Criteria.Clear();
						tmp.Criteria.AddRange(context.Criteria);
						tmp.Criteria.Add(criteria);

						try
						{
							criteria.MatchCount = ProductFilter(tmp).Count();
						}
						catch (Exception exc) {
							exc.Dump();
						}

						if (criteria.MatchCount > 0)
							result.Add(criteria);
					}
				}
			}

			result.ForEach(c => c.IsInactive = true);
			return result;
		}
		private List<FilterCriteria> ProductFilterableManufacturer(FilterProductContext context, bool getAll = false)
		{
			bool includeFeatured = IncludeFeatured;
			var query = ProductFilter(context);

			var manus =
				from p in query
				from pm in p.ProductManufacturers
				where pm.IsFeaturedProduct == includeFeatured && !pm.Manufacturer.Deleted
				select pm.Manufacturer;

			var grouped =
				from m in manus
				orderby m.DisplayOrder
				group m by m.Id into grp
				orderby grp.Key
				select new FilterCriteria
				{
					MatchCount = grp.Count(),
					Value = grp.FirstOrDefault().Name
				};

			grouped = grouped.OrderByDescending(m => m.MatchCount);

			if (!getAll)
				grouped = grouped.Take(MaxDisplayCriteria);

			var lst = grouped.ToList();

			lst.ForEach(c =>
			{
				c.Name = "Name";
				c.Entity = "Manufacturer";
				c.IsInactive = true;
			});

			return lst;
		}
		private List<FilterCriteria> ProductFilterableSpecAttributes(FilterProductContext context, string attributeName = null)
		{
			var query = ProductFilter(context);

			var attributes =
				from p in query
				from sa in p.ProductSpecificationAttributes
				where sa.AllowFiltering && sa.ShowOnProductPage
				orderby sa.DisplayOrder
				select sa.SpecificationAttributeOption;

			if (attributeName.HasValue())
				attributes = attributes.Where(a => a.SpecificationAttribute.Name == attributeName);

			var grouped =
				from a in attributes
				group a by new { a.SpecificationAttributeId, a.Id } into g
				select new FilterCriteria
				{
					Name = g.FirstOrDefault().SpecificationAttribute.Name,
					Value = g.FirstOrDefault().Name,
					ID = g.Key.Id,
					MatchCount = g.Count()
				};


			var lst = grouped.OrderByDescending(a => a.MatchCount).ToList();

			lst.ForEach(c =>
			{
				c.Entity = ShortcutSpecAttribute;
				c.IsInactive = true;
			});

			return lst;
		}

		public virtual List<FilterCriteria> Deserialize(string jsonData)
		{
			if (jsonData.HasValue())
			{
				if (jsonData.StartsWith("["))
				{
					return JsonConvert.DeserializeObject<List<FilterCriteria>>(jsonData);
				}

				return new List<FilterCriteria> { JsonConvert.DeserializeObject<FilterCriteria>(jsonData) };
			}
			return new List<FilterCriteria>();
		}
		public virtual string Serialize(List<FilterCriteria> criteria)
		{
			//criteria.FindAll(c => c.Type.IsNullOrEmpty()).ForEach(c => c.Type = _defaultType);
			if (criteria != null && criteria.Count > 0)
				return JsonConvert.SerializeObject(criteria);
			return "";
		}

		public virtual bool ToWhereClause(FilterSql context)
		{
			if (context.Values == null)
				context.Values = new List<object>();
			else
				context.Values.Clear();

			if (context.WhereClause == null)
				context.WhereClause = new StringBuilder();
			else
				context.WhereClause.Clear();

			int index = 0;

			FilterParentheses(context.Criteria);

			foreach (var itm in context.Criteria)
			{
				if (context.WhereClause.Length > 0)
					context.WhereClause.AppendFormat(" {0} ", itm.Or ? "Or" : "And");

				if (itm.Open.HasValue && itm.Open.Value)
					context.WhereClause.Append("(");

				if (IsShortcut(context, itm, ref index))
				{
				}
				else if (itm.IsRange)
				{
					string valueLeft, valueRight;
					itm.Value.SplitToPair(out valueLeft, out valueRight, "~");

					context.WhereClause.AppendFormat("({0} >= {1} And {0} {2} {3})", 
						itm.SqlName, 
						FormatParameterIndex(ref index),
						itm.Operator == FilterOperator.RangeGreaterEqualLessEqual ? "<=" : "<",
						FormatParameterIndex(ref index)
					);

					context.Values.Add(FilterValueToObject(valueLeft, itm.Type));
					context.Values.Add(FilterValueToObject(valueRight, itm.Type));
				}
				else if (itm.Value.IsNullOrEmpty())
				{
					context.WhereClause.AppendFormat("ASCII({0}) Is Null", itm.SqlName);		// true if null or empty (string)
				}
				else {
					context.WhereClause.Append(itm.SqlName);

					if (itm.Operator == FilterOperator.Contains)
						context.WhereClause.Append(".Contains(");
					else if (itm.Operator == FilterOperator.StartsWith)
						context.WhereClause.Append(".StartsWith(");
					else if (itm.Operator == FilterOperator.EndsWith)
						context.WhereClause.Append(".EndsWith(");
					else
						context.WhereClause.AppendFormat(" {0} ", itm.Operator == null ? "=" : itm.Operator.ToString());

					context.WhereClause.Append(FormatParameterIndex(ref index));

					if (itm.Operator == FilterOperator.Contains || itm.Operator == FilterOperator.StartsWith || itm.Operator == FilterOperator.EndsWith)
						context.WhereClause.Append(")");

					context.Values.Add(FilterValueToObject(itm.Value, itm.Type));
				}

				if (itm.Open.HasValue && !itm.Open.Value)
					context.WhereClause.Append(")");
			}
			return (context.WhereClause.Length > 0);
		}
		public virtual bool ToWhereClause(FilterSql context, List<FilterCriteria> findIn, Predicate<FilterCriteria> match)
		{
			if (context.Criteria != null)
				context.Criteria.Clear();	// !

			context.Criteria = findIn.FindAll(match);
			return ToWhereClause(context);
		}

		public virtual IQueryable<Product> ProductFilter(FilterProductContext context)
		{
			var nowUtc = DateTime.UtcNow;
			var sql = new FilterSql();
			var query = AllProducts(context.CategoryIds);

			// manufacturer
			if (ToWhereClause(sql, context.Criteria, c => !c.IsInactive && c.Entity == "Manufacturer"))
			{
				bool includeFeatured = IncludeFeatured;
				
				var pmq = (
					from p in query
					from pm in p.ProductManufacturers
					where (!includeFeatured || includeFeatured == pm.IsFeaturedProduct) && !pm.Manufacturer.Deleted
					select pm).Where(sql.WhereClause.ToString(), sql.Values.ToArray());

				query = pmq.Select(pm => pm.Product);
			}

			// specification attribute
			if (ToWhereClause(sql, context.Criteria, c => !c.IsInactive && (c.Entity == "SpecificationAttributeOption" || c.Entity == ShortcutSpecAttribute)))
			{
				//var saq = (
				//	from p in query
				//	from sa in p.ProductSpecificationAttributes
				//	select sa).Where(a => a.AllowFiltering).Where(sql.WhereClause.ToString(), sql.Values.ToArray());

				//query = saq.Select(sa => sa.Product);

				int countSameNameAttributes = sql.Criteria
					.Where(c => c.Entity == ShortcutSpecAttribute)
					.GroupBy(c => c.Name)
					.Count();

				var specRepository = EngineContext.Current.Resolve<IRepository<ProductSpecificationAttribute>>();

				var saq = specRepository.Table
					.Where(a => a.AllowFiltering && a.ShowOnProductPage)
					.Where(sql.WhereClause.ToString(), sql.Values.ToArray())
					.GroupBy(a => a.ProductId)
					.Where(grp => (grp.Count() >= countSameNameAttributes));

				query =
					from p in query
					join sa in saq on p.Id equals sa.Key
					select p;
			}

			// sort
			var order = (ProductSortingEnum)(context.OrderBy ?? 0);
			switch (order)
			{
				case ProductSortingEnum.NameDesc:
					query = query.OrderByDescending(p => p.Name);
					break;
				case ProductSortingEnum.PriceAsc:
					query = query.OrderBy(p => p.Price);
					break;
				case ProductSortingEnum.PriceDesc:
					query = query.OrderByDescending(p => p.Price);
					break;
				case ProductSortingEnum.CreatedOn:
					query = query.OrderByDescending(p => p.CreatedOnUtc);
					break;
				case ProductSortingEnum.CreatedOnAsc:
					query = query.OrderBy(p => p.CreatedOnUtc);
					break;
				default:
					query = query.OrderBy(p => p.Name);
					break;
			}


			// distinct (required?)
			//query =
			//	from p in query
			//	group p by p.Id into grp
			//	orderby grp.Key
			//	select grp.FirstOrDefault();

			//query.ToString().Dump();
			return query;
		}
		
		public virtual void ProductFilterable(FilterProductContext context)
		{
			if (context.Criteria.FirstOrDefault(c => c.Entity == FilterService.ShortcutPrice) == null)
				context.Criteria.AddRange(ProductFilterablePrices(context));

			if (context.Criteria.FirstOrDefault(c => c.Name == "Name" && c.Entity == "Manufacturer") == null)
				context.Criteria.AddRange(ProductFilterableManufacturer(context));

			context.Criteria.AddRange(ProductFilterableSpecAttributes(context));
		}
		public virtual void ProductFilterableMultiSelect(FilterProductContext context, string filterMultiSelect)
		{
			var criteriaMultiSelect = Deserialize(filterMultiSelect).FirstOrDefault();
			List<FilterCriteria> inactive = null;

			if (criteriaMultiSelect != null)
			{
				context.Criteria.RemoveAll(c => c.Name == criteriaMultiSelect.Name && c.Entity == criteriaMultiSelect.Entity);

				if (criteriaMultiSelect.Name == "Name" && criteriaMultiSelect.Entity == "Manufacturer")
					inactive = ProductFilterableManufacturer(context, true);
				else if (criteriaMultiSelect.Entity == FilterService.ShortcutPrice)
					inactive = ProductFilterablePrices(context);
				else if (criteriaMultiSelect.Entity == FilterService.ShortcutSpecAttribute)
					inactive = ProductFilterableSpecAttributes(context, criteriaMultiSelect.Name);
			}

			// filters WITHOUT the multiple selectable filters
			string excludedFilter = Serialize(context.Criteria);

			// filters WITH the multiple selectable filters (required for highlighting selected values)
			context.Criteria = Deserialize(context.Filter);

			context.Filter = excludedFilter;

			if (inactive != null)
			{
				inactive.ForEach(c => c.IsInactive = true);
				context.Criteria.AddRange(inactive);
			}
		}
	}
}

