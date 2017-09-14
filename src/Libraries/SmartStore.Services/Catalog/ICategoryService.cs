using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public partial interface ICategoryService
    {

        /// <summary>
        /// Assign acl to sub-categories and products
        /// </summary>
        /// <param name="categoryId">Category Id</param>
        /// <param name="touchProductsWithMultipleCategories">Reserved for future use: Whether to assign acl's to products which are contained in multiple categories.</param>
        /// <param name="touchExistingAcls">Reserved for future use: Whether to delete existing Acls.</param>
        /// <param name="categoriesOnly">Reserved for future use: Whether to assign acl's only to categories.</param>
        void InheritAclIntoChildren(int categoryId,
            bool touchProductsWithMultipleCategories = false,
            bool touchExistingAcls = false,
            bool categoriesOnly = false);

        /// <summary>
        /// Assign stores to sub-categories and products
        /// </summary>
        /// <param name="categoryId">Category Id</param>
        /// <param name="touchProductsWithMultipleCategories">Reserved for future use: Whether to assign acl's to products which are contained in multiple categories.</param>
        /// <param name="touchExistingAcls">Reserved for future use: Whether to delete existing Acls.</param>
        /// <param name="categoriesOnly">Reserved for future use: Whether to assign acl's only to categories.</param>
        void InheritStoresIntoChildren(int categoryId,
            bool touchProductsWithMultipleCategories = false,
            bool touchExistingAcls = false,
            bool categoriesOnly = false);

        /// <summary>
        /// Delete category
        /// </summary>
        /// <param name="category">Category</param>
		/// <param name="deleteChilds">Whether to delete child categories or to set them to no parent.</param>
		void DeleteCategory(Category category, bool deleteChilds = false);

		/// <summary>
		/// Gets categories
		/// </summary>
		/// <param name="categoryName">Category name</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <param name="alias">Alias to be filtered</param>
		/// <param name="applyNavigationFilters">(Obsolete) Whether to apply <see cref="ICategoryNavigationFilter"/> instances to the actual categories query. Never applied when <paramref name="showHidden"/> is <c>true</c></param>
		/// <param name="storeId">Store identifier; 0 to load all records</param>
		/// <returns>Category query</returns>
		IQueryable<Category> GetCategories(
			string categoryName = "",
			bool showHidden = false,
			string alias = null,
			bool applyNavigationFilters = true,
			int storeId = 0);

		/// <summary>
		/// Gets all categories
		/// </summary>
		/// <param name="categoryName">Category name</param>
		/// <param name="pageIndex">Page index</param>
		/// <param name="pageSize">Page size</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <param name="alias">Alias to be filtered</param>
		/// <param name="applyNavigationFilters">(Obsolete) Whether to apply <see cref="ICategoryNavigationFilter"/> instances to the actual categories query. Never applied when <paramref name="showHidden"/> is <c>true</c></param>
		/// <param name="ignoreCategoriesWithoutExistingParent">A value indicating whether categories without parent category in provided category list (source) should be ignored</param>
		/// <param name="storeId">Store identifier; 0 to load all records</param>
		/// <returns>Categories</returns>
		IPagedList<Category> GetAllCategories(
			string categoryName = "", 
			int pageIndex = 0, 
			int pageSize = int.MaxValue, 
			bool showHidden = false, 
			string alias = null,
			bool applyNavigationFilters = true, 
			bool ignoreCategoriesWithoutExistingParent = true, 
			int storeId = 0);

        /// <summary>
        /// Gets all categories filtered by parent category identifier
        /// </summary>
        /// <param name="parentCategoryId">Parent category identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Category collection</returns>
        IList<Category> GetAllCategoriesByParentCategoryId(int parentCategoryId, bool showHidden = false);

        /// <summary>
        /// Gets all categories displayed on the home page
        /// </summary>
        /// <returns>Categories</returns>
        IList<Category> GetAllCategoriesDisplayedOnHomePage();
                
        /// <summary>
        /// Gets a category
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>Category</returns>
        Category GetCategoryById(int categoryId);

        /// <summary>
        /// Inserts category
        /// </summary>
        /// <param name="category">Category</param>
        void InsertCategory(Category category);

        /// <summary>
        /// Updates the category
        /// </summary>
        /// <param name="category">Category</param>
        void UpdateCategory(Category category);

        /// <summary>
        /// Update HasDiscountsApplied property (used for performance optimization)
        /// </summary>
        /// <param name="category">Category</param>
        void UpdateHasDiscountsApplied(Category category);

        /// <summary>
        /// Deletes a product category mapping
        /// </summary>
        /// <param name="productCategory">Product category</param>
        void DeleteProductCategory(ProductCategory productCategory);

        /// <summary>
        /// Gets product category mapping collection
        /// </summary>
        /// <param name="categoryId">Category identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product a category mapping collection</returns>
        IPagedList<ProductCategory> GetProductCategoriesByCategoryId(int categoryId,
            int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Gets a product category mapping collection
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product category mapping collection</returns>
        IList<ProductCategory> GetProductCategoriesByProductId(int productId, bool showHidden = false);

		/// <summary>
		/// Gets product category mappings
		/// </summary>
		/// <param name="productIds">Product identifiers</param>
		/// <param name="hasDiscountsApplied">A value indicating whether to filter categories with applied discounts</param>
		/// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <returns>Map with product category mappings</returns>
		Multimap<int, ProductCategory> GetProductCategoriesByProductIds(int[] productIds, bool? hasDiscountsApplied = null, bool showHidden = false);

		/// <summary>
		/// Gets product category mappings
		/// </summary>
		/// <param name="categoryIds">Category identifiers</param>
		/// <returns>Map with product category mappings</returns>
		Multimap<int, ProductCategory> GetProductCategoriesByCategoryIds(int[] categoryIds);

        /// <summary>
        /// Gets a product category mapping 
        /// </summary>
        /// <param name="productCategoryId">Product category mapping identifier</param>
        /// <returns>Product category mapping</returns>
        ProductCategory GetProductCategoryById(int productCategoryId);

        /// <summary>
        /// Inserts a product category mapping
        /// </summary>
        /// <param name="productCategory">>Product category mapping</param>
        void InsertProductCategory(ProductCategory productCategory);

        /// <summary>
        /// Updates the product category mapping 
        /// </summary>
        /// <param name="productCategory">>Product category mapping</param>
        void UpdateProductCategory(ProductCategory productCategory);

		/// <summary>
		/// Gets the category trail
		/// </summary>
		/// <param name="category">Category</param>
		/// <returns>Trail</returns>
		ICollection<Category> GetCategoryTrail(Category category);

		/// <summary>
		/// Builds a category breadcrumb (path) for a particular product
		/// </summary>
		/// <param name="product">The product</param>
		/// <param name="languageId">The id of language</param>
		/// <param name="pathLookup">A delegate for fast (cached) path lookup</param>
		/// <param name="addPathToCache">A callback that saves the resolved path to a cache (when <c>pathLookup</c> returned null)</param>
		/// <param name="categoryLookup">A delegate for fast (cached) category lookup</param>
		/// <param name="prodCategory">First product category of product</param>
		/// <returns>Category breadcrumb for product</returns>
		string GetCategoryPath(Product product, int? languageId, Func<int, string> pathLookup, Action<int, string> addPathToCache, Func<int, Category> categoryLookup,
			ProductCategory prodCategory = null);
    }

	public static class ICategoryServiceExtensions
	{
		/// <summary>
		/// Builds a category breadcrumb for a particular product
		/// </summary>
		/// <param name="product">The product</param>
		/// <returns>Category breadcrumb for product</returns>
		public static string GetCategoryBreadCrumb(this ICategoryService categoryService, Product product)
		{
			return categoryService.GetCategoryPath(product, null, null, null, null);
		}
	}
}
