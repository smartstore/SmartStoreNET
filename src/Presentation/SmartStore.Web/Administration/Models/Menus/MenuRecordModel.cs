using System.Collections.Generic;
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
    public class MenuRecordModel : TabbableModel, ILocalizedModel<MenuRecordLocalizedModel>, IStoreSelector, IAclSelector
    {
        public MenuRecordModel()
        {
            Locales = new List<MenuRecordLocalizedModel>();
            ItemTree = new TreeNode<MenuItem>(new MenuItem());
        }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.SystemName")]
        public string SystemName { get; set; }

        public bool IsSystemMenu { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Title")]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Published")]
        public bool Published { get; set; }

        // Store mapping.
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }
        public IEnumerable<SelectListItem> AvailableStores { get; set; }
        public int[] SelectedStoreIds { get; set; }

        // ACL.
        public bool SubjectToAcl { get; set; }
        public IEnumerable<SelectListItem> AvailableCustomerRoles { get; set; }
        public int[] SelectedCustomerRoleIds { get; set; }

        public IList<MenuRecordLocalizedModel> Locales { get; set; }

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