using System;
using System.Collections.Generic;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Polls;
using SmartStore.Core.Domain.Topics;
using SmartStore.Utilities;

namespace SmartStore.Web.Infrastructure.Cache
{
	public partial class ModelCacheEventConsumer : IDbSaveHook
	{
		/// <summary>
		/// Key for ManufacturerNavigationModel caching
		/// </summary>
		/// <remarks>
		/// {0} : value indicating whether a default picture is displayed in case if no real picture exists
		/// {1} : language id
		/// {2} : current store ID
		/// {3} : items to display
		/// </remarks>
		public const string MANUFACTURER_NAVIGATION_MODEL_KEY = "pres:manufacturer:navigation-{0}-{1}-{2}-{3}";
		public const string MANUFACTURER_NAVIGATION_PATTERN_KEY = "pres:manufacturer:navigation*";

		/// <summary>
		/// Indicates whether a manufacturer has featured products
		/// </summary>
		/// <remarks>
		/// {0} : manufacturer id
		/// {1} : roles of the current user
		/// {2} : current store ID
		/// </remarks>
		public const string MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY = "pres:manufacturer:hasfeaturedproducts-{0}-{1}-{2}";
		public const string MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY = "pres:manufacturer:hasfeaturedproducts*";

		/// <summary>
		/// Indicates whether a category has featured products
		/// </summary>
		/// <remarks>
		/// {0} : category id
		/// {1} : roles of the current user
		/// {2} : current store ID
		/// </remarks>
		public const string CATEGORY_HAS_FEATURED_PRODUCTS_KEY = "pres:category:hasfeaturedproducts-{0}-{1}-{2}";
		public const string CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY = "pres:category:hasfeaturedproducts*";

		/// <summary>
		/// Key for ProductTagModel caching
		/// </summary>
		/// <remarks>
		/// {0} : product id
		/// {1} : language id
		/// {2} : current store ID
		/// </remarks>
		public const string PRODUCTTAG_BY_PRODUCT_MODEL_KEY = "pres:producttag:byproduct-{0}-{1}-{2}";
		public const string PRODUCTTAG_BY_PRODUCT_PATTERN_KEY = "pres:producttag:byproduct*";

		/// <summary>
		/// Key for PopularProductTagsModel caching
		/// </summary>
		/// <remarks>
		/// {0} : language id
		/// {1} : current store ID
		/// </remarks>
		public const string PRODUCTTAG_POPULAR_MODEL_KEY = "pres:producttag:popular-{0}-{1}";
		public const string PRODUCTTAG_POPULAR_PATTERN_KEY = "pres:producttag:popular*";

		/// <summary>
		/// Key for ProductManufacturers model caching
		/// </summary>
		/// <remarks>
		/// {0} : product id
		/// {1} : value indicating whether a default picture is displayed in case if no real picture exists
		/// {2} : language id
		/// {3} : current store ID
		/// </remarks>
		public const string PRODUCT_MANUFACTURERS_MODEL_KEY = "pres:product:manufacturers-{0}-{1}-{2}-{3}";
		public const string PRODUCT_MANUFACTURERS_PATTERN_KEY = "pres:product:manufacturers*";

		/// <summary>
		/// Key for ProductSpecificationModel caching
		/// </summary>
		/// <remarks>
		/// {0} : product id
		/// {1} : language id
		/// </remarks>
		public const string PRODUCT_SPECS_MODEL_KEY = "pres:product:specs-{0}-{1}";
		public const string PRODUCT_SPECS_PATTERN_KEY = "pres:product:specs*";

		/// <summary>
		/// Key for TopicModel caching
		/// </summary>
		/// <remarks>
		/// {0} : topic id/systemname
		/// {1} : language id
		/// {2} : store id
		/// </remarks>
		public const string TOPIC_BY_SYSTEMNAME_KEY = "pres:topic:page.bysystemname-{0}-{1}-{2}";
		public const string TOPIC_BY_ID_KEY = "pres:topic:page.byid-{0}-{1}-{2}";
		public const string TOPIC_PATTERN_KEY = "pres:topic:page*";

		/// <summary>
		/// Key for TopicWidget caching
		/// </summary>
		/// <remarks>
		/// {0} : store id
		/// {1} : language id
		/// </remarks>
		public const string TOPIC_WIDGET_ALL_MODEL_KEY = "pres:topic:widget-all-{0}-{1}";
		public const string TOPIC_WIDGET_PATTERN_KEY = "pres:topic:widget*";

