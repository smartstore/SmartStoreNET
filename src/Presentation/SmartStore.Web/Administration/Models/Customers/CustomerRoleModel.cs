using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Collections;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Localization;
using SmartStore.Rules;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Customers
{
    [Validator(typeof(CustomerRoleValidator))]
    public class CustomerRoleModel : EntityModelBase
    {
        public CustomerRoleModel()
        {
            TaxDisplayTypes = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.FreeShipping")]
        [AllowHtml]
        public bool FreeShipping { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.TaxExempt")]
        public bool TaxExempt { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.TaxDisplayType")]
        public int? TaxDisplayType { get; set; }
        public List<SelectListItem> TaxDisplayTypes { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.Active")]
        public bool Active { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.IsSystemRole")]
        public bool IsSystemRole { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.SystemName")]
        public string SystemName { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.MinOrderTotal")]
        public decimal? OrderTotalMinimum { get; set; }

        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.Fields.MaxOrderTotal")]
        public decimal? OrderTotalMaximum { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }

        [UIHint("RuleSets")]
        [AdditionalMetadata("multiple", true)]
        [AdditionalMetadata("scope", RuleScope.Customer)]
        [SmartResourceDisplayName("Admin.Customers.CustomerRoles.AutomatedAssignmentRules")]
        public int[] SelectedRuleSetIds { get; set; }
        public bool ShowRuleApplyButton { get; set; }

        public TreeNode<IPermissionNode> PermissionTree { get; set; }

        public int GridPageSize { get; set; }
        public bool UsernamesEnabled { get; set; }
    }


    public partial class CustomerRoleValidator : AbstractValidator<CustomerRoleModel>
    {
        public CustomerRoleValidator(Localizer T)
        {
            RuleFor(x => x.Name).NotNull();

            //RuleFor(x => x.OrderTotalMinimum).GreaterThan(0).When(x => x.OrderTotalMinimum.HasValue)
            //    .WithMessage(T("Admin.Validation.ValueGreaterZero"));

            //RuleFor(x => x.OrderTotalMaximum).GreaterThan(0).When(x => x.OrderTotalMaximum.HasValue)
            //    .WithMessage(T("Admin.Validation.ValueGreaterZero"));

            //RuleFor(x => x.OrderTotalMaximum).GreaterThan(x => x.OrderTotalMinimum ?? 0)
            //    .When(x => x.OrderTotalMaximum.HasValue && x.OrderTotalMinimum.HasValue)
            //    .WithMessage(string.Format(
            //        T("Admin.Validation.ValueGreaterThan"),
            //        T("Admin.Customers.CustomerRoles.Fields.MinOrderTotal"))
            //    );
            RuleFor(x => x.OrderTotalMaximum)
                .GreaterThan(x => x.OrderTotalMinimum ?? 0);
        }
    }
}