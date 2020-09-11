using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Rules;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Discounts
{
    [Validator(typeof(DiscountValidator))]
    public class DiscountModel : EntityModelBase
    {
        public DiscountModel()
        {
            AppliedToCategories = new List<AppliedToEntityModel>();
            AppliedToManufacturers = new List<AppliedToEntityModel>();
            AppliedToProducts = new List<AppliedToEntityModel>();
        }

        public int GridPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountType")]
        public int DiscountTypeId { get; set; }
        public string DiscountTypeName { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.UsePercentage")]
        public bool UsePercentage { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountPercentage")]
        public string FormattedDiscountPercentage
        {
            get
            {
                if (UsePercentage)
                {
                    return string.Format("{0:0.##}", DiscountPercentage);
                }

                return string.Empty;
            }
        }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountAmount")]
        public decimal DiscountAmount { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountAmount")]
        public string FormattedDiscountAmount { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.StartDate")]
        public DateTime? StartDateUtc { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.EndDate")]
        public DateTime? EndDateUtc { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.RequiresCouponCode")]
        public bool RequiresCouponCode { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.CouponCode")]
        [AllowHtml]
        public string CouponCode { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.DiscountLimitation")]
        public int DiscountLimitationId { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.LimitationTimes")]
        public int LimitationTimes { get; set; }


        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.AppliedToCategories")]
        public IList<AppliedToEntityModel> AppliedToCategories { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.AppliedToManufacturers")]
        public IList<AppliedToEntityModel> AppliedToManufacturers { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Discounts.Fields.AppliedToProducts")]
        public IList<AppliedToEntityModel> AppliedToProducts { get; set; }


        [UIHint("RuleSets")]
        [AdditionalMetadata("multiple", true)]
        [AdditionalMetadata("scope", RuleScope.Cart)]
        [SmartResourceDisplayName("Admin.Promotions.Discounts.RuleSetRequirements")]
        public int[] SelectedRuleSetIds { get; set; }

        [SmartResourceDisplayName("Admin.Rules.NumberOfRules")]
        public int NumberOfRules { get; set; }

        #region Nested classes

        public class DiscountUsageHistoryModel : EntityModelBase
        {
            public int DiscountId { get; set; }

            [SmartResourceDisplayName("Admin.Promotions.Discounts.History.Order")]
            public int OrderId { get; set; }

            [SmartResourceDisplayName("Common.CreatedOn")]
            public DateTime CreatedOn { get; set; }
        }

        public class AppliedToEntityModel : EntityModelBase
        {
            public string Name { get; set; }
        }

        #endregion
    }

    public partial class DiscountValidator : AbstractValidator<DiscountModel>
    {
        public DiscountValidator()
        {
            RuleFor(x => x.Name).NotNull();
        }
    }
}