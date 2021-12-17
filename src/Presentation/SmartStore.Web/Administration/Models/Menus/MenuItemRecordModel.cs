using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
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
    [Validator(typeof(MenuItemRecordValidator))]
    public class MenuItemRecordModel : TabbableModel, IIcon, ILocalizedModel<MenuItemRecordLocalizedModel>
    {
        public int MenuId { get; set; }
        public string Model { get; set; }
        public bool ProviderAppendsMultipleItems { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.ParentItem")]
        public int? ParentItemId { get; set; }
        public IList<SelectListItem> AllItems { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.LinkTarget")]
        public string ProviderName { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Title")]
        public string Title { get; set; }
        public string TitlePlaceholder { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.ShortDescription")]
        public string ShortDescription { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.PermissionNames")]
        [UIHint("AccessPermissions")]
        public string[] PermissionNames { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.BeginGroup")]
        public bool BeginGroup { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.ShowExpanded")]
        public bool ShowExpanded { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.NoFollow")]
        public bool NoFollow { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.NewWindow")]
        public bool NewWindow { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.Icon")]
        public string Icon { get; set; }

        public string Style { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.IconColor")]
        public string IconColor { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.HtmlId")]
        public string HtmlId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.CssClass")]
        public string CssClass { get; set; }

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

        public IList<MenuItemRecordLocalizedModel> Locales { get; set; }
    }


    public class MenuItemRecordLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Title")]
        public string Title { get; set; }

        [SmartResourceDisplayName("Admin.ContentManagement.Menus.Item.ShortDescription")]
        public string ShortDescription { get; set; }
    }


    public partial class MenuItemRecordValidator : AbstractValidator<MenuItemRecordModel>
    {
        public MenuItemRecordValidator(Localizer T)
        {
            RuleFor(x => x.ProviderName).NotEmpty();

            RuleFor(x => x.Model)
                .Must(x =>
                {
                    try
                    {
                        if (x.HasValue())
                        {
                            var node = new TreeNode<MenuItem>(new MenuItem());
                            node.ApplyRouteData(x);

                            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                            var result = node.Value.GenerateUrl(urlHelper);

                            return result.HasValue();
                        }
                    }
                    catch { }

                    return false;
                })
                .When(x => x.ProviderName.IsCaseInsensitiveEqual("route"))
                .WithMessage(T("Admin.ContentManagement.Menus.Item.InvalidRouteValues"));
        }
    }
}