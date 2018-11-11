using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Catalog;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
	[Validator(typeof(ManufacturerValidator))]
    public class ManufacturerModel : EntityModelBase, ILocalizedModel<ManufacturerLocalizedModel>
    {
        public ManufacturerModel()
        {
            if (PageSize < 1)
            {
                PageSize = 5;
            }

            Locales = new List<ManufacturerLocalizedModel>();
            AvailableManufacturerTemplates = new List<SelectListItem>();
        }

		public int GridPageSize { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.ManufacturerTemplate")]
        [AllowHtml]
        public int ManufacturerTemplateId { get; set; }
        public IList<SelectListItem> AvailableManufacturerTemplates { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [UIHint("Picture")]
        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Picture")]
        public int PictureId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.PageSize")]
        public int PageSize { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.AllowCustomersToSelectPageSize")]
        public bool AllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.PriceRanges")]
        [AllowHtml]
        public string PriceRanges { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Deleted")]
        public bool Deleted { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Common.CreatedOn")]
		public DateTime? CreatedOn { get; set; }

		[SmartResourceDisplayName("Common.UpdatedOn")]
		public DateTime? UpdatedOn { get; set; }
        
        public IList<ManufacturerLocalizedModel> Locales { get; set; }

		//Store mapping
		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

        #region Nested classes

        public class ManufacturerProductModel : EntityModelBase
        {
            public int ManufacturerId { get; set; }

            public int ProductId { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Products.Fields.Product")]
            public string ProductName { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
			public string Sku { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
			public string ProductTypeName { get; set; }
			public string ProductTypeLabelHint { get; set; }

			[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
			public bool Published { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Products.Fields.IsFeaturedProduct")]
            public bool IsFeaturedProduct { get; set; }

            [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Products.Fields.DisplayOrder")]
            //we don't name it DisplayOrder because Telerik has a small bug 
            //"if we have one more editor with the same name on a page, it doesn't allow editing"
            //in our case it's category.DisplayOrder
            public int DisplayOrder1 { get; set; }
        }

        #endregion
    }

    public class ManufacturerLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Description")]
        [AllowHtml]
        public string Description {get;set;}

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.SeName")]
        [AllowHtml]
        public string SeName { get; set; }
    }
}