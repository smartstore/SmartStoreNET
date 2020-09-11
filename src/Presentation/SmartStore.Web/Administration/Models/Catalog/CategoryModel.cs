using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Rules;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Catalog
{
    [Validator(typeof(CategoryValidator))]
    public class CategoryModel : TabbableModel, ILocalizedModel<CategoryLocalizedModel>
    {
        public CategoryModel()
        {
            Locales = new List<CategoryLocalizedModel>();
            AvailableCategoryTemplates = new List<SelectListItem>();
            AvailableDefaultViewModes = new List<SelectListItem>();
            AvailableBadgeStyles = new List<SelectListItem>();
        }

        public int GridPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.FullName")]
        public string FullName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.BottomDescription")]
        [AllowHtml]
        public string BottomDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.ExternalLink")]
        [AllowHtml, UIHint("Link")]
        public string ExternalLink { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.BadgeText")]
        [AllowHtml]
        public string BadgeText { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.BadgeStyle")]
        public int BadgeStyle { get; set; }
        public IList<SelectListItem> AvailableBadgeStyles { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Alias")]
        public string Alias { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.CategoryTemplate")]
        [AllowHtml]
        public int CategoryTemplateId { get; set; }
        public IList<SelectListItem> AvailableCategoryTemplates { get; set; }

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

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Parent")]
        public int? ParentCategoryId { get; set; }

        [UIHint("Media"), AdditionalMetadata("album", "catalog")]
        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Picture")]
        public int? PictureId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.PageSize")]
        public int? PageSize { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.AllowCustomersToSelectPageSize")]
        public bool? AllowCustomersToSelectPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.PageSizeOptions")]
        public string PageSizeOptions { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.ShowOnHomePage")]
        public bool ShowOnHomePage { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Deleted")]
        public bool Deleted { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime? CreatedOn { get; set; }

        [SmartResourceDisplayName("Common.UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }

        public IList<CategoryLocalizedModel> Locales { get; set; }

        public string Breadcrumb { get; set; }

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

        public string ParentCategoryBreadcrumb { get; set; }

        [UIHint("Discounts")]
        [AdditionalMetadata("multiple", true)]
        [AdditionalMetadata("discountType", DiscountType.AssignedToCategories)]
        [SmartResourceDisplayName("Admin.Promotions.Discounts.AppliedDiscounts")]
        public int[] SelectedDiscountIds { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Catalog.DefaultViewMode")]
        public string DefaultViewMode { get; set; }
        public IList<SelectListItem> AvailableDefaultViewModes { get; private set; }

        [UIHint("RuleSets")]
        [AdditionalMetadata("multiple", true)]
        [AdditionalMetadata("scope", RuleScope.Product)]
        [SmartResourceDisplayName("Admin.Catalog.Categories.AutomatedAssignmentRules")]
        public int[] SelectedRuleSetIds { get; set; }
        public bool ShowRuleApplyButton { get; set; }

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

            // We don't name it DisplayOrder because Telerik has a small bug 
            // "if we have one more editor with the same name on a page, it doesn't allow editing".
            // In our case it's category.DisplayOrder.
            [SmartResourceDisplayName("Common.DisplayOrder")]
            public int DisplayOrder1 { get; set; }

            [SmartResourceDisplayName("Admin.Rules.AddedByRule")]
            public bool IsSystemMapping { get; set; }
        }

        #endregion
    }

    public class CategoryLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.FullName")]
        public string FullName { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.BottomDescription")]
        [AllowHtml]
        public string BottomDescription { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Categories.Fields.BadgeText")]
        [AllowHtml]
        public string BadgeText { get; set; }

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

    public partial class CategoryValidator : AbstractValidator<CategoryModel>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
        }
    }

    public class CategoryMapper :
        IMapper<Category, CategoryModel>,
        IMapper<CategoryModel, Category>
    {
        public void Map(Category from, CategoryModel to)
        {
            MiniMapper.Map(from, to);
            to.SeName = from.GetSeName(0, true, false);
            to.PictureId = from.MediaFileId;
        }

        public void Map(CategoryModel from, Category to)
        {
            MiniMapper.Map(from, to);
            to.MediaFileId = from.PictureId.ZeroToNull();
        }
    }
}