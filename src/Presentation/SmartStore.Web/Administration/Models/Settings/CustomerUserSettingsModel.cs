using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Plugins;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Settings
{
    [Validator(typeof(CustomerUserSettingsValidator))]
    public partial class CustomerUserSettingsModel : ModelBase, ILocalizedModel<CustomerUserSettingsLocalizedModel>
    {
        public CustomerUserSettingsModel()
        {
            CustomerSettings = new CustomerSettingsModel();
            AddressSettings = new AddressSettingsModel();
            ExternalAuthenticationSettings = new ExternalAuthenticationSettingsModel();
            PrivacySettings = new PrivacySettingsModel();
            Locales = new List<CustomerUserSettingsLocalizedModel>();
            PrivacySettings = new PrivacySettingsModel();
        }

        public CustomerSettingsModel CustomerSettings { get; set; }
        public AddressSettingsModel AddressSettings { get; set; }
        public ExternalAuthenticationSettingsModel ExternalAuthenticationSettings { get; set; }
        public PrivacySettingsModel PrivacySettings { get; set; }
        public IList<CustomerUserSettingsLocalizedModel> Locales { get; set; }

        #region Nested classes

        public partial class CustomerSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CustomerLoginType")]
            public CustomerLoginType CustomerLoginType { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CustomerNumberMethod")]
            public CustomerNumberMethod CustomerNumberMethod { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CustomerNumberVisibility")]
            public CustomerNumberVisibility CustomerNumberVisibility { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowUsersToChangeUsernames")]
            public bool AllowUsersToChangeUsernames { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CheckUsernameAvailabilityEnabled")]
            public bool CheckUsernameAvailabilityEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.UserRegistrationType")]
            public UserRegistrationType UserRegistrationType { get; set; }

            [UIHint("CustomerRoles")]
            [AdditionalMetadata("includeSystemRoles", false)]
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.RegisterCustomerRole")]
            public int RegisterCustomerRoleId { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowCustomersToUploadAvatars")]
            public bool AllowCustomersToUploadAvatars { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ShowCustomersLocation")]
            public bool ShowCustomersLocation { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ShowCustomersJoinDate")]
            public bool ShowCustomersJoinDate { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowViewingProfiles")]
            public bool AllowViewingProfiles { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.NotifyNewCustomerRegistration")]
            public bool NotifyNewCustomerRegistration { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.HideDownloadableProductsTab")]
            public bool HideDownloadableProductsTab { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.HideBackInStockSubscriptionsTab")]
            public bool HideBackInStockSubscriptionsTab { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CustomerNameFormat")]
            public CustomerNameFormat CustomerNameFormat { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CustomerNameFormatMaxLength")]
            public int CustomerNameFormatMaxLength { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.NewsletterEnabled")]
            public bool NewsletterEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.HideNewsletterBlock")]
            public bool HideNewsletterBlock { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StoreLastVisitedPage")]
            public bool StoreLastVisitedPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.GenderEnabled")]
            public bool GenderEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.TitleEnabled")]
            public bool TitleEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FirstNameRequired")]
            public bool FirstNameRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.LastNameRequired")]
            public bool LastNameRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DateOfBirthEnabled")]
            public bool DateOfBirthEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CompanyEnabled")]
            public bool CompanyEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CompanyRequired")]
            public bool CompanyRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddressEnabled")]
            public bool StreetAddressEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddressRequired")]
            public bool StreetAddressRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled")]
            public bool StreetAddress2Enabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required")]
            public bool StreetAddress2Required { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ZipPostalCodeEnabled")]
            public bool ZipPostalCodeEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ZipPostalCodeRequired")]
            public bool ZipPostalCodeRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CityEnabled")]
            public bool CityEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CityRequired")]
            public bool CityRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CountryEnabled")]
            public bool CountryEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StateProvinceEnabled")]
            public bool StateProvinceEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StateProvinceRequired")]
            public bool StateProvinceRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PhoneEnabled")]
            public bool PhoneEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PhoneRequired")]
            public bool PhoneRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FaxEnabled")]
            public bool FaxEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FaxRequired")]
            public bool FaxRequired { get; set; }

            #region Password

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DefaultPasswordFormat")]
            public int DefaultPasswordFormat { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PasswordMinLength")]
            public int PasswordMinLength { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.MinDigitsInPassword")]
            public int MinDigitsInPassword { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.MinSpecialCharsInPassword")]
            public int MinSpecialCharsInPassword { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.MinUppercaseCharsInPassword")]
            public int MinUppercaseCharsInPassword { get; set; }

            #endregion
        }

        public partial class AddressSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.SalutationEnabled")]
            public bool SalutationEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Salutations")]
            public string Salutations { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.TitleEnabled")]
            public bool TitleEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CompanyEnabled")]
            public bool CompanyEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CompanyRequired")]
            public bool CompanyRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddressEnabled")]
            public bool StreetAddressEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddressRequired")]
            public bool StreetAddressRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled")]
            public bool StreetAddress2Enabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required")]
            public bool StreetAddress2Required { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ZipPostalCodeEnabled")]
            public bool ZipPostalCodeEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ZipPostalCodeRequired")]
            public bool ZipPostalCodeRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CityEnabled")]
            public bool CityEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CityRequired")]
            public bool CityRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CountryEnabled")]
            public bool CountryEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CountryRequired")]
            public bool CountryRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StateProvinceEnabled")]
            public bool StateProvinceEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StateProvinceRequired")]
            public bool StateProvinceRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PhoneEnabled")]
            public bool PhoneEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PhoneRequired")]
            public bool PhoneRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FaxEnabled")]
            public bool FaxEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FaxRequired")]
            public bool FaxRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ValidateEmailAddress")]
            public bool ValidateEmailAddress { get; set; }
        }

        public partial class ExternalAuthenticationSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ExternalAuthenticationAutoRegisterEnabled")]
            public bool AutoRegisterEnabled { get; set; }
        }

        public partial class PrivacySettingsModel
        {
            public PrivacySettingsModel()
            {
                EnableCookieConsent = true;
                ModalCookieConsent = true;
                CookieInfos = new List<CookieInfo>();
                SameSiteMode = SameSiteType.Lax;
            }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.EnableCookieConsent")]
            public bool EnableCookieConsent { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.ModalCookieConsent")]
            public bool ModalCookieConsent { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.SameSiteMode")]
            public SameSiteType SameSiteMode { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.StoreLastIpAddress")]
            public bool StoreLastIpAddress { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.DisplayGdprConsentOnForms")]
            public bool DisplayGdprConsentOnForms { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.FullNameOnContactUsRequired")]
            public bool FullNameOnContactUsRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.FullNameOnProductRequestRequired")]
            public bool FullNameOnProductRequestRequired { get; set; }

            public List<CookieInfo> CookieInfos { get; set; }
        }

        #endregion
    }

    public class CustomerUserSettingsLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Salutations")]
        public string Salutations { get; set; }
    }

    public partial class CookieInfoModel : ILocalizedModel<CookieInfoLocalizedModel>
    {
        public CookieInfoModel()
        {
            Locales = new List<CookieInfoLocalizedModel>();
        }

        [Required]
        [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.Name")]
        public string Name { get; set; }

        [Required]
        [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.Description")]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.CookieType")]
        public CookieType CookieType { get; set; }

        /// <summary>
        /// Used for display in grid
        /// </summary>
        [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.CookieType")]
        public string CookieTypeName { get; set; }

        /// <summary>
        /// Used to mark which cookie info can be deleted from setting.
        /// </summary>
        [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.IsPluginInfo")]
        public bool IsPluginInfo { get; set; }

        [UIHint("Stores")]
        [AdditionalMetadata("multiple", true)]
        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
        public bool LimitedToStores { get; set; }

        public IList<CookieInfoLocalizedModel> Locales { get; set; }
    }

    public class CookieInfoLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.Name")]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.Privacy.CookieInfo.Description")]
        public string Description { get; set; }
    }

    public partial class CustomerUserSettingsValidator : AbstractValidator<CustomerUserSettingsModel>
    {
        public CustomerUserSettingsValidator()
        {
            RuleFor(x => x.CustomerSettings.PasswordMinLength).GreaterThanOrEqualTo(4);
            RuleFor(x => x.CustomerSettings.MinDigitsInPassword).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CustomerSettings.MinSpecialCharsInPassword).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CustomerSettings.MinUppercaseCharsInPassword).GreaterThanOrEqualTo(0);
        }
    }
}