﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Customers;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(CategoryValidator))]
    public class CategoryModel : TabbableModel, ILocalizedModel<CategoryLocalizedModel>
    {
        public CategoryModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }
            Locales = new List<CategoryLocalizedModel>();
            AvailableCategoryTemplates = new List<SelectListItem>();
            AvailableDefaultViewModes = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Alias")]
		public string Alias { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.CategoryTemplate")]
        [AllowHtml]
        public int CategoryTemplateId { get; set; }
        public IList<SelectListItem> AvailableCategoryTemplates { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Parent")]
        // codehint: sm-edit
        public int? ParentCategoryId { get; set; }

        [UIHint("Picture")]
        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Picture")]
        public int PictureId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.PageSize")]
        public int PageSize { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.PriceRanges")]
        [AllowHtml]
        public string PriceRanges { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Deleted")]
        public bool Deleted { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Common.CreatedOn")]
		public DateTime? CreatedOn { get; set; }

		[SmartResourceDisplayName("Common.UpdatedOn")]
		public DateTime? UpdatedOn { get; set; }
        
        public IList<CategoryLocalizedModel> Locales { get; set; }

        public string Breadcrumb { get; set; }

        //ACL
        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.SubjectToAcl")]
        public bool SubjectToAcl { get; set; }
        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.AclCustomerRoles")]
        public List<CustomerRoleModel> AvailableCustomerRoles { get; set; }
        public int[] SelectedCustomerRoleIds { get; set; }

		//Store mapping
		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

        public string ParentCategoryBreadcrumb { get; set; }

        //discounts
        public List<Discount> AvailableDiscounts { get; set; }
        public int[] SelectedDiscountIds { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultViewMode")]
        public string DefaultViewMode { get; set; }
        public IList<SelectListItem> AvailableDefaultViewModes { get; private set; }

        #region Nested classes

        public class CategoryProductModel : EntityModelBase
        {
            public int CategoryId { get; set; }

            public int ProductId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Categories.Products.Fields.Product")]
            public string ProductName { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
			public string Sku { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
			public string ProductTypeName { get; set; }
			public string ProductTypeLabelHint { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
			public bool Published { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Categories.Products.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Categories.Products.Fields.DisplayOrder")]
            //we don't name it DisplayOrder because Telerik has a small bug 
            //"if we have one more editor with the same name on a page, it doesn't allow editing"
            //in our case it's category.DisplayOrder
            public int DisplayOrder1 { get; set; }
        }

        public class AddCategoryProductModel : ModelBase
        {
            public AddCategoryProductModel()
            {
                AvailableCategories = new List<SelectListItem>();
                AvailableManufacturers = new List<SelectListItem>();
				AvailableProductTypes = new List<SelectListItem>();
            }
            public GridModel<ProductModel> Products { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductName")]
            [AllowHtml]
            public string SearchProductName { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchCategory")]
            public int SearchCategoryId { get; set; }
            [SmartResourceDisplayName("Admin.Catalog.Products.List.SearchManufacturer")]
            public int SearchManufacturerId { get; set; }
			[SmartResourceDisplayName("Admin.Catalog.Products.List.SearchProductType")]
			public int SearchProductTypeId { get; set; }

            public IList<SelectListItem> AvailableCategories { get; set; }
            public IList<SelectListItem> AvailableManufacturers { get; set; }
			public IList<SelectListItem> AvailableProductTypes { get; set; }

            public int CategoryId { get; set; }

            public int[] SelectedProductIds { get; set; }
        }

        #endregion
    }

    public class CategoryLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
        [AllowHtml]
        public string Description {get;set;}

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }
    }

}