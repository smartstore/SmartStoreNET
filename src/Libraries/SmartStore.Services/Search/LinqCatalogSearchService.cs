using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Core.Search;
using SmartStore.Services.Catalog;

namespace SmartStore.Services.Search
{
	public partial class LinqCatalogSearchService : ICatalogSearchService
	{
		private readonly IProductService _productService;
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IRepository<AclRecord> _aclRepository;
		private readonly IEventPublisher _eventPublisher;

		public LinqCatalogSearchService(
			IProductService productService,
			IRepository<Product> productRepository,
			IRepository<LocalizedProperty> localizedPropertyRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IRepository<AclRecord> aclRepository,
			IEventPublisher eventPublisher)
		{
			_productService = productService;
			_productRepository = productRepository;
			_localizedPropertyRepository = localizedPropertyRepository;
			_storeMappingRepository = storeMappingRepository;
			_aclRepository = aclRepository;
			_eventPublisher = eventPublisher;

			QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

		#region Utilities

		private void FlattenFilters(ICollection<ISearchFilter> filters, List<ISearchFilter> result)
		{
			foreach (var filter in filters)
			{
				var combinedFilter = filter as ICombinedSearchFilter;
				if (combinedFilter != null)
				{
					FlattenFilters(combinedFilter.Filters, result);
				}
				else
				{
					result.Add(filter);
				}
			}
		}

		private List<int> GetIdList(List<ISearchFilter> filters, string fieldName)
		{
			var result = new List<int>();

			foreach (IAttributeSearchFilter filter in filters)
			{
				if (!(filter is IRangeSearchFilter) && filter.FieldName == fieldName)
					result.Add((int)filter.Term);
			}

			return result;
		}

		private IOrderedQueryable<Product> OrderBy<TKey>(ref bool ordered, IQueryable<Product> query, Expression<Func<Product, TKey>> keySelector, bool descending = false)
		{
			if (ordered)
			{
				if (descending)
					return ((IOrderedQueryable<Product>)query).ThenByDescending(keySelector);

				return ((IOrderedQueryable<Product>)query).ThenBy(keySelector);
			}
			else
			{
				ordered = true;

				if (descending)
					return query.OrderByDescending(keySelector);

				return query.OrderBy(keySelector);
			}
		}

		private IQueryable<Product> QueryCategories(IQueryable<Product> query, List<int> ids, bool? featuredOnly)
		{
			if (ids.Any())
			{
				return
					from p in query
					from pc in p.ProductCategories.Where(pc => ids.Contains(pc.CategoryId))
					where (!featuredOnly.HasValue || featuredOnly.Value == pc.IsFeaturedProduct)
					select p;
			}

			return query;
		}

		private IQueryable<Product> QueryManufacturers(IQueryable<Product> query, List<int> ids, bool? featuredOnly)
		{
			if (ids.Any())
			{
				return
					from p in query
					from pm in p.ProductManufacturers.Where(pm => ids.Contains(pm.ManufacturerId))
					where (!featuredOnly.HasValue || featuredOnly.Value == pm.IsFeaturedProduct)
					select p;
			}

			return query;
		}

		protected virtual IQueryable<Product> GetProductQuery(CatalogSearchQuery searchQuery)
		{
			var ordered = false;
			var utcNow = DateTime.UtcNow;
			var term = searchQuery.Term;
			var fields = searchQuery.Fields;
			var languageId = searchQuery.LanguageId ?? 0;

			var query = _productRepository.Table
				.Where(x => !x.Deleted);

			#region Search Term

			if (term.HasValue() && fields != null && fields.Length != 0 && fields.Any(x => x.HasValue()))
			{
				if (searchQuery.Mode == SearchMode.StartsWith)
				{
					query =
						from p in query
						join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into plp
						from lp in plp.DefaultIfEmpty()
						where
						(fields.Contains("name") && p.Name.StartsWith(term)) ||
						(fields.Contains("sku") && p.Sku.StartsWith(term)) ||
						(fields.Contains("shortdescription") && p.ShortDescription.StartsWith(term)) ||
						(languageId != 0 && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "Name" && lp.LocaleValue.StartsWith(term)) ||
						(languageId != 0 && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" && lp.LocaleValue.StartsWith(term))
						select p;
				}
				else if (searchQuery.Mode == SearchMode.Contains)
				{
					query =
						from p in query
						join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into plp
						from lp in plp.DefaultIfEmpty()
						where
						(fields.Contains("name") && p.Name.Contains(term)) ||
						(fields.Contains("sku") && p.Sku.Contains(term)) ||
						(fields.Contains("shortdescription") && p.ShortDescription.Contains(term)) ||
						(languageId != 0 && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "Name" && lp.LocaleValue.Contains(term)) ||
						(languageId != 0 && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" && lp.LocaleValue.Contains(term))
						select p;
				}
				else
				{
					query =
						from p in query
						join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into plp
						from lp in plp.DefaultIfEmpty()
						where
						(fields.Contains("name") && p.Name == term) ||
						(fields.Contains("sku") && p.Sku == term) ||
						(fields.Contains("shortdescription") && p.ShortDescription == term) ||
						(languageId != 0 && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "Name" && lp.LocaleValue == term) ||
						(languageId != 0 && lp.LanguageId == languageId && lp.LocaleKeyGroup == "Product" && lp.LocaleKey == "ShortDescription" && lp.LocaleValue == term)
						select p;
				}
			}

			#endregion

			#region Filters

			var filters = new List<ISearchFilter>();
			FlattenFilters(searchQuery.Filters, filters);

			var productIds = GetIdList(filters, "id");
			if (productIds.Any())
			{
				query = query.Where(x => productIds.Contains(x.Id));
			}

			var categoryIds = GetIdList(filters, "categoryid");
			if (categoryIds.Any())
			{
				if (categoryIds.Count == 1 && categoryIds.First() == 0)
				{
					// has no category
					query = query.Where(x => x.ProductCategories.Count == 0);
				}
				else
				{
					query = QueryCategories(query, categoryIds, null);
				}
			}

			query = QueryCategories(query, GetIdList(filters, "featuredcategoryid"), true);
			query = QueryCategories(query, GetIdList(filters, "notfeaturedcategoryid"), false);

			var manufacturerIds = GetIdList(filters, "manufacturerid");
			if (manufacturerIds.Any())
			{
				if (manufacturerIds.Count == 1 && manufacturerIds.First() == 0)
				{
					// has no manufacturer
					query = query.Where(x => x.ProductManufacturers.Count == 0);
				}
				else
				{
					query = QueryManufacturers(query, manufacturerIds, null);
				}
			}

			query = QueryManufacturers(query, GetIdList(filters, "featuredmanufacturerid"), true);
			query = QueryManufacturers(query, GetIdList(filters, "notfeaturedmanufacturerid"), false);

			var tagIds = GetIdList(filters, "tagid");
			if (tagIds.Any())
			{
				query =
					from p in query
					from pt in p.ProductTags.Where(pt => tagIds.Contains(pt.Id))
					select p;
			}

			if (!QuerySettings.IgnoreAcl)
			{
				var roleIds = GetIdList(filters, "roleid");
				if (roleIds.Any())
				{
					query =
						from p in query
						join acl in _aclRepository.Table on new { pid = p.Id, pname = "Product" } equals new { pid = acl.EntityId, pname = acl.EntityName } into pacl
						from acl in pacl.DefaultIfEmpty()
						where !p.SubjectToAcl || roleIds.Contains(acl.CustomerRoleId)
						select p;
				}
			}

			foreach (IAttributeSearchFilter filter in filters)
			{
				var rangeFilter = filter as IRangeSearchFilter;

				if (filter.FieldName == "id")
				{
					if (rangeFilter != null)
					{
						var lower = filter.Term as int?;
						var upper = rangeFilter.UpperTerm as int?;

						if (lower.HasValue)
						{
							if (rangeFilter.IncludesLower)
								query = query.Where(x => x.Id >= lower.Value);
							else
								query = query.Where(x => x.Id > lower.Value);
						}

						if (upper.HasValue)
						{
							if (rangeFilter.IncludesUpper)
								query = query.Where(x => x.Id <= upper.Value);
							else
								query = query.Where(x => x.Id < upper.Value);
						}
					}
				}
				else if (filter.FieldName == "categoryid")
				{
					if (rangeFilter != null && 1 == ((filter.Term as int?) ?? 0) && int.MaxValue == ((rangeFilter.UpperTerm as int?) ?? 0))
					{
						// has any category
						query = query.Where(x => x.ProductCategories.Count > 0);
					}
				}
				else if (filter.FieldName == "manufacturerid")
				{
					if (rangeFilter != null && 1 == ((filter.Term as int?) ?? 0) && int.MaxValue == ((rangeFilter.UpperTerm as int?) ?? 0))
					{
						// has any manufacturer
						query = query.Where(x => x.ProductManufacturers.Count > 0);
					}
				}
				else if (filter.FieldName == "published")
				{
					query = query.Where(x => x.Published == (bool)filter.Term);
				}
				else if (filter.FieldName == "availablestart")
				{
					if (rangeFilter != null)
					{
						var lower = filter.Term as DateTime?;
						var upper = rangeFilter.UpperTerm as DateTime?;

						if (lower.HasValue)
						{
							if (rangeFilter.IncludesLower)
								query = query.Where(x => !x.AvailableStartDateTimeUtc.HasValue || x.AvailableStartDateTimeUtc >= lower.Value);
							else
								query = query.Where(x => !x.AvailableStartDateTimeUtc.HasValue || x.AvailableStartDateTimeUtc > lower.Value);
						}

						if (upper.HasValue)
						{
							if (rangeFilter.IncludesLower)
								query = query.Where(x => !x.AvailableStartDateTimeUtc.HasValue || x.AvailableStartDateTimeUtc <= upper.Value);
							else
								query = query.Where(x => !x.AvailableStartDateTimeUtc.HasValue || x.AvailableStartDateTimeUtc < upper.Value);
						}
					}
				}
				else if (filter.FieldName == "availableend")
				{
					if (rangeFilter != null)
					{
						var lower = filter.Term as DateTime?;
						var upper = rangeFilter.UpperTerm as DateTime?;

						if (lower.HasValue)
						{
							if (rangeFilter.IncludesLower)
								query = query.Where(x => !x.AvailableEndDateTimeUtc.HasValue || x.AvailableEndDateTimeUtc >= lower.Value);
							else
								query = query.Where(x => !x.AvailableEndDateTimeUtc.HasValue || x.AvailableEndDateTimeUtc > lower.Value);
						}

						if (upper.HasValue)
						{
							if (rangeFilter.IncludesLower)
								query = query.Where(x => !x.AvailableEndDateTimeUtc.HasValue || x.AvailableEndDateTimeUtc <= upper.Value);
							else
								query = query.Where(x => !x.AvailableEndDateTimeUtc.HasValue || x.AvailableEndDateTimeUtc < upper.Value);
						}
					}
				}
				else if (filter.FieldName == "visibleindividually")
				{
					query = query.Where(x => x.VisibleIndividually == (bool)filter.Term);
				}
				else if (filter.FieldName == "showonhomepage")
				{
					query = query.Where(p => p.ShowOnHomePage == (bool)filter.Term);
				}
				else if (filter.FieldName == "parentid")
				{
					query = query.Where(x => x.ParentGroupedProductId == (int)filter.Term);
				}
				else if (filter.FieldName == "typeid")
				{
					query = query.Where(x => x.ProductTypeId == (int)filter.Term);
				}
				else if (filter.FieldName == "stockquantity")
				{
					if (rangeFilter != null)
					{
						var lower = filter.Term as int?;
						var upper = rangeFilter.UpperTerm as int?;

						if (lower.HasValue)
						{
							if (rangeFilter.IncludesLower)
								query = query.Where(x => x.StockQuantity >= lower.Value);
							else
								query = query.Where(x => x.StockQuantity > lower.Value);
						}

						if (upper.HasValue)
						{
							if (rangeFilter.IncludesUpper)
								query = query.Where(x => x.StockQuantity <= upper.Value);
							else
								query = query.Where(x => x.StockQuantity < upper.Value);
						}
					}
				}
				else if (filter.FieldName.StartsWith("price"))
				{
					if (rangeFilter != null)
					{
						var lower = filter.Term as double?;
						var upper = rangeFilter.UpperTerm as double?;

						if (lower.HasValue)
						{
							var minPrice = Convert.ToDecimal(lower.Value);

							query = query.Where(x =>
								((x.SpecialPrice.HasValue &&
								((!x.SpecialPriceStartDateTimeUtc.HasValue || x.SpecialPriceStartDateTimeUtc.Value < utcNow) &&
								(!x.SpecialPriceEndDateTimeUtc.HasValue || x.SpecialPriceEndDateTimeUtc.Value > utcNow))) &&
								(x.SpecialPrice >= minPrice))
								||
								((!x.SpecialPrice.HasValue ||
								((x.SpecialPriceStartDateTimeUtc.HasValue && x.SpecialPriceStartDateTimeUtc.Value > utcNow) ||
								(x.SpecialPriceEndDateTimeUtc.HasValue && x.SpecialPriceEndDateTimeUtc.Value < utcNow))) &&
								(x.Price >= minPrice))
							);
						}

						if (upper.HasValue)
						{
							var maxPrice = Convert.ToDecimal(upper);

							query = query.Where(x =>
								((x.SpecialPrice.HasValue &&
								((!x.SpecialPriceStartDateTimeUtc.HasValue || x.SpecialPriceStartDateTimeUtc.Value < utcNow) &&
								(!x.SpecialPriceEndDateTimeUtc.HasValue || x.SpecialPriceEndDateTimeUtc.Value > utcNow))) &&
								(x.SpecialPrice <= maxPrice))
								||
								((!x.SpecialPrice.HasValue ||
								((x.SpecialPriceStartDateTimeUtc.HasValue && x.SpecialPriceStartDateTimeUtc.Value > utcNow) ||
								(x.SpecialPriceEndDateTimeUtc.HasValue && x.SpecialPriceEndDateTimeUtc.Value < utcNow))) &&
								(x.Price <= maxPrice))
							);
						}
					}
				}
				else if (filter.FieldName == "createdon")
				{
					if (rangeFilter != null)
					{
						var lower = filter.Term as DateTime?;
						var upper = rangeFilter.UpperTerm as DateTime?;

						if (lower.HasValue)
						{
							if (rangeFilter.IncludesLower)
								query = query.Where(x => x.CreatedOnUtc >= lower.Value);
							else
								query = query.Where(x => x.CreatedOnUtc > lower.Value);
						}

						if (upper.HasValue)
						{
							if (rangeFilter.IncludesLower)
								query = query.Where(x => x.CreatedOnUtc <= upper.Value);
							else
								query = query.Where(x => x.CreatedOnUtc < upper.Value);
						}
					}
				}
				else if (filter.FieldName == "storeid")
				{
					if (!QuerySettings.IgnoreMultiStore)
					{
						var storeId = (int)filter.Term;
						if (storeId != 0)
						{
							query =
								from p in query
								join sm in _storeMappingRepository.Table on new { pid = p.Id, pname = "Product" } equals new { pid = sm.EntityId, pname = sm.EntityName } into psm
								from sm in psm.DefaultIfEmpty()
								where !p.LimitedToStores || sm.StoreId == storeId
								select p;
						}
					}
				}
			}

			#endregion

			#region Sorting

			foreach (var sort in searchQuery.Sorting)
			{
				if (sort.FieldName.IsEmpty())
				{
					// sort by relevance
					if (categoryIds.Any())
					{
						var categoryId = categoryIds.First();
						query = OrderBy(ref ordered, query, x => x.ProductCategories.Where(pc => pc.CategoryId == categoryId).FirstOrDefault().DisplayOrder);
					}
					else if (manufacturerIds.Any())
					{
						var manufacturerId = manufacturerIds.First();
						query = OrderBy(ref ordered, query, x => x.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId).FirstOrDefault().DisplayOrder);
					}
					else if (searchQuery.Filters.OfType<IAttributeSearchFilter>().Any(x => x.FieldName == "parentid"))
					{
						query = OrderBy(ref ordered, query, x => x.DisplayOrder);
					}
					else
					{
						query = OrderBy(ref ordered, query, x => x.Name);
					}
				}
				else if (sort.FieldName == "createdon")
				{
					query = OrderBy(ref ordered, query, x => x.CreatedOnUtc, sort.Descending);
				}
				else if (sort.FieldName == "name")
				{
					query = OrderBy(ref ordered, query, x => x.Name, sort.Descending);
				}
				else if (sort.FieldName == "price")
				{
					query = OrderBy(ref ordered, query, x => x.Price, sort.Descending);
				}
				else
				{
					query = OrderBy(ref ordered, query, x => x.Name);
				}
			}

			if (!ordered)
			{
				query = query.OrderBy(x => x.Id);
			}

			#endregion

			return query;
		}

		#endregion

		public CatalogSearchResult Search(CatalogSearchQuery searchQuery, ProductLoadFlags loadFlags = ProductLoadFlags.None, bool direct = false)
		{
			_eventPublisher.Publish(new CatalogSearchingEvent(searchQuery));

			int totalCount = 0;
			Func<IList<Product>> hitsFactory = null;

			if (searchQuery.Take > 0)
			{
				var query = GetProductQuery(searchQuery);

				totalCount = query.Count();

				query = query
					.Skip(searchQuery.PageIndex * searchQuery.Take)
					.Take(searchQuery.Take);

				var ids = query.Select(x => x.Id).ToArray();
				hitsFactory = () => _productService.GetProductsByIds(ids, loadFlags);
			}

			var result = new CatalogSearchResult(
				null,
				totalCount,
				hitsFactory,
				searchQuery,
				null,
				null,
				null);

			_eventPublisher.Publish(new CatalogSearchedEvent(searchQuery, result));

			return result;
		}
	}
}
