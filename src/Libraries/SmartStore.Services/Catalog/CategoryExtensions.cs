using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class CategoryExtensions
    {
        /// <summary>
        /// Sort categories for tree representation
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="parentId">Parent category identifier</param>
        /// <param name="ignoreCategoriesWithoutExistingParent">A value indicating whether categories without parent category in provided category list (source) should be ignored</param>
        /// <returns>Sorted categories</returns>
        public static IList<Category> SortCategoriesForTree(this IList<Category> source, int parentId = 0, bool ignoreCategoriesWithoutExistingParent = false)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var result = new List<Category>();

            var categories = source.ToList().FindAll(c => c.ParentCategoryId == parentId);
            foreach (var cat in categories)
            {
                result.Add(cat);
                result.AddRange(SortCategoriesForTree(source, cat.Id, true));
            }
            if (!ignoreCategoriesWithoutExistingParent && result.Count != source.Count)
            {
                // find categories without parent in provided category source and insert them into result
                foreach (var cat in source)
                    if (result.Where(x => x.Id == cat.Id).FirstOrDefault() == null)
                        result.Add(cat);
            }
            return result;
        }

        /// <summary>
        /// Returns a ProductCategory that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="categoryId">Category identifier</param>
        /// <returns>A ProductCategory that has the specified values; otherwise null</returns>
        public static ProductCategory FindProductCategory(this IList<ProductCategory> source, int productId, int categoryId)
        {
			foreach (var productCategory in source)
			{
				if (productCategory.ProductId == productId && productCategory.CategoryId == categoryId)
					return productCategory;
			}
            return null;
        }

		public static string GetCategoryNameWithAlias(this Category category)
		{
			if (category != null)
			{
				if (category.Alias.HasValue())
					return "{0} ({1})".FormatWith(category.Name, category.Alias);
				else
					return category.Name;
			}
			return null;
		}

        public static string GetCategoryNameWithPrefix(this Category category, ICategoryService categoryService, IDictionary<int, Category> mappedCategories = null)
        {
            string result = string.Empty;

            while (category != null)
            {
                if (String.IsNullOrEmpty(result))
                {
                    result = category.GetCategoryNameWithAlias();
                }
                else
                {
                    result = "--" + result;
                }

                int parentId = category.ParentCategoryId;
                if (mappedCategories == null)
                {
                    category = categoryService.GetCategoryById(parentId);
                }
                else
                {
                    category = mappedCategories.ContainsKey(parentId) ? mappedCategories[parentId] : categoryService.GetCategoryById(parentId);
                }
            }
            return result;
        }

        public static string GetCategoryBreadCrumb(this Category category, ICategoryService categoryService, IDictionary<int, Category> mappedCategories = null)
        {
            string result = string.Empty;

            while (category != null && !category.Deleted)
            {
                if (String.IsNullOrEmpty(result))
                {
                    result = category.GetCategoryNameWithAlias();
                }
                else
                {
                    result = category.GetCategoryNameWithAlias() + " >> " + result;
                }

                int parentId = category.ParentCategoryId;
                if (mappedCategories == null)
                {
                    category = categoryService.GetCategoryById(parentId);
                }
                else
                {
                    category = mappedCategories.ContainsKey(parentId) ? mappedCategories[parentId] : categoryService.GetCategoryById(parentId);
                }
            }

            return result;
        }

    }
}