		/// <summary>
		/// Key for CategoryTemplate caching
		/// </summary>
		/// <remarks>
		/// {0} : category template id
		/// </remarks>
		public const string CATEGORY_TEMPLATE_MODEL_KEY = "pres:categorytemplate-{0}";
		public const string CATEGORY_TEMPLATE_PATTERN_KEY = "pres:categorytemplate*";

		/// <summary>
		/// Key for ManufacturerTemplate caching
		/// </summary>
		/// <remarks>
		/// {0} : manufacturer template id
		/// </remarks>
		public const string MANUFACTURER_TEMPLATE_MODEL_KEY = "pres:manufacturertemplate-{0}";
		public const string MANUFACTURER_TEMPLATE_PATTERN_KEY = "pres:manufacturertemplate*";

		/// <summary>
		/// Key for ProductTemplate caching
		/// </summary>
		/// <remarks>
		/// {0} : product template id
		/// </remarks>
		public const string PRODUCT_TEMPLATE_MODEL_KEY = "pres:producttemplate-{0}";
		public const string PRODUCT_TEMPLATE_PATTERN_KEY = "pres:producttemplate*";

		/// <summary>
		/// Key for bestsellers identifiers displayed on the home page
		/// </summary>
		/// <remarks>
		/// {0} : current store id
		/// </remarks>
		public const string HOMEPAGE_BESTSELLERS_IDS_KEY = "pres:bestsellers:homepage-{0}";
		public const string HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY = "pres:bestsellers:homepage*";

		/// <summary>
		/// Key for "also purchased" product identifiers displayed on the product details page
		/// </summary>
		/// <remarks>
		/// {0} : current product id
		/// {1} : current store id
		/// </remarks>
		public const string PRODUCTS_ALSO_PURCHASED_IDS_KEY = "pres:alsopuchased-{0}-{1}";
		public const string PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY = "pres:alsopuchased*";

		/// <summary>
		/// Key for cart picture caching
		/// </summary>
		/// <remarks>
		/// {0} : product id
		/// {1} : product attribute combination id
		/// {2} : picture size
		/// {3} : value indicating whether a default picture is displayed in case if no real picture exists
		/// {4} : language ID ("alt" and "title" can depend on localized product name)
		/// {6} : current store ID
		/// </remarks>
		public const string CART_PICTURE_MODEL_KEY = "pres:cart:picture-{0}-{1}-{2}-{3}-{4}-{5}";
		public const string CART_PICTURE_PATTERN_KEY = "pres:cart:picture*";

		/// <summary>
		/// Key for home page polls
		/// </summary>
		/// <remarks>
		/// {0} : language ID
		/// {1} : current store ID
		/// </remarks>
		public const string HOMEPAGE_POLLS_MODEL_KEY = "pres:poll:homepage-{0}-{1}";
		/// <summary>
		/// Key for polls by system name
		/// </summary>
		/// <remarks>
		/// {0} : poll system name
		/// {1} : language ID
		/// {2} : current store ID
		/// </remarks>
		public const string POLL_BY_SYSTEMNAME_MODEL_KEY = "pres:poll:systemname-{0}-{1}-{2}";
		public const string POLLS_PATTERN_KEY = "pres:poll:*";

		/// <summary>
		/// Key for blog tag list model
		/// </summary>
		/// <remarks>
		/// {0} : language ID
		/// {1} : store ID
		/// </remarks>
		public const string BLOG_TAGS_MODEL_KEY = "pres:blog:tags-{0}-{1}";
		/// <summary>
		/// Key for blog archive (years, months) block model
		/// </summary>
		/// <remarks>
		/// {0} : language ID
		/// {1} : current store ID
		/// </remarks>
		public const string BLOG_MONTHS_MODEL_KEY = "pres:blog:months-{0}-{1}";
		public const string BLOG_PATTERN_KEY = "pres:blog:*";

		/// <summary>
		/// Key for home page news
		/// </summary>
		/// <remarks>
		/// {0} : language ID
		/// {1} : store ID
		/// </remarks>
		public const string HOMEPAGE_NEWSMODEL_KEY = "pres:news:homepage-{0}-{1}";
		public const string NEWS_PATTERN_KEY = "pres:news:*";

		/// <summary>
		/// Key for states by country id
		/// </summary>
		/// <remarks>
		/// {0} : country ID
		/// {1} : addEmptyStateIfRequired value
		/// {2} : language ID
		/// </remarks>
		public const string STATEPROVINCES_BY_COUNTRY_MODEL_KEY = "pres:stateprovinces:bycountry-{0}-{1}-{2}";
		public const string STATEPROVINCES_PATTERN_KEY = "pres:stateprovinces:*";

		/// <summary>
		/// Key for available languages
		/// </summary>
		/// <remarks>
		/// {0} : current store ID
		/// </remarks>
		public const string AVAILABLE_LANGUAGES_MODEL_KEY = "pres:languages:all-{0}";
		public const string AVAILABLE_LANGUAGES_PATTERN_KEY = "pres:languages:*";

		/// <summary>
		/// Key for available currencies
		/// </summary>
		/// <remarks>
		/// {0} : language ID
		/// {1} : current store ID
		/// </remarks>
		public const string AVAILABLE_CURRENCIES_MODEL_KEY = "pres:currencies:all-{0}-{1}";
		public const string AVAILABLE_CURRENCIES_PATTERN_KEY = "pres:currencies:*";

		private static readonly HashSet<string> _candidateSettingKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			TypeHelper.NameOf<CatalogSettings>(x => x.NumberOfProductTags, true),
			TypeHelper.NameOf<CatalogSettings>(x => x.ManufacturerItemsToDisplayOnHomepage, true),
			TypeHelper.NameOf<CatalogSettings>(x => x.NumberOfBestsellersOnHomepage, true),
			TypeHelper.NameOf<BlogSettings>(x => x.NumberOfTags, true),
			TypeHelper.NameOf<NewsSettings>(x => x.MainPageNewsCount, true)
		};

		private readonly ICacheManager _cacheManager;

		public ModelCacheEventConsumer(ICacheManager cacheManager)
		{
			_cacheManager = cacheManager;
		}

		public void OnBeforeSave(IHookedEntity entry)
		{
			throw new NotImplementedException();
		}

		public void OnBeforeSaveCompleted()
		{
		}

		public void OnAfterSave(IHookedEntity entry)
		{
			var state = entry.InitialState;
			var entity = entry.Entity;

			if (entity is Product && state == EntityState.Modified)
			{
				var deleted = ((Product)entity).Deleted;
				if (deleted)
				{
					_cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
					_cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
				}
				else
				{
					_cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
					_cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
					_cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
					_cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
					_cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
				}
			}
			else if (entity is Manufacturer)
			{
				_cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
				_cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
			}
			else if (entity is ProductManufacturer)
			{
				_cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
				_cacheManager.RemoveByPattern(MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
			}
			else if (entity is ProductCategory)
			{
				_cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
			}
			else if (entity is ProductTag)
			{
				_cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
				_cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
			}
			else if (entity is SpecificationAttribute && state != EntityState.Added)
			{
				_cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
			}
			else if (entity is SpecificationAttributeOption && state != EntityState.Added)
			{
				_cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
			}
			else if (entity is ProductSpecificationAttribute)
			{
				_cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
			}
			else if (entity is Topic)
			{
				_cacheManager.RemoveByPattern(TOPIC_WIDGET_PATTERN_KEY);
				if (state != EntityState.Added)
				{
					_cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
				}
			}
			else if (entity is CategoryTemplate)
			{
				_cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
			}
			else if (entity is ManufacturerTemplate)
			{
				_cacheManager.RemoveByPattern(MANUFACTURER_TEMPLATE_PATTERN_KEY);
			}
			else if (entity is ProductTemplate)
			{
				_cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
			}
			else if (entity is Currency)
			{
				_cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
			}
			else if (entity is Language)
			{
				// Clear all localizable models
				_cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
				_cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
				_cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
				_cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
				_cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
				_cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
				_cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
			}
			else if (entity is Order || entity is OrderItem)
			{
				_cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
				_cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
			}
			else if (entity is Picture)
			{
				_cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
			}
			else if (entity is Poll)
			{
				_cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
			}
			else if (entity is BlogPost)
			{
				_cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
			}
			else if (entity is NewsItem)
			{
				_cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
			}
			else if (entity is StateProvince)
			{
				_cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
			}
			else if (entity is Setting)
			{
				var setting = entity as Setting;
				if (_candidateSettingKeys.Contains(setting.Name))
				{
					// Clear models which depend on settings
					_cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY); // depends on CatalogSettings.NumberOfProductTags
					_cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY); // depends on CatalogSettings.ManufacturerItemsToDisplayOnHomepage
					_cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY); // depends on CatalogSettings.NumberOfBestsellersOnHomepage
					_cacheManager.RemoveByPattern(BLOG_PATTERN_KEY); // depends on BlogSettings.NumberOfTags
					_cacheManager.RemoveByPattern(NEWS_PATTERN_KEY); // depends on NewsSettings.MainPageNewsCount
				}
			}
			else
			{
				// Register as void hook for all other entity type/state combis
				throw new NotImplementedException();
			}
		}

		public void OnAfterSaveCompleted()
		{
		}
	}
}