using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Menus
{
    [Validator(typeof(MenuRecordValidator))]
    public class MenuRecordModel : EntityModelBase, IStoreSelector, IAclSelector
    {
        [SmartResourceDisplayName("Admin.ContentManagement.Menus.SystemName")]
        public string SystemName { get; set; }

        public bool IsSystemMenu { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Title")]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Published")]
        public bool Published { get; set; }

        // Store mapping
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }
        public IEnumerable<SelectListItem> AvailableStores { get; set; }
        public int[] SelectedStoreIds { get; set; }

        // ACL
        public bool SubjectToAcl { get; set; }
        public IEnumerable<SelectListItem> AvailableCustomerRoles { get; set; }
        public int[] SelectedCustomerRoleIds { get; set; }
    }


    public partial class MenuRecordValidator : AbstractValidator<MenuRecordModel>
    {
        public MenuRecordValidator(Localizer T)
        {
        }
    }
}