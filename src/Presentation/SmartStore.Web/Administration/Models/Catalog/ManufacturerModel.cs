using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(ManufacturerValidator))]
    public class ManufacturerModel : TabbableModel, ILocalizedModel<ManufacturerLocalizedModel>
    {
        public ManufacturerModel()
        {
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

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.BottomDescription")]
        [AllowHtml]
        public string BottomDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.ManufacturerTemplate")]
        [AllowHtml]
        public int ManufacturerTemplateId { get; set; }
        public IList<SelectListItem> AvailableManufacturerTemplates { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        public string SeName { get; set; }

        [UIHint("Media"), AdditionalMetadata("album", "catalog")]
        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Picture")]
        public int? PictureId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.PageSize")]
        public int? PageSize { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.AllowCustomersToSelectPageSize")]
        public bool? AllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Deleted")]
        public bool Deleted { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [SmartResourceDisplayName("Common.UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }

        public IList<ManufacturerLocalizedModel> Locales { get; set; }

        // ACL.
        [UIHint("CustomerRoles")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.CustomerRole.LimitedTo")]
        public int[] SelectedCustomerRoleIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.CustomerRole.LimitedTo")]
        public bool SubjectToAcl { get; set; }

        // Store mapping.
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        [UIHint("Discounts")]
        [AdditionalMetadata("multiple", true)]
        [AdditionalMetadata("discountType", DiscountType.AssignedToManufacturers)]
        [SmartResourceDisplayName("Admin.Promotions.Discounts.AppliedDiscounts")]
        public int[] SelectedDiscountIds { get; set; }

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

            [SmartResourceDisplayName("Common.DisplayOrder")]
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
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.BottomDescription")]
        [AllowHtml]
        public string BottomDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaKeywords")]
        [AllowHtml]
        public string MetaKeywords { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaDescription")]
        [AllowHtml]
        public string MetaDescription { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.MetaTitle")]
        [AllowHtml]
        public string MetaTitle { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Seo.SeName")]
        [AllowHtml]
        public string SeName { get; set; }
    }

    public partial class ManufacturerValidator : AbstractValidator<ManufacturerModel>
    {
        public ManufacturerValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    public class ManufacturerMapper :
        IMapper<Manufacturer, ManufacturerModel>,
        IMapper<ManufacturerModel, Manufacturer>
    {
        public void Map(Manufacturer from, ManufacturerModel to)
        {
            MiniMapper.Map(from, to);
            to.SeName = from.GetSeName(0, true, false);
            to.PictureId = from.MediaFileId;
        }

        public void Map(ManufacturerModel from, Manufacturer to)
        {
            MiniMapper.Map(from, to);
            to.MediaFileId = from.PictureId.ZeroToNull();
        }
    }
}