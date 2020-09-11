using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Infrastructure;

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
        /// Builds LINQ query for categories
        /// </summary>
        /// <param name="categoryName">Category name filter</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="alias">Alias filter</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <returns>Category query</returns>
        IQueryable<Category> BuildCategoriesQuery(
            string categoryName = "",
            bool showHidden = false,
            string alias = null,
            int storeId = 0);

        /// <summary>
        /// Gets all categories
        /// </summary>
        /// <param name="categoryName">Category name filter</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="alias">Alias filter</param>
        /// <param name="applyNavigationFilters">(Obsolete) Whether to apply <see cref="ICategoryNavigationFilter"/> instances to the actual categories query. Never applied when <paramref name="showHidden"/> is <c>true</c></param>
        /// <param name="ignoreDetachedCategories">A value indicating whether categories without parent category in provided category list (source) should be ignored</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <returns>Categories</returns>
        IPagedList<Category> GetAllCategories(
            string categoryName = "",
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false,
            string alias = null,
            bool ignoreDetachedCategories = true,
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
        /// Gets categories by Ids
        /// </summary>
        /// <param name="categoryId">Array of category identifiers</param>
        /// <returns>List of Categories</returns>
        IList<Category> GetCategoriesByIds(int[] categoryIds);

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
        IPagedList<ProductCategory> GetProductCategoriesByCategoryId(
            int categoryId,
            int pageIndex,
            int pageSize,
            bool showHidden = false);

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
        /// <param name="node">The category node</param>
        /// <returns>Trail</returns>
        IEnumerable<ICategoryNode> GetCategoryTrail(ICategoryNode node);


        /// <summary>
        /// Builds a category breadcrumb (path) for a particular category node
        /// </summary>
        /// <param name="treeNode">The category node</param>
        /// <param name="languageId">The id of language. Pass <c>null</c> to skip localization.</param>
        /// <param name="aliasPattern">How the category alias - if specified - should be appended to the category name (e.g. <c>({0})</c>)</param>
        /// <param name="separator">The separator string</param>
        /// <returns>Category breadcrumb path</returns>
        string GetCategoryPath(
            TreeNode<ICategoryNode> treeNode,
            int? languageId = null,
            string aliasPattern = null,
            string separator = " » ");

        /// <summary>
        /// Gets the tree representation of categories
        /// </summary>
        /// <param name="rootCategoryId">Specifies which node to return as root</param>
        /// <param name="includeHidden"><c>false</c> excludes unpublished and ACL-inaccessible categories</param>
        /// <param name="storeId">&gt; 0 = apply store mapping, 0 to bypass store mapping</param>
        /// <returns>The category tree representation</returns>
        /// <remarks>
        /// This method puts the tree result into application cache, so subsequent calls are very fast.
        /// Localization is up to the caller because the nodes only contain unlocalized data.
        /// Subscribe to the <c>CategoryTreeChanged</c> event if you need to evict cache entries which depend
        /// on this method's result.
        /// </remarks>
        TreeNode<ICategoryNode> GetCategoryTree(
            int rootCategoryId = 0,
            bool includeHidden = false,
            int storeId = 0);

        /// <summary>
        /// Get count of all categories
        /// </summary>
        /// <returns>Count</returns>
        int CountAllCategories();
    }

    public static class ICategoryServiceExtensions
    {
        /// <summary>
        /// Builds a category breadcrumb for a particular product
        /// </summary>
        /// <param name="product">The product</param>
        /// <param name="languageId">The id of language. Pass <c>null</c> to skip localization.</param>
        /// <param name="storeId">The id of store. Pass <c>null</c> to skip store filtering.</param>
        /// <param name="separator">The separator string</param>
        /// <returns>Category breadcrumb for product</returns>
        public static string GetCategoryPath(this ICategoryService categoryService,
            Product product,
            int? languageId = null,
            int? storeId = null,
            string separator = " » ")
        {
            Guard.NotNull(product, nameof(product));

            string result = string.Empty;

            var pc = categoryService.GetProductCategoriesByProductId(product.Id).FirstOrDefault();
            if (pc != null)
            {
                var node = categoryService.GetCategoryTree(pc.CategoryId, false, storeId ?? 0);
                if (node != null)
                {
                    result = categoryService.GetCategoryPath(node, languageId, null, separator);
                }
            }

            return result;
        }
    }
}
