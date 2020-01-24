using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Collections;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Admin.Models.Menus
{
    [Validator(typeof(MenuRecordValidator))]
    public class MenuRecordModel : TabbableModel, ILocalizedModel<MenuRecordLocalizedModel>
    {
        [SmartResourceDisplayName("Admin.ContentManagement.Menus.SystemName")]
        public string SystemName { get; set; }

        public bool IsSystemMenu { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Template")]
        public string Template { get; set; }
        public bool IsCustomTemplate { get; set; }
        public IList<SelectListItem> AllTemplates { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.WidgetZone")]
        [UIHint("WidgetZone")]
        public string[] WidgetZone { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Title")]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.DisplayOrder")]
        public int DisplayOrder { get; set; }

        // Store mapping.
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        // ACL.
        [UIHint("CustomerRoles")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.CustomerRole.LimitedTo")]
        public int[] SelectedCustomerRoleIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.CustomerRole.LimitedTo")]
        public bool SubjectToAcl { get; set; }

        public IList<MenuRecordLocalizedModel> Locales { get; set; }
        public IList<SelectListItem> AllProviders { get; set; }
        public TreeNode<MenuItem> ItemTree { get; set; }
    }


    public class MenuRecordLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Title")]
        public string Title { get; set; }
    }

    public partial class MenuRecordValidator : AbstractValidator<MenuRecordModel>
    {
        public MenuRecordValidator(Localizer T)
        {
            RuleFor(x => x.SystemName).NotEmpty();
        }
    }
}