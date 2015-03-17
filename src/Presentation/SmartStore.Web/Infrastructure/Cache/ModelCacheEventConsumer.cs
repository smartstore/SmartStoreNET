﻿using System;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Configuration;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Polls;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Events;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Infrastructure.Cache
{
    /// <summary>
	/// Model cache event consumer (used for caching of presentation layer models)
    /// </summary>
	public partial class ModelCacheEventConsumer: 
        //languages
        IConsumer<EntityInserted<Language>>,
        IConsumer<EntityUpdated<Language>>,
        IConsumer<EntityDeleted<Language>>,
        //currencies
        IConsumer<EntityInserted<Currency>>,
        IConsumer<EntityUpdated<Currency>>,
        IConsumer<EntityDeleted<Currency>>,
        //settings
        IConsumer<EntityUpdated<Setting>>,
        //manufacturers
        IConsumer<EntityInserted<Manufacturer>>,
        IConsumer<EntityUpdated<Manufacturer>>,
        IConsumer<EntityDeleted<Manufacturer>>,
        //product manufacturers
        IConsumer<EntityInserted<ProductManufacturer>>,
        IConsumer<EntityUpdated<ProductManufacturer>>,
        IConsumer<EntityDeleted<ProductManufacturer>>,
        //categories
        IConsumer<EntityInserted<Category>>, // codehint: sm-add
        IConsumer<EntityUpdated<Category>>,
        IConsumer<EntityDeleted<Category>>,
        //product categories
        IConsumer<EntityInserted<ProductCategory>>,
        IConsumer<EntityUpdated<ProductCategory>>,
        IConsumer<EntityDeleted<ProductCategory>>,
        //products
        IConsumer<EntityInserted<Product>>,
        IConsumer<EntityUpdated<Product>>,
        IConsumer<EntityDeleted<Product>>,
        //product tags
        IConsumer<EntityInserted<ProductTag>>,
        IConsumer<EntityUpdated<ProductTag>>,
        IConsumer<EntityDeleted<ProductTag>>,
        //specification attributes
        IConsumer<EntityInserted<SpecificationAttribute>>,
        IConsumer<EntityUpdated<SpecificationAttribute>>,
        IConsumer<EntityDeleted<SpecificationAttribute>>,
        //specification attribute options
        IConsumer<EntityInserted<SpecificationAttributeOption>>,
        IConsumer<EntityUpdated<SpecificationAttributeOption>>,
        IConsumer<EntityDeleted<SpecificationAttributeOption>>,
        //Product specification attribute
        IConsumer<EntityInserted<ProductSpecificationAttribute>>,
        IConsumer<EntityUpdated<ProductSpecificationAttribute>>,
        IConsumer<EntityDeleted<ProductSpecificationAttribute>>,
        //Topics
        IConsumer<EntityInserted<Topic>>,
        IConsumer<EntityUpdated<Topic>>,
        IConsumer<EntityDeleted<Topic>>,
        //Category templates
        IConsumer<EntityInserted<CategoryTemplate>>,
        IConsumer<EntityUpdated<CategoryTemplate>>,
        IConsumer<EntityDeleted<CategoryTemplate>>,
        //Manufacturer templates
        IConsumer<EntityInserted<ManufacturerTemplate>>,
        IConsumer<EntityUpdated<ManufacturerTemplate>>,
        IConsumer<EntityDeleted<ManufacturerTemplate>>,
        //Product templates
        IConsumer<EntityInserted<ProductTemplate>>,
        IConsumer<EntityUpdated<ProductTemplate>>,
        IConsumer<EntityDeleted<ProductTemplate>>,
        //Orders
        IConsumer<EntityInserted<Order>>,
        IConsumer<EntityUpdated<Order>>,
        IConsumer<EntityDeleted<Order>>,
        //Order product variants
        IConsumer<EntityInserted<OrderItem>>,
        IConsumer<EntityUpdated<OrderItem>>,
        IConsumer<EntityDeleted<OrderItem>>,
        //Pictures
        IConsumer<EntityInserted<Picture>>,
        IConsumer<EntityUpdated<Picture>>,
        IConsumer<EntityDeleted<Picture>>,
        //Product picture mapping
        IConsumer<EntityInserted<ProductPicture>>,
        IConsumer<EntityUpdated<ProductPicture>>,
        IConsumer<EntityDeleted<ProductPicture>>,
        //polls
        IConsumer<EntityInserted<Poll>>,
        IConsumer<EntityUpdated<Poll>>,
        IConsumer<EntityDeleted<Poll>>,
        //blog posts
        IConsumer<EntityInserted<BlogPost>>,
        IConsumer<EntityUpdated<BlogPost>>,
        IConsumer<EntityDeleted<BlogPost>>,
        //news items
        IConsumer<EntityInserted<NewsItem>>,
        IConsumer<EntityUpdated<NewsItem>>,
        IConsumer<EntityDeleted<NewsItem>>,
        //states/province
        IConsumer<EntityInserted<StateProvince>>,
        IConsumer<EntityUpdated<StateProvince>>,
        IConsumer<EntityDeleted<StateProvince>>,
        //customer roles
        IConsumer<EntityUpdated<CustomerRole>>,
        IConsumer<EntityDeleted<CustomerRole>>,
		//stores
        IConsumer<EntityUpdated<Store>>,
        IConsumer<EntityDeleted<Store>>
    {
        /// <summary>
        /// Key for ManufacturerNavigationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : current manufacturer id
        /// {1} : language id
		/// {2} : current store ID
        /// </remarks>
        public const string MANUFACTURER_NAVIGATION_MODEL_KEY = "sm.pres.manufacturer.navigation-{0}-{1}-{2}";
        public const string MANUFACTURER_NAVIGATION_PATTERN_KEY = "sm.pres.manufacturer.navigation";

		/// <summary>
		/// Indicates whether a manufacturer has featured products
		/// </summary>
		/// <remarks>
		/// {0} : manufacturer id
		/// {1} : roles of the current user
		/// {2} : current store ID
		/// </remarks>
		public const string MANUFACTURER_HAS_FEATURED_PRODUCTS_KEY = "sm.pres.manufacturer.hasfeaturedproducts-{0}-{1}-{2}";
		public const string MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY = "sm.pres.manufacturer.hasfeaturedproducts";

        /// <summary>
        /// Key for CategoryNavigationModel caching
        /// </summary>
        /// <remarks>
		/// {0} : language id
		/// {1} : comma separated list of customer roles
		/// {2} : current store id
        /// </remarks>
        public const string CATEGORY_NAVIGATION_MODEL_KEY = "sm.pres.category.navigation-{0}-{1}-{2}";
        public const string CATEGORY_NAVIGATION_PATTERN_KEY = "sm.pres.category.navigation";

        /// <summary>
        /// Key for GetChildCategoryIds method results caching
        /// </summary>
        /// <remarks>
        /// {0} : parent category id
        /// {1} : show hidden?
        /// {2} : comma separated list of customer roles
		/// {3} : current store id
        /// </remarks>
        public const string CATEGORY_CHILD_IDENTIFIERS_MODEL_KEY = "sm.pres.category.childidentifiers-{0}-{1}-{2}-{3}";
        public const string CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY = "sm.pres.category.childidentifiers";

		/// <summary>
		/// Indicates whether a category has featured products
		/// </summary>
		/// <remarks>
		/// {0} : category id
		/// {1} : roles of the current user
		/// {2} : current store ID
		/// </remarks>
		public const string CATEGORY_HAS_FEATURED_PRODUCTS_KEY = "sm.pres.category.hasfeaturedproducts-{0}-{1}-{2}";
		public const string CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY = "sm.pres.category.hasfeaturedproducts";

        /// <summary>
        /// Key for ProductTagModel caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
		/// {2} : current store ID
        /// </remarks>
        public const string PRODUCTTAG_BY_PRODUCT_MODEL_KEY = "sm.pres.producttag.byproduct-{0}-{1}-{2}";
        public const string PRODUCTTAG_BY_PRODUCT_PATTERN_KEY = "sm.pres.producttag.byproduct";

        /// <summary>
        /// Key for PopularProductTagsModel caching
        /// </summary>
        /// <remarks>
        /// {0} : language id
		/// {1} : current store ID
        /// </remarks>
        public const string PRODUCTTAG_POPULAR_MODEL_KEY = "sm.pres.producttag.popular-{0}-{1}";
        public const string PRODUCTTAG_POPULAR_PATTERN_KEY = "sm.pres.producttag.popular";

        /// <summary>
        /// Key for ProductManufacturers model caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
		/// {2} : current store ID
        /// </remarks>
        public const string PRODUCT_MANUFACTURERS_MODEL_KEY = "sm.pres.product.manufacturers-{0}-{1}-{2}";
        public const string PRODUCT_MANUFACTURERS_PATTERN_KEY = "sm.pres.product.manufacturers";

        /// <summary>
        /// Key for ProductSpecificationModel caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : language id
        /// </remarks>
        public const string PRODUCT_SPECS_MODEL_KEY = "sm.pres.product.specs-{0}-{1}";
        public const string PRODUCT_SPECS_PATTERN_KEY = "sm.pres.product.specs";

        /// <summary>
        /// Key for TopicModel caching
        /// </summary>
        /// <remarks>
        /// {0} : topic id
        /// {1} : language id
		/// {2} : store id
        /// </remarks>
        public const string TOPIC_MODEL_KEY = "sm.pres.topic.details-{0}-{1}-{2}";
        public const string TOPIC_PATTERN_KEY = "sm.pres.topic.details";

        /// <summary>
        /// Key for TopicWidget caching
        /// </summary>
        /// <remarks>
        /// {0} : store id
        /// {1} : language id
        /// </remarks>
        public const string TOPIC_WIDGET_PATTERN_KEY = "sm.pres.topic.widget";
        public const string TOPIC_WIDGET_ALL_MODEL_KEY = "sm.pres.topic.widget-all-{0}-{1}";

        /// <summary>
        /// Key for CategoryTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : category template id
        /// </remarks>
        public const string CATEGORY_TEMPLATE_MODEL_KEY = "sm.pres.categorytemplate-{0}";
        public const string CATEGORY_TEMPLATE_PATTERN_KEY = "sm.pres.categorytemplate";

        /// <summary>
        /// Key for ManufacturerTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer template id
        /// </remarks>
        public const string MANUFACTURER_TEMPLATE_MODEL_KEY = "sm.pres.manufacturertemplate-{0}";
        public const string MANUFACTURER_TEMPLATE_PATTERN_KEY = "sm.pres.manufacturertemplate";

        /// <summary>
        /// Key for ProductTemplate caching
        /// </summary>
        /// <remarks>
        /// {0} : product template id
        /// </remarks>
        public const string PRODUCT_TEMPLATE_MODEL_KEY = "sm.pres.producttemplate-{0}";
        public const string PRODUCT_TEMPLATE_PATTERN_KEY = "sm.pres.producttemplate";

        /// <summary>
        /// Key for bestsellers identifiers displayed on the home page
        /// </summary>
		/// <remarks>
		/// {0} : current store id
		/// </remarks>
        public const string HOMEPAGE_BESTSELLERS_IDS_KEY = "sm.pres.bestsellers.homepage-{0}";
        public const string HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY = "sm.pres.bestsellers.homepage";

        /// <summary>
        /// Key for "also purchased" product identifiers displayed on the product details page
        /// </summary>
        /// <remarks>
        /// {0} : current product id
		/// {1} : current store id
        /// </remarks>
        public const string PRODUCTS_ALSO_PURCHASED_IDS_KEY = "sm.pres.alsopuchased-{0}-{1}";
        public const string PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY = "sm.pres.alsopuchased";

        /// <summary>
        /// Key for default product picture caching
        /// </summary>
        /// <remarks>
        /// {0} : product id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized product name)
        /// {4} : is connection SSL secured?
		/// {5} : current store ID
        /// </remarks>
        public const string PRODUCT_DEFAULTPICTURE_MODEL_KEY = "sm.pres.product.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string PRODUCT_DEFAULTPICTURE_PATTERN_KEY = "sm.pres.product.picture";

        /// <summary>
        /// Key for category picture caching
        /// </summary>
        /// <remarks>
        /// {0} : category id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized category name)
        /// {4} : is connection SSL secured?
		/// {5} : current store ID
        /// </remarks>
        public const string CATEGORY_PICTURE_MODEL_KEY = "sm.pres.category.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string CATEGORY_PICTURE_PATTERN_KEY = "sm.pres.category.picture";

        /// <summary>
        /// Key for manufacturer picture caching
        /// </summary>
        /// <remarks>
        /// {0} : manufacturer id
        /// {1} : picture size
        /// {2} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {3} : language ID ("alt" and "title" can depend on localized manufacturer name)
        /// {4} : is connection SSL secured?
		/// {5} : current store ID
        /// </remarks>
        public const string MANUFACTURER_PICTURE_MODEL_KEY = "sm.pres.manufacturer.picture-{0}-{1}-{2}-{3}-{4}-{5}";
        public const string MANUFACTURER_PICTURE_PATTERN_KEY = "sm.pres.manufacturer.picture";

        /// <summary>
        /// Key for cart picture caching
        /// </summary>
        /// <remarks>
		/// {0} : product id
		/// {1} : product attribute combination id
        /// {2} : picture size
        /// {3} : value indicating whether a default picture is displayed in case if no real picture exists
        /// {4} : language ID ("alt" and "title" can depend on localized product name)
        /// {5} : is connection SSL secured?
		/// {6} : current store ID
        /// </remarks>
        public const string CART_PICTURE_MODEL_KEY = "sm.pres.cart.picture-{0}-{1}-{2}-{3}-{4}-{5}-{6}";
        public const string CART_PICTURE_PATTERN_KEY = "sm.pres.cart.picture";

        /// <summary>
        /// Key for home page polls
        /// </summary>
        /// <remarks>
        /// {0} : language ID
		/// {1} : current store ID
        /// </remarks>
        public const string HOMEPAGE_POLLS_MODEL_KEY = "sm.pres.poll.homepage-{0}-{1}";
        /// <summary>
        /// Key for polls by system name
        /// </summary>
        /// <remarks>
        /// {0} : poll system name
        /// {1} : language ID
		/// {2} : current store ID
        /// </remarks>
        public const string POLL_BY_SYSTEMNAME_MODEL_KEY = "sm.pres.poll.systemname-{0}-{1}-{2}";
        public const string POLLS_PATTERN_KEY = "sm.pres.poll.";

        /// <summary>
        /// Key for blog tag list model
        /// </summary>
        /// <remarks>
        /// {0} : language ID
		/// {1} : store ID
        /// </remarks>
        public const string BLOG_TAGS_MODEL_KEY = "sm.pres.blog.tags-{0}-{1}";
        /// <summary>
        /// Key for blog archive (years, months) block model
        /// </summary>
        /// <remarks>
        /// {0} : language ID
		/// {1} : current store ID
        /// </remarks>
        public const string BLOG_MONTHS_MODEL_KEY = "sm.pres.blog.months-{0}-{1}";
        public const string BLOG_PATTERN_KEY = "sm.pres.blog.";

        /// <summary>
        /// Key for home page news
        /// </summary>
        /// <remarks>
        /// {0} : language ID
		/// {1} : store ID
        /// </remarks>
        public const string HOMEPAGE_NEWSMODEL_KEY = "sm.pres.news.homepage-{0}-{1}";
        public const string NEWS_PATTERN_KEY = "sm.pres.news.";
        
        /// <summary>
        /// Key for states by country id
        /// </summary>
        /// <remarks>
        /// {0} : country ID
        /// {1} : addEmptyStateIfRequired value
        /// {2} : language ID
        /// </remarks>
        public const string STATEPROVINCES_BY_COUNTRY_MODEL_KEY = "sm.pres.stateprovinces.bycountry-{0}-{1}-{2}";
        public const string STATEPROVINCES_PATTERN_KEY = "sm.pres.stateprovinces.";

        /// <summary>
        /// Key for available languages
        /// </summary>
		/// <remarks>
		/// {0} : current store ID
		/// </remarks>
        public const string AVAILABLE_LANGUAGES_MODEL_KEY = "sm.pres.languages.all-{0}";
        public const string AVAILABLE_LANGUAGES_PATTERN_KEY = "sm.pres.languages.";

        /// <summary>
        /// Key for available currencies
        /// </summary>
        /// <remarks>
        /// {0} : language ID
		/// {1} : current store ID
        /// </remarks>
        public const string AVAILABLE_CURRENCIES_MODEL_KEY = "sm.pres.currencies.all-{0}-{1}";
        public const string AVAILABLE_CURRENCIES_PATTERN_KEY = "sm.pres.currencies.";

		/// <summary>
		/// Key for store header data
		/// </summary>
		/// <remarks>
		/// {0} : current store ID
		/// </remarks>
        public const string SHOPHEADER_MODEL_KEY = "sm.pres.shopheader-{0}";
		public const string SHOPHEADER_MODEL_PATTERN_KEY = "sm.pres.shopheader";

		/// <summary>
		/// Key for sitemap
		/// </summary>
		/// <remarks>
		/// {0} : language id
		/// {1} : current user roles
		/// {2} : current store id
		/// </remarks>
		public const string SITEMAP_PAGE_MODEL_KEY = "sm.pres.sitemap.page-{0}-{1}-{2}";

		/// <summary>
		/// Key for seo sitemap
		/// </summary>
		/// <remarks>
		/// {0} : language id
		/// {1} : current user roles
		/// {2} : current store id
		/// </remarks>
		public const string SITEMAP_XML_MODEL_KEY = "sm.pres.sitemap.xml-{0}-{1}-{2}";
		public const string SITEMAP_PATTERN_KEY = "sm.pres.sitemap";

        private readonly ICacheManager _cacheManager;
        
        public ModelCacheEventConsumer(Func<string, ICacheManager> cache)
        {
			this._cacheManager = cache("static");
        }

        //languages
        public void HandleEvent(EntityInserted<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Language> eventMessage)
        {
            //clear all localizable models
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_LANGUAGES_PATTERN_KEY);
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        
        //currencies
        public void HandleEvent(EntityInserted<Currency> eventMessage)
        {
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Currency> eventMessage)
        {
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Currency> eventMessage)
        {
            _cacheManager.RemoveByPattern(AVAILABLE_CURRENCIES_PATTERN_KEY);
        }

        //settings
        public void HandleEvent(EntityUpdated<Setting> eventMessage)
        {
            //clear models which depend on settings
            _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY); //depends on CatalogSettings.NumberOfProductTags
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY); //depends on CatalogSettings.ManufacturersBlockItemsToDisplay
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY); //depends on CatalogSettings.ShowCategoryProductNumber and CatalogSettings.ShowCategoryProductNumberIncludingSubcategories
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY); //depends on CatalogSettings.NumberOfBestsellersOnHomepage
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY); //depends on BlogSettings.NumberOfTags
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY); //depends on NewsSettings.MainPageNewsCount
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY); //depends on sitemap settings
        }
        
        //manufacturers
        public void HandleEvent(EntityInserted<Manufacturer> eventMessage)
        {
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Manufacturer> eventMessage)
        {
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Manufacturer> eventMessage)
        {
            _cacheManager.RemoveByPattern(MANUFACTURER_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //product manufacturers
        public void HandleEvent(EntityInserted<ProductManufacturer> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
			_cacheManager.RemoveByPattern(MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<ProductManufacturer> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
			_cacheManager.RemoveByPattern(MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<ProductManufacturer> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_MANUFACTURERS_PATTERN_KEY);
			_cacheManager.RemoveByPattern(MANUFACTURER_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        
        //categories
        public void HandleEvent(EntityInserted<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Category> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //product categories
        public void HandleEvent(EntityInserted<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
			_cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
			_cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<ProductCategory> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
			_cacheManager.RemoveByPattern(CATEGORY_HAS_FEATURED_PRODUCTS_PATTERN_KEY);
        }

        //products
        public void HandleEvent(EntityInserted<Product> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Product> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Product> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }

        //product tags
        public void HandleEvent(EntityInserted<ProductTag> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
			_cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<ProductTag> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
			_cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<ProductTag> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCTTAG_POPULAR_PATTERN_KEY);
			_cacheManager.RemoveByPattern(PRODUCTTAG_BY_PRODUCT_PATTERN_KEY);
        }
        
        //specification attributes
        public void HandleEvent(EntityInserted<SpecificationAttribute> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdated<SpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<SpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
        }
        
        //specification attribute options
        public void HandleEvent(EntityInserted<SpecificationAttributeOption> eventMessage)
        {
        }
        public void HandleEvent(EntityUpdated<SpecificationAttributeOption> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<SpecificationAttributeOption> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
        }
        
        //Product specification attribute
        public void HandleEvent(EntityInserted<ProductSpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<ProductSpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<ProductSpecificationAttribute> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_SPECS_PATTERN_KEY);
        }

        //Topics
        public void HandleEvent(EntityInserted<Topic> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_WIDGET_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Topic> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_WIDGET_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Topic> eventMessage)
        {
            _cacheManager.RemoveByPattern(TOPIC_PATTERN_KEY);
            _cacheManager.RemoveByPattern(TOPIC_WIDGET_PATTERN_KEY);
			_cacheManager.RemoveByPattern(SITEMAP_PATTERN_KEY);
        }
        
        //Category templates
        public void HandleEvent(EntityInserted<CategoryTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<CategoryTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<CategoryTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_TEMPLATE_PATTERN_KEY);
        }

        //Manufacturer templates
        public void HandleEvent(EntityInserted<ManufacturerTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<ManufacturerTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<ManufacturerTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(MANUFACTURER_TEMPLATE_PATTERN_KEY);
        }

        //Product templates
        public void HandleEvent(EntityInserted<ProductTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<ProductTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<ProductTemplate> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_TEMPLATE_PATTERN_KEY);
        }


        //Orders
        public void HandleEvent(EntityInserted<Order> eventMessage)
        {
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Order> eventMessage)
        {
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Order> eventMessage)
        {
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        //Order product variants
        public void HandleEvent(EntityInserted<OrderItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<OrderItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<OrderItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(HOMEPAGE_BESTSELLERS_IDS_PATTERN_KEY);
            _cacheManager.RemoveByPattern(PRODUCTS_ALSO_PURCHASED_IDS_PATTERN_KEY);
        }

        //Pictures
        public void HandleEvent(EntityInserted<Picture> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Picture> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Picture> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(MANUFACTURER_PICTURE_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CART_PICTURE_PATTERN_KEY);
        }

        //Product picture mappings
        public void HandleEvent(EntityInserted<ProductPicture> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<ProductPicture> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<ProductPicture> eventMessage)
        {
            _cacheManager.RemoveByPattern(PRODUCT_DEFAULTPICTURE_PATTERN_KEY);
        }

        //Polls
        public void HandleEvent(EntityInserted<Poll> eventMessage)
        {
            _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<Poll> eventMessage)
        {
            _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<Poll> eventMessage)
        {
            _cacheManager.RemoveByPattern(POLLS_PATTERN_KEY);
        }

        //Blog posts
        public void HandleEvent(EntityInserted<BlogPost> eventMessage)
        {
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<BlogPost> eventMessage)
        {
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<BlogPost> eventMessage)
        {
            _cacheManager.RemoveByPattern(BLOG_PATTERN_KEY);
        }

        //News items
        public void HandleEvent(EntityInserted<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<NewsItem> eventMessage)
        {
            _cacheManager.RemoveByPattern(NEWS_PATTERN_KEY);
        }

        //State/province
        public void HandleEvent(EntityInserted<StateProvince> eventMessage)
        {
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
        }
        public void HandleEvent(EntityUpdated<StateProvince> eventMessage)
        {
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<StateProvince> eventMessage)
        {
            _cacheManager.RemoveByPattern(STATEPROVINCES_PATTERN_KEY);
        }

        //customer roles
        public void HandleEvent(EntityUpdated<CustomerRole> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
        }
        public void HandleEvent(EntityDeleted<CustomerRole> eventMessage)
        {
            _cacheManager.RemoveByPattern(CATEGORY_NAVIGATION_PATTERN_KEY);
            _cacheManager.RemoveByPattern(CATEGORY_CHILD_IDENTIFIERS_PATTERN_KEY);
        }

		//stores
		public void HandleEvent(EntityUpdated<Store> eventMessage)
		{
			_cacheManager.RemoveByPattern(SHOPHEADER_MODEL_PATTERN_KEY);
		}
		public void HandleEvent(EntityDeleted<Store> eventMessage)
		{
			_cacheManager.RemoveByPattern(SHOPHEADER_MODEL_PATTERN_KEY);
		}

    }
}
