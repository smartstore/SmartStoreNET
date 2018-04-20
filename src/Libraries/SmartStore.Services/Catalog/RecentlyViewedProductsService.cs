using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Security;

namespace SmartStore.Services.Catalog
{
	/// <summary>
	/// Recently viewed products service
	/// </summary>
	public partial class RecentlyViewedProductsService : IRecentlyViewedProductsService
    {
        #region Fields

        private readonly HttpContextBase _httpContext;
        private readonly IProductService _productService;
		private readonly IAclService _aclService;
		private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor
        
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="httpContext">HTTP context</param>
        /// <param name="productService">Product service</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public RecentlyViewedProductsService(
			HttpContextBase httpContext,
			IProductService productService,
			IAclService aclService,
			CatalogSettings catalogSettings)
        {
            _httpContext = httpContext;
            _productService = productService;
			_aclService = aclService;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets a "recently viewed products" identifier list
        /// </summary>
        /// <returns>"recently viewed products" list</returns>
        protected IList<int> GetRecentlyViewedProductsIds()
        {
            return GetRecentlyViewedProductsIds(int.MaxValue);
        }

        /// <summary>
        /// Gets a "recently viewed products" identifier list
        /// </summary>
        /// <param name="number">Number of products to load</param>
        /// <returns>"recently viewed products" list</returns>
        protected IList<int> GetRecentlyViewedProductsIds(int number)
        {
            var productIds = new List<int>();
            var recentlyViewedCookie = _httpContext.Request.Cookies.Get("SmartStore.RecentlyViewedProducts");
            if ((recentlyViewedCookie == null) || (recentlyViewedCookie.Values == null))
                return productIds;

            string[] values = recentlyViewedCookie.Values.GetValues("RecentlyViewedProductIds");
            if (values == null)
                return productIds;

            productIds.AddRange(values.Select(x => int.Parse(x)).Distinct().Take(number));

            return productIds;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a "recently viewed products" list
        /// </summary>
        /// <param name="number">Number of products to load</param>
        /// <returns>"recently viewed products" list</returns>
        public virtual IList<Product> GetRecentlyViewedProducts(int number)
        {
            var productIds = GetRecentlyViewedProductsIds(number);
			var recentlyViewedProducts = _productService
				.GetProductsByIds(productIds.ToArray())
				.Where(x => x.Published && !x.Deleted && !x.IsSystemProduct && _aclService.Authorize(x))
				.ToList();

            return recentlyViewedProducts;
        }

        /// <summary>
        /// Adds a product to a recently viewed products list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        public virtual void AddProductToRecentlyViewedList(int productId)
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return;

            var oldProductIds = GetRecentlyViewedProductsIds();
            var newProductIds = new List<int>(oldProductIds);

            if (!newProductIds.Contains(productId)) 
            {
                newProductIds.Add(productId);
            }

            var recentlyViewedCookie = _httpContext.Request.Cookies.Get("SmartStore.RecentlyViewedProducts");
			if (recentlyViewedCookie == null)
			{
				recentlyViewedCookie = new HttpCookie("SmartStore.RecentlyViewedProducts");
				recentlyViewedCookie.HttpOnly = true;
			}
            recentlyViewedCookie.Values.Clear();

            int maxProducts = _catalogSettings.RecentlyViewedProductsNumber;
            if (maxProducts <= 0)
                maxProducts = 10;

            int skip = Math.Max(0, newProductIds.Count - maxProducts);
            newProductIds.Skip(skip).Take(maxProducts).Each(x => {
                recentlyViewedCookie.Values.Add("RecentlyViewedProductIds", x.ToString());
            });

            recentlyViewedCookie.Expires = DateTime.Now.AddDays(10.0);
            _httpContext.Response.Cookies.Set(recentlyViewedCookie);
        }
        
        #endregion
    }
}
