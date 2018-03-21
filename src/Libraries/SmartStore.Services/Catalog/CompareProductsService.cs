using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Search;

namespace SmartStore.Services.Catalog
{
	/// <summary>
	/// Compare products service
	/// </summary>
	public partial class CompareProductsService : ICompareProductsService
    {
        private const string COMPARE_PRODUCTS_COOKIE_NAME = "sm.CompareProducts";

        private readonly HttpContextBase _httpContext;
        private readonly IProductService _productService;
		private readonly ICatalogSearchService _catalogSearchService;

		public CompareProductsService(
			HttpContextBase httpContext,
			IProductService productService,
			ICatalogSearchService catalogSearchService)
        {
            _httpContext = httpContext;
            _productService = productService;
			_catalogSearchService = catalogSearchService;
        }

        #region Utilities

        /// <summary>
        /// Gets a "compare products" identifier list
        /// </summary>
        /// <returns>"compare products" identifier list</returns>
        protected List<int> GetComparedProductIds()
        {
            var productIds = new List<int>();
            HttpCookie compareCookie = _httpContext.Request.Cookies.Get(COMPARE_PRODUCTS_COOKIE_NAME);
            if ((compareCookie == null) || (compareCookie.Values == null))
                return productIds;
            string[] values = compareCookie.Values.GetValues("CompareProductIds");
            if (values == null)
                return productIds;
            foreach (string productId in values)
            {
                int prodId = int.Parse(productId);
                if (!productIds.Contains(prodId))
                    productIds.Add(prodId);
            }

            return productIds;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears a "compare products" list
        /// </summary>
        public virtual void ClearCompareProducts()
        {
            var compareCookie = _httpContext.Request.Cookies.Get(COMPARE_PRODUCTS_COOKIE_NAME);
            if (compareCookie != null)
            {
                compareCookie.Values.Clear();
                compareCookie.Expires = DateTime.Now.AddYears(-1);
                _httpContext.Response.Cookies.Set(compareCookie);
            }
        }

        /// <summary>
        /// Gets a "compare products" list
        /// </summary>
        /// <returns>"Compare products" list</returns>
        public virtual IList<Product> GetComparedProducts()
        {
            var products = new List<Product>();
            var productIds = GetComparedProductIds();
            foreach (int productId in productIds)
            {
                var product = _productService.GetProductById(productId);
                if (product != null && product.Published && !product.Deleted && !product.IsSystemProduct)
                    products.Add(product);
            }
            return products;
        }

        public virtual int GetComparedProductsCount()
        {
			var productIds = GetComparedProductIds();
			if (productIds.Count == 0)
				return 0;

			var searchQuery = new CatalogSearchQuery()
				.VisibleOnly()
				.WithProductIds(productIds.ToArray())
				.BuildHits(false);

			var result = _catalogSearchService.Search(searchQuery);
            return result.TotalHitsCount;
        }


        /// <summary>
        /// Removes a product from a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        public virtual void RemoveProductFromCompareList(int productId)
        {
            var oldProductIds = GetComparedProductIds();
            var newProductIds = new List<int>();
            newProductIds.AddRange(oldProductIds);
            newProductIds.Remove(productId);

            var compareCookie = _httpContext.Request.Cookies.Get(COMPARE_PRODUCTS_COOKIE_NAME);
            if ((compareCookie == null) || (compareCookie.Values == null))
                return;
            compareCookie.Values.Clear();
            foreach (int newProductId in newProductIds)
                compareCookie.Values.Add("CompareProductIds", newProductId.ToString());
            compareCookie.Expires = DateTime.Now.AddDays(10.0);
            _httpContext.Response.Cookies.Set(compareCookie);
        }

        /// <summary>
        /// Adds a product to a "compare products" list
        /// </summary>
        /// <param name="productId">Product identifier</param>
        public virtual void AddProductToCompareList(int productId)
        {
            var oldProductIds = GetComparedProductIds();
            var newProductIds = new List<int>();
            newProductIds.Add(productId);
			
			foreach (int oldProductId in oldProductIds)
			{
				if (oldProductId != productId)
					newProductIds.Add(oldProductId);
			}

            var compareCookie = _httpContext.Request.Cookies.Get(COMPARE_PRODUCTS_COOKIE_NAME);
			if (compareCookie == null)
			{
				compareCookie = new HttpCookie(COMPARE_PRODUCTS_COOKIE_NAME);
				compareCookie.HttpOnly = true;
			}

            compareCookie.Values.Clear();
            int maxProducts = 4;
            int i = 1;
            foreach (int newProductId in newProductIds)
            {
                compareCookie.Values.Add("CompareProductIds", newProductId.ToString());
                if (i == maxProducts)
                    break;
                i++;
            }
            compareCookie.Expires = DateTime.Now.AddDays(10.0);
            _httpContext.Response.Cookies.Set(compareCookie);
        }

        #endregion
    }
}
