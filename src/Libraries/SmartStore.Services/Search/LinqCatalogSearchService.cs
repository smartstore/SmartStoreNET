using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Search
{
	public partial class LinqCatalogSearchService : ILinqCatalogSearchService
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

		#endregion

		public virtual IQueryable<Product> GetProducts(CatalogSearchQuery searchQuery)
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
					if (field == "Sku")
					{
						query = query.Where(x => x.Sku.Contains(term));
					}
					else if (field == "Name")
					{
						query = query.Where(x => x.Name.Contains(term));

						query = QueryLocalizedProperties(query, "Product", "Name", languageId, term);
					}
					else if (field == "ShortDescription")
					{
						query = query.Where(x => x.ShortDescription.Contains(term));

						query = QueryLocalizedProperties(query, "Product", "ShortDescription", languageId, term);
					}
					else if (field == "FullDescription")
					{
						query = query.Where(x => x.FullDescription.Contains(term));

						query = QueryLocalizedProperties(query, "Product", "FullDescription", languageId, term);
					}
					else if (field == "ProductTags")
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

			var showHidden = (searchQuery.Filters.FirstOrDefault(x => x.FieldName == "ShowHidden")?.Term as bool?) ?? false;

			if (!showHidden)
			{
				query = query.Where(x =>
					(!x.AvailableStartDateTimeUtc.HasValue || x.AvailableStartDateTimeUtc.Value < utcNow) &&
					(!x.AvailableEndDateTimeUtc.HasValue || x.AvailableEndDateTimeUtc.Value > utcNow)
				);

				if (!searchQuery.Filters.Any(x => x.FieldName == "Published"))
				{
					query = query.Where(x => x.Published);
				}

				if (!QuerySettings.IgnoreAcl)
				{
					var allowedCustomerRolesIds = _services.WorkContext.CurrentCustomer.CustomerRoles.Where(cr => cr.Active).Select(cr => cr.Id).ToList();

					query =
						from p in query
						join acl in _aclRepository.Table on new { pid = p.Id, pname = "Product" } equals new { pid = acl.EntityId, pname = acl.EntityName } into pacl
						from acl in pacl.DefaultIfEmpty()
						where !p.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
						select p;
				}
			}

			foreach (var filter in searchQuery.Filters)
			{
				if (filter.FieldName == "Published")
				{
					query = query.Where(x => x.Published == (bool)filter.Term);
				}
				else if (filter.FieldName == "VisibleIndividually")
				{
					query = query.Where(x => x.VisibleIndividually == (bool)filter.Term);
				}
				else if (filter.FieldName == "HomePageProducts")
				{
					query = query.Where(p => p.ShowOnHomePage == (bool)filter.Term);
				}
				else if (filter.FieldName == "ParentGroupedProductId")
				{
					query = query.Where(x => x.ParentGroupedProductId == (int)filter.Term);
				}
				else if (filter.FieldName == "ProductTypeId")
				{
					query = query.Where(x => x.ProductTypeId == (int)filter.Term);
				}
				else if (filter.FieldName == "StockQuantity")
				{
					if (filter.IsRangeFilter)
					{
						if (filter.IncludesLower)
							query = query.Where(x => x.StockQuantity >= (int)filter.Term);

						if (filter.IncludesUpper)
							query = query.Where(x => x.StockQuantity <= (int)filter.UpperTerm);
					}
				}
				else if (filter.FieldName == "Price")
				{
					if (filter.IsRangeFilter)
					{
						if (filter.IncludesLower)
						{
							var minPrice = Convert.ToDecimal(filter.Term);

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

						if (filter.IncludesUpper)
						{
							var maxPrice = Convert.ToDecimal(filter.UpperTerm);

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
				else if (filter.FieldName == "CreatedOnUtc")
				{
					if (filter.IsRangeFilter)
					{
						if (filter.IncludesLower)
							query = query.Where(x => x.CreatedOnUtc >= (DateTime)filter.Term);

						if (filter.IncludesUpper)
							query = query.Where(x => x.CreatedOnUtc <= (DateTime)filter.UpperTerm);
					}
				}
				else if (filter.FieldName == "Ids")
				{
					var ids = ((string)filter.Term).ToIntArray().ToList();

					query = query.Where(x => ids.Contains(x.Id));
				}
				else if (filter.FieldName == "Id")
				{
					if (filter.IsRangeFilter)
					{
						if (filter.IncludesLower)
							query = query.Where(x => x.Id >= (int)filter.Term);

						if (filter.IncludesUpper)
							query = query.Where(x => x.Id <= (int)filter.UpperTerm);
					}
					else
					{
						query = query.Where(x => x.Id == (int)filter.Term);
					}
				}
				else if (filter.FieldName == "CategoryIds")
				{
					var ids = ((string)filter.Term).ToIntArray().ToList();
					var isFeaturedProduct = searchQuery.Filters.FirstOrDefault(x => x.FieldName == "IsFeaturedProduct")?.Term as bool?;

					query =
						from p in query
						from pc in p.ProductCategories.Where(pc => ids.Contains(pc.CategoryId))
						where (!isFeaturedProduct.HasValue || isFeaturedProduct.Value == pc.IsFeaturedProduct)
						select p;
				}
				else if (filter.FieldName == "WithoutCategories")
				{
					if ((bool)filter.Term)
						query = query.Where(x => x.ProductCategories.Count == 0);
					else
						query = query.Where(x => x.ProductCategories.Count > 0);
				}
				else if (filter.FieldName == "ManufacturerIds")
				{
					var ids = ((string)filter.Term).ToIntArray().ToList();
					var isFeaturedProduct = searchQuery.Filters.FirstOrDefault(x => x.FieldName == "IsFeaturedProduct")?.Term as bool?;

					query =
						from p in query
						from pm in p.ProductManufacturers.Where(pm => ids.Contains(pm.ManufacturerId))
						where (!isFeaturedProduct.HasValue || isFeaturedProduct.Value == pm.IsFeaturedProduct)
						select p;
				}
				else if (filter.FieldName == "WithoutManufacturers")
				{
					if ((bool)filter.Term)
						query = query.Where(x => x.ProductManufacturers.Count == 0);
					else
						query = query.Where(x => x.ProductManufacturers.Count > 0);
				}
				else if (filter.FieldName == "ProductTagIds")
				{
					var ids = ((string)filter.Term).ToIntArray().ToList();

					query =
						from p in query
						from pt in p.ProductTags.Where(pt => ids.Contains(pt.Id))
						select p;
				}
				else if (filter.FieldName == "StoreId")
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
					if (searchQuery.Filters.Any(x => x.FieldName == "CategoryIds"))
					{
						var categoryIds = searchQuery.Filters.First(x => x.FieldName == "CategoryIds").Term as List<int>;
						var categoryId = categoryIds.First();

						query = OrderBy(query, x => x.ProductCategories.Where(pc => pc.CategoryId == categoryId).FirstOrDefault().DisplayOrder);
					}
					else if (searchQuery.Filters.Any(x => x.FieldName == "ManufacturerId"))
					{
						var manufacturerId = (int)searchQuery.Filters.First(x => x.FieldName == "ManufacturerId").Term;

						query = OrderBy(query, x => x.ProductManufacturers.Where(pm => pm.ManufacturerId == manufacturerId).FirstOrDefault().DisplayOrder);
					}
					else if (searchQuery.Filters.Any(x => x.FieldName == "ParentGroupedProductId"))
					{
						query = OrderBy(query, x => x.DisplayOrder);
					}
					else
					{
						query = OrderBy(query, x => x.Name);
					}
				}
				else if (sort.FieldName == "CreatedOnUtc")
				{
					query = OrderBy(query, x => x.CreatedOnUtc, sort.Descending);
				}
				else if (sort.FieldName == "Name")
				{
					query = OrderBy(query, x => x.Name, sort.Descending);
				}
				else if (sort.FieldName == "Price")
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
	}
}
