using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

namespace SmartStore.Admin.Models.Directory
{
    [Validator(typeof(DeliveryTimeValidator))]
    public class DeliveryTimeModel : EntityModelBase, ILocalizedModel<DeliveryTimeLocalizedModel>
    {
        public DeliveryTimeModel()
        {
            Locales = new List<DeliveryTimeLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.Configuration.DeliveryTimes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.DeliveryTimes.Fields.DisplayLocale")]
        [AllowHtml]
        public string DisplayLocale { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.DeliveryTimes.Fields.Color")]
        [AllowHtml]
        public string ColorHexValue { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.DeliveryTimes.Fields.IsDefault")]
        public bool IsDefault { get; set; }
        
        public IList<DeliveryTimeLocalizedModel> Locales { get; set; }
    }

    public class DeliveryTimeLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.DeliveryTimes.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }

    public partial class DeliveryTimeValidator : AbstractValidator<DeliveryTimeModel>
    {
        public DeliveryTimeValidator(Localizer T)
        {
            RuleFor(x => x.Name).NotEmpty().Length(1, 50);
            RuleFor(x => x.ColorHexValue).NotEmpty().Length(1, 50);
            RuleFor(x => x.DisplayLocale)
                .Must(x =>
                {
                    try
                    {
                        if (String.IsNullOrEmpty(x))
                            return true;
                        var culture = new CultureInfo(x);
                        return culture != null;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage(T("Admin.Configuration.DeliveryTimes.Fields.DisplayLocale.Validation"));
        }
    }
}