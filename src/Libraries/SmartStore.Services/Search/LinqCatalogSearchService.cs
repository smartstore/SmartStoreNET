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
using SmartStore.Core.Search;

namespace SmartStore.Services.Search
{
	public partial class LinqCatalogSearchService : ICatalogSearchService
	{
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IRepository<AclRecord> _aclRepository;
		private readonly ICommonServices _services;

		public LinqCatalogSearchService(
			IRepository<Product> productRepository,
			IRepository<LocalizedProperty> localizedPropertyRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IRepository<AclRecord> aclRepository,
			ICommonServices services)
		{
			_productRepository = productRepository;
			_localizedPropertyRepository = localizedPropertyRepository;
			_storeMappingRepository = storeMappingRepository;
			_aclRepository = aclRepository;
			_services = services;

			QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

		#region Utilities

		private List<int> GetIdList(CatalogSearchQuery searchQuery, string fieldName)
		{
			return searchQuery.Filters
				.OfType<ITermSearchFilter>()
				.Where(x => !(x is IRangeSearchFilter) && x.FieldName == fieldName)
				.Select(x => (int)x.Term)
				.ToList();
		}

		private IOrderedQueryable<Product> OrderBy<TKey>(IQueryable<Product> query, Expression<Func<Product, TKey>> keySelector, bool descending = false)
		{
			var ordered = query as IOrderedQueryable<Product>;

			if (ordered == null)
			{
				if (descending)
					return query.OrderByDescending(keySelector);

				return query.OrderBy(keySelector);
			}

			if (descending)
				return ordered.ThenByDescending(keySelector);

			return ordered.ThenBy(keySelector);
		}

		private IQueryable<Product> QueryLocalizedProperties(IQueryable<Product> query, string keyGroup, string key, int languageId, string term)
		{
			if (languageId != 0)
			{
				query =
					from p in query
					join lp in _localizedPropertyRepository.Table on p.Id equals lp.EntityId into plp
					from lp in plp.DefaultIfEmpty()
					where lp.LanguageId == languageId && lp.LocaleKeyGroup == keyGroup && lp.LocaleKey == key && lp.LocaleValue.Contains(term)
					select p;
			}

			return query;
		}

		protected virtual IQueryable<Product> GetProducts(CatalogSearchQuery searchQuery)
		{
			var utcNow = DateTime.UtcNow;
			var term = searchQuery.Term;
			var languageId = searchQuery.LanguageId ?? 0;

			var query = _productRepository.Table
				.Where(x => !x.Deleted);

			#region Search Term

			if (term.HasValue() && searchQuery.Fields != null && searchQuery.Fields.Length != 0 && searchQuery.Fields.Any(x => x.HasValue()))
			{
				foreach (var field in searchQuery.Fields)
				{
					if (field == "sku")
					{
						query = query.Where(x => x.Sku.Contains(term));
					}
					else if (field == "name")
					{
						query = query.Where(x => x.Name.Contains(term));

						query = QueryLocalizedProperties(query, "Product", "Name", languageId, term);
					}
					else if (field == "shortdescription")
					{
						query = query.Where(x => x.ShortDescription.Contains(term));

						query = QueryLocalizedProperties(query, "Product", "ShortDescription", languageId, term);
					}
					else if (field == "fulldescription")
					{
						query = query.Where(x => x.FullDescription.Contains(term));

						query = QueryLocalizedProperties(query, "Product", "FullDescription", languageId, term);
					}
					else if (field == "tagname")
					{
						query =
							from p in query
							from pt in p.ProductTags.DefaultIfEmpty()
							where pt.Name.Contains(term)
							select p;

						query = QueryLocalizedProperties(query, "ProductTag", "Name", languageId, term);
					}
				}
			}

			#endregion

			#region Filters

			if (!QuerySettings.IgnoreAcl)
			{
				var roleIds = GetIdList(searchQuery, "roleid");
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

			var productIds = GetIdList(searchQuery, "id");
			if (productIds.Any())
			{
				query = query.Where(x => productIds.Contains(x.Id));
			}

			var termFilters = searchQuery.Filters.OfType<ITermSearchFilter>().ToList();

			// TODO: HasAnyCategories, ProductCategories.Count
			var categoryIds = GetIdList(searchQuery, "categoryid");
			if (categoryIds.Any())
			{
				var featuredOnly = termFilters.FirstOrDefault(x => x.FieldName == "featured")?.Term as bool?;

				query =
					from p in query
					from pm in p.ProductManufacturers.Where(pm => categoryIds.Contains(pm.ManufacturerId))
					where (!featuredOnly.HasValue || featuredOnly.Value == pm.IsFeaturedProduct)
					select p;
			}

			// TODO: HasAnyManufacturers, ProductManufacturers.Count
			var manufacturerIds = GetIdList(searchQuery, "manufacturerid");
			if (manufacturerIds.Any())
			{
				var featuredOnly = termFilters.FirstOrDefault(x => x.FieldName == "featured")?.Term as bool?;

				query =
					from p in query
					from pm in p.ProductManufacturers.Where(pm => manufacturerIds.Contains(pm.ManufacturerId))
					where (!featuredOnly.HasValue || featuredOnly.Value == pm.IsFeaturedProduct)
					select p;
			}

			var tagIds = GetIdList(searchQuery, "tagid");
			if (tagIds.Any())
			{
				query =
					from p in query
					from pt in p.ProductTags.Where(pt => tagIds.Contains(pt.Id))
					select p;
			}

			foreach (var filter in termFilters)
			{
				// TODO: (mg) what about ICombinedSearchFilter here?

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
				else if (filter.FieldName == "price")
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
						query =
							from p in query
							join sm in _storeMappingRepository.Table on new { pid = p.Id, pname = "Product" } equals new { pid = sm.EntityId, pname = sm.EntityName } into psm
							from sm in psm.DefaultIfEmpty()
							where !p.LimitedToStores || sm.StoreId == (int)filter.Term
							select p;
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
						query = OrderBy(query, x => x.ProductCategories.Where(pc => pc.CategoryId == categoryId).FirstOrDefault().DisplayOrder);
					}
					else if (manufacturerIds.Any())
					{
						var manufacturerId = manufacturerIds.First();
						query = OrderBy(query, x => x.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId).FirstOrDefault().DisplayOrder);
					}
					else if (searchQuery.Filters.OfType<ITermSearchFilter>().Any(x => x.FieldName == "parentid"))
					{
						query = OrderBy(query, x => x.DisplayOrder);
					}
					else
					{
						query = OrderBy(query, x => x.Name);
					}
				}
				else if (sort.FieldName == "createdon")
				{
					query = OrderBy(query, x => x.CreatedOnUtc, sort.Descending);
				}
				else if (sort.FieldName == "name")
				{
					query = OrderBy(query, x => x.Name, sort.Descending);
				}
				else if (sort.FieldName == "price")
				{
					query = OrderBy(query, x => x.Price, sort.Descending);
				}
				else
				{
					query = OrderBy(query, x => x.Name);
				}
			}

			if ((query as IOrderedQueryable<Product>) == null)
			{
				query = query.OrderBy(x => x.Id);
			}

			#endregion

			#region Paging

			if (searchQuery.Skip > 0)
			{
				query = query.Skip(searchQuery.Skip);
			}

			if (searchQuery.Take != int.MaxValue)
			{
				query = query.Take(searchQuery.Take);
			}

			#endregion

			return query;
		}

		#endregion

		public CatalogSearchResult Search(CatalogSearchQuery searchQuery)
		{
			var productQuery = GetProducts(searchQuery);
			var hits = new PagedList<Product>(productQuery, searchQuery.PageIndex, searchQuery.Take);

			return new CatalogSearchResult(hits, searchQuery, null);
		}
	}
}
