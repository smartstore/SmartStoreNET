using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Orders;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Orders
{
    [Validator(typeof(CheckoutAttributeValueValidator))]
    public class CheckoutAttributeValueModel : EntityModelBase, ILocalizedModel<CheckoutAttributeValueLocalizedModel>
    {
        public CheckoutAttributeValueModel()
        {
            Locales = new List<CheckoutAttributeValueLocalizedModel>();
        }

        public int CheckoutAttributeId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.PriceAdjustment")]
        public decimal PriceAdjustment { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.WeightAdjustment")]
        public decimal WeightAdjustment { get; set; }
        public string BaseWeightIn { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.DisplayOrder")]
        public int DisplayOrder {get;set;}

        public IList<CheckoutAttributeValueLocalizedModel> Locales { get; set; }

    }

    public class CheckoutAttributeValueLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Catalog.Attributes.CheckoutAttributes.Values.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }
}