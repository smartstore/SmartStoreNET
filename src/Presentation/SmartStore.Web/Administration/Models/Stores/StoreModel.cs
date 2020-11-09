using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Stores
{
    [Validator(typeof(StoreValidator))]
    public partial class StoreModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.Url")]
        [AllowHtml]
        public string Url { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.SslEnabled")]
        public virtual bool SslEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.SecureUrl")]
        [AllowHtml]
        public virtual string SecureUrl { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.ForceSslForAllPages")]
        public bool ForceSslForAllPages { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.Hosts")]
        [AllowHtml]
        public string Hosts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.StoreLogo")]
        [UIHint("Media"), AdditionalMetadata("album", "content")]
        public int LogoMediaFileId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.FavIconMediaFileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content"), AdditionalMetadata("typeFilter", ".ico")]
        public int? FavIconMediaFileId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.PngIconMediaFileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content"), AdditionalMetadata("typeFilter", ".png")]
        public int? PngIconMediaFileId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.AppleTouchIconMediaFileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content"), AdditionalMetadata("typeFilter", "image")]
        public int? AppleTouchIconMediaFileId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.MsTileImageMediaFileId")]
        [UIHint("Media"), AdditionalMetadata("album", "content"), AdditionalMetadata("typeFilter", "image")]
        public int? MsTileImageMediaFileId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.MsTileColor")]
        [UIHint("Color")]
        public string MsTileColor { get; set; }


        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.HtmlBodyId")]
        public string HtmlBodyId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.ContentDeliveryNetwork")]
        [AllowHtml]
        public string ContentDeliveryNetwork { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.PrimaryStoreCurrencyId")]
        public int PrimaryStoreCurrencyId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.PrimaryStoreCurrencyId")]
        public string PrimaryStoreCurrencyName
        {
            get
            {
                try
                {
                    return AvailableCurrencies.First(x => x.Value == PrimaryStoreCurrencyId.ToString()).Text;
                }
                catch { }

                return null;
            }
        }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.PrimaryExchangeRateCurrencyId")]
        public int PrimaryExchangeRateCurrencyId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Stores.Fields.PrimaryExchangeRateCurrencyId")]
        public string PrimaryExchangeRateCurrencyName
        {
            get
            {
                try
                {
                    return AvailableCurrencies.First(x => x.Value == PrimaryExchangeRateCurrencyId.ToString()).Text;
                }
                catch { }

                return null;
            }
        }

        public List<SelectListItem> AvailableCurrencies { get; set; }
    }

    public partial class StoreValidator : AbstractValidator<StoreModel>
    {
        public StoreValidator(Localizer T)
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.Url)
                .Must(x => x.HasValue() && x.IsWebUrl())
                .WithMessage(T("Admin.Validation.Url"));

            RuleFor(x => x.SecureUrl)
                .Must(x => x.HasValue() && x.IsWebUrl())
                .When(x => x.SslEnabled)
                .WithMessage(T("Admin.Validation.Url"));

            RuleFor(x => x.HtmlBodyId).Matches(@"^([A-Za-z])(\w|\-)*$")
                .WithMessage(T("Admin.Configuration.Stores.Fields.HtmlBodyId.Validation"));
        }
    }
}