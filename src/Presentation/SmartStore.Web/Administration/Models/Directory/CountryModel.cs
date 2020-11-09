using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.ComponentModel;
using SmartStore.Core.Domain.Directory;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Directory
{
    [Validator(typeof(CountryValidator))]
    public class CountryModel : TabbableModel, ILocalizedModel<CountryLocalizedModel>
    {
        public CountryModel()
        {
            Locales = new List<CountryLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.AllowsBilling")]
        public bool AllowsBilling { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.AllowsShipping")]
        public bool AllowsShipping { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.TwoLetterIsoCode")]
        [AllowHtml]
        public string TwoLetterIsoCode { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.ThreeLetterIsoCode")]
        [AllowHtml]
        public string ThreeLetterIsoCode { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.NumericIsoCode")]
        public int NumericIsoCode { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.SubjectToVat")]
        public bool SubjectToVat { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.DisplayCookieManager")]
        public bool DisplayCookieManager { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.NumberOfStates")]
        public int NumberOfStates { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.AddressFormat")]
        public string AddressFormat { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.DefaultCurrency")]
        public int? DefaultCurrencyId { get; set; }
        public IList<SelectListItem> AllCurrencies { get; set; }

        public IList<CountryLocalizedModel> Locales { get; set; }

        // Store mapping.
        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }
    }

    public class CountryLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Countries.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }
    }

    public partial class CountryValidator : AbstractValidator<CountryModel>
    {
        public CountryValidator()
        {
            RuleFor(x => x.Name).NotNull();
            RuleFor(x => x.TwoLetterIsoCode).NotEmpty();
            RuleFor(x => x.TwoLetterIsoCode).Length(2);
            RuleFor(x => x.ThreeLetterIsoCode).NotEmpty();
            RuleFor(x => x.ThreeLetterIsoCode).Length(3);
        }
    }

    public class CountryMapper :
        IMapper<Country, CountryModel>
    {
        public void Map(Country from, CountryModel to)
        {
            MiniMapper.Map(from, to);
            to.NumberOfStates = from.StateProvinces?.Count ?? 0;
        }
    }
}