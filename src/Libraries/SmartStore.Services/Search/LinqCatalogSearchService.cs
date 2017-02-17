using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;

namespace SmartStore.Services.Search
{
	public partial class LinqCatalogSearchService : ICatalogSearchService
	{
		private readonly IProductService _productService;
		private readonly IRepository<Product> _productRepository;
		private readonly IRepository<ProductManufacturer> _productManufacturerRepository;
		private readonly IRepository<ProductCategory> _productCategoryRepository;
		private readonly IRepository<Manufacturer> _manufacturerRepository;
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IRepository<AclRecord> _aclRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly ICommonServices _services;
		private readonly IDeliveryTimeService _deliveryTimeService;

		public LinqCatalogSearchService(
			IProductService productService,
			IRepository<Product> productRepository,
			IRepository<ProductManufacturer> productManufacturerRepository,
			IRepository<ProductCategory> productCategoryRepository,
			IRepository<Manufacturer> manufacturerRepository,	
			IRepository<Category> categoryRepository,
			IRepository<LocalizedProperty> localizedPropertyRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IRepository<AclRecord> aclRepository,
			IEventPublisher eventPublisher,
			ICommonServices services,
			IDeliveryTimeService deliveryTimeService)
		{
			_productService = productService;
			_productRepository = productRepository;
			_productManufacturerRepository = productManufacturerRepository;
			_productCategoryRepository = productCategoryRepository;
			_manufacturerRepository = manufacturerRepository;
			_categoryRepository = categoryRepository;
			_localizedPropertyRepository = localizedPropertyRepository;
			_storeMappingRepository = storeMappingRepository;
			_aclRepository = aclRepository;
			_eventPublisher = eventPublisher;
			_services = services;
			_deliveryTimeService = deliveryTimeService;

			QuerySettings = DbQuerySettings.Default;
			Logger = NullLogger.Instance;
		}

		public DbQuerySettings QuerySettings { get; set; }

		public ILogger Logger { get; set; }

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

		private Facet CreateFacet(FacetDescriptor descriptor, object value, IndexTypeCode typeCode, string label, int displayOrder)
		{
			var newValue = new FacetValue(value, typeCode)
			{
				IsSelected = descriptor.Values.Any(x => x.IsSelected && x.Value.Equals(value)),
				Label = label,
				DisplayOrder = displayOrder
			};

			return new Facet(newValue);
		}

		protected virtual IQueryable<Product> ApplySearchTerm(IQueryable<Product> query, CatalogSearchQuery searchQuery)
		{
			var term = searchQuery.Term;
			var fields = searchQuery.Fields;
			var languageId = searchQuery.LanguageId ?? 0;

			if (term.HasValue() && fields != null && fields.Length != 0 && fields.Any(x => x.HasValue()))
			{
				// SearchMode.ExactMatch doesn't make sense here
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
				else
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
			}

			return query;
		}

		protected virtual IQueryable<Product> GetProductQuery(CatalogSearchQuery searchQuery)
		{
			var ordered = false;
			var utcNow = DateTime.UtcNow;

			var query = _productRepository.Table.Where(x => !x.Deleted);

			query = ApplySearchTerm(query, searchQuery);

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

			query = query.GroupBy(x => x.Id).Select(x => x.FirstOrDefault());

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

		protected virtual IDictionary<string, FacetGroup> GetFacets(CatalogSearchQuery searchQuery)
		{
			var result = new Dictionary<string, FacetGroup>();
			var languageId = searchQuery.LanguageId ?? _services.WorkContext.WorkingLanguage.Id;

			foreach (var key in searchQuery.FacetDescriptors.Keys)
			{
				var descriptor = searchQuery.FacetDescriptors[key];
				var facets = new List<Facet>();

				if (key == "categoryid" || key == "featuredcategoryid" || key == "notfeaturedcategoryid")
				{
					#region categories

					// order by product count
					var categoryQuery =
						from c in _categoryRepository.TableUntracked
						where !c.Deleted && c.Published
						join pc in _productCategoryRepository.TableUntracked on c.Id equals pc.CategoryId into pcm
						from pc in pcm.DefaultIfEmpty()
						group c by c.Id into grp
						orderby grp.Count() descending
						select new
						{
							Id = grp.FirstOrDefault().Id,
							Name = grp.FirstOrDefault().Name,
							DisplayOrder = grp.FirstOrDefault().DisplayOrder
						};

					if (descriptor.MaxChoicesCount > 0)
					{
						categoryQuery = categoryQuery.Take(descriptor.MaxChoicesCount);
					}

					var categories = categoryQuery.ToList();

					var nameQuery = _localizedPropertyRepository.TableUntracked
						.Where(x => x.LocaleKeyGroup == "Category" && x.LocaleKey == "Name" && x.LanguageId == languageId);

					var names = nameQuery.ToList().ToDictionarySafe(x => x.EntityId, x => x.LocaleValue);

					foreach (var category in categories)
					{
						string label = null;
						names.TryGetValue(category.Id, out label);

						facets.Add(CreateFacet(descriptor, category.Id, IndexTypeCode.Int32, label.HasValue() ? label : category.Name, category.DisplayOrder));
					}

					#endregion
				}
				else if (key == "manufacturerid")
				{
					#region manufacturers

					// order by product count
					var manufacturerQuery =
						from m in _manufacturerRepository.TableUntracked
						where !m.Deleted && m.Published
						join pm in _productManufacturerRepository.TableUntracked on m.Id equals pm.ManufacturerId into pmm
						from pm in pmm.DefaultIfEmpty()
						group m by m.Id into grp
						orderby grp.Count() descending
						select new
						{
							Id = grp.FirstOrDefault().Id,
							Name = grp.FirstOrDefault().Name,
							DisplayOrder = grp.FirstOrDefault().DisplayOrder
						};

					if (descriptor.MaxChoicesCount > 0)
					{
						manufacturerQuery = manufacturerQuery.Take(descriptor.MaxChoicesCount);
					}

					var manufacturers = manufacturerQuery.ToList();

					var nameQuery = _localizedPropertyRepository.TableUntracked
						.Where(x => x.LocaleKeyGroup == "Manufacturer" && x.LocaleKey == "Name" && x.LanguageId == languageId);

					var names = nameQuery.ToList().ToDictionarySafe(x => x.EntityId, x => x.LocaleValue);

					foreach (var manu in manufacturers)
					{
						string label = null;
						names.TryGetValue(manu.Id, out label);

						facets.Add(CreateFacet(descriptor, manu.Id, IndexTypeCode.Int32, label.HasValue() ? label : manu.Name, manu.DisplayOrder));
					}

					#endregion
				}
				else if (key == "rate")
				{
					#region ratings

					var count = 0;
					for (double rate = 1.0; rate <= 5.0; ++rate)
					{
						facets.Add(CreateFacet(descriptor, rate, IndexTypeCode.Double, rate.ToString(), ++count));
					}

					#endregion
				}
				else if (key == "deliveryid")
				{
					#region delivery times

					var deliveryTimes = _deliveryTimeService.GetAllDeliveryTimes();

					var nameQuery = _localizedPropertyRepository.TableUntracked
						.Where(x => x.LocaleKeyGroup == "DeliveryTime" && x.LocaleKey == "Name" && x.LanguageId == languageId);

					var names = nameQuery.ToList().ToDictionarySafe(x => x.EntityId, x => x.LocaleValue);

					foreach (var deliveryTime in deliveryTimes)
					{
						string label = null;
						names.TryGetValue(deliveryTime.Id, out label);

						facets.Add(CreateFacet(descriptor, deliveryTime.Id, IndexTypeCode.Int32, label.HasValue() ? label : deliveryTime.Name, deliveryTime.DisplayOrder));
					}

					#endregion
				}
				else if (key == "price")
				{
					#region prices

					// provide 'up to' price suggestions
					List<double> existingPrices = null;

					try
					{
						var productQuery = ApplySearchTerm(_productRepository.TableUntracked.Where(x => !x.Deleted), searchQuery);
						existingPrices = productQuery
							.Select(x => (double)Decimal.Round(x.Price))
							.Where(x => x != 0.0)
							.Distinct()
							.ToList();
					}
					catch (Exception exception)
					{
						Logger.Error(exception);
					}

					if (existingPrices != null && existingPrices.Any())
					{
						var count = 0;
						var maxPrice = existingPrices.Max();

						for (double price = GetNextMaxPrice(0.0);
							price < double.MaxValue;
							price = GetNextMaxPrice(price))
						{
							if ((descriptor.MaxChoicesCount > 0 && count >= descriptor.MaxChoicesCount) || price > maxPrice)
								break;

							if (!existingPrices.Any(x => x < price))
								continue;

							var newValue = new FacetValue(null, price, IndexTypeCode.Double, false, true)
							{
								IsSelected = descriptor.Values.Any(x => x.IsSelected && x.UpperValue != null && (double)x.UpperValue == price),
								Label = price.ToString(),
								DisplayOrder = ++count
							};

							facets.Add(new Facet(newValue));
						}
					}

					// remove too granular price ranges
					if (facets.Count > 3 && facets.Any(x => x.Value.UpperValue != null && (double)x.Value.UpperValue == 25.0))
					{
						facets.RemoveFacet(5.0, true);
						facets.RemoveFacet(10.0, true);
					}

					// add facet for individual price filter
					if (descriptor.Values.Any() && !facets.Any(x => x.Value.IsSelected))
					{
						var individualPrice = descriptor.Values.First();

						// check if price facet already exists otherwise insert it
						var priceExists = (!individualPrice.IncludesLower && individualPrice.IncludesUpper && facets.Any(x => x.Value.Equals(individualPrice)));
						if (!priceExists)
						{
							facets.Insert(0, new Facet(new FacetValue(individualPrice)));
						}
					}

					#endregion
				}

				if (!facets.Any())
					continue;

				//facets.Each(x => $"{key} {x.Value.ToString()}".Dump());

				result.Add(key, new FacetGroup(
					key,
					descriptor.Label,
					descriptor.IsMultiSelect,
					descriptor.DisplayOrder,
					facets.OrderBy(descriptor)));
			}
			
			return result;
		}

		protected static double GetNextMaxPrice(double price)
		{
			if (price < 10)
				return price + 5;
			if (price < 25)
				return price + 15;
			if (price < 200)
				return price + 25;
			if (price < 500)
				return price + 50;
			if (price < 1000)
				return price + 100;
			if (price < 2000)
				return price + 250;
			if (price < 5000)
				return price + 500;
			if (price < 10000)
				return price + 1000;
			if (price < 20000)
				return price + 2500;
			if (price < 50000)
				return price + 5000;
			if (price < 100000)
				return price + 10000;
			if (price < 200000)
				return price + 25000;
			if (price < 500000)
				return price + 50000;
			if (price < 1000000)
				return price + 100000;
			if (price < 2000000)
				return price + 250000;
			if (price < 5000000)
				return price + 500000;
			if (price < 10000000)
				return price + 1000000;
			if (price < 20000000)
				return price + 2500000;
			if (price < 50000000)
				return price + 5000000;
			return double.MaxValue;
		}

		#endregion

		public CatalogSearchResult Search(CatalogSearchQuery searchQuery, ProductLoadFlags loadFlags = ProductLoadFlags.None, bool direct = false)
		{
			_eventPublisher.Publish(new CatalogSearchingEvent(searchQuery));

			int totalCount = 0;
			Func<IList<Product>> hitsFactory = null;
			IDictionary<string, FacetGroup> facets = null;

			if (searchQuery.Take > 0)
			{
				var query = GetProductQuery(searchQuery);

				totalCount = query.Count();

				query = query
					.Skip(searchQuery.PageIndex * searchQuery.Take)
					.Take(searchQuery.Take);

				var ids = query.Select(x => x.Id).ToArray();
				hitsFactory = () => _productService.GetProductsByIds(ids, loadFlags);

				if (totalCount > 0 && searchQuery.FacetDescriptors.Any())
				{
					facets = GetFacets(searchQuery);
				}
			}

			var result = new CatalogSearchResult(
				null,
				searchQuery,
				totalCount,
				hitsFactory,
				null,
				facets);

			_eventPublisher.Publish(new CatalogSearchedEvent(searchQuery, result));

			return result;
		}
	}
}
