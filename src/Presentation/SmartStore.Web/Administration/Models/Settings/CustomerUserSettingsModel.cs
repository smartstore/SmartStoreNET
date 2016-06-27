using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    public partial class CustomerUserSettingsModel
    {
        public CustomerUserSettingsModel()
        {
            CustomerSettings = new CustomerSettingsModel();
            AddressSettings = new AddressSettingsModel();
            DateTimeSettings = new DateTimeSettingsModel();
            ExternalAuthenticationSettings = new ExternalAuthenticationSettingsModel();
        }
        public CustomerSettingsModel CustomerSettings { get; set; }
        public AddressSettingsModel AddressSettings { get; set; }
        public DateTimeSettingsModel DateTimeSettings { get; set; }
        public ExternalAuthenticationSettingsModel ExternalAuthenticationSettings { get; set; }

        #region Nested classes

        public partial class CustomerSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.UsernamesEnabled")]
            public bool UsernamesEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CustomerNumberMethod")]
            public CustomerNumberMethod CustomerNumberMethod { get; set; }
            public SelectList AvailableCustomerNumberMethods { get; set; }
			public IList<SelectListItem> AvailableRegisterCustomerRoles { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CustomerNumberVisibility")]
            public CustomerNumberVisibility CustomerNumberVisibility { get; set; }
            public SelectList AvailableCustomerNumberVisibilities { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowUsersToChangeUsernames")]
            public bool AllowUsersToChangeUsernames { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CheckUsernameAvailabilityEnabled")]
            public bool CheckUsernameAvailabilityEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.UserRegistrationType")]
            public int UserRegistrationType { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.RegisterCustomerRole")]
			public int RegisterCustomerRoleId { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowCustomersToUploadAvatars")]
            public bool AllowCustomersToUploadAvatars { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DefaultAvatarEnabled")]
            public bool DefaultAvatarEnabled { get; set; }

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
            public int CustomerNameFormat { get; set; }

			[SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.CustomerNameFormatMaxLength")]
			public int CustomerNameFormatMaxLength { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DefaultPasswordFormat")]
            public int DefaultPasswordFormat { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.NewsletterEnabled")]
            public bool NewsletterEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.HideNewsletterBlock")]
            public bool HideNewsletterBlock { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.StoreLastVisitedPage")]
            public bool StoreLastVisitedPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DisplayPrivacyAgreementOnContactUs")]
            public bool DisplayPrivacyAgreementOnContactUs { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.GenderEnabled")]
            public bool GenderEnabled { get; set; }

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

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PhoneEnabled")]
            public bool PhoneEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.PhoneRequired")]
            public bool PhoneRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FaxEnabled")]
            public bool FaxEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.FaxRequired")]
            public bool FaxRequired { get; set; }
		}

        public partial class AddressSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyEnabled")]
            public bool CompanyEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyRequired")]
            public bool CompanyRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressEnabled")]
            public bool StreetAddressEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressRequired")]
            public bool StreetAddressRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled")]
            public bool StreetAddress2Enabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required")]
            public bool StreetAddress2Required { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeEnabled")]
            public bool ZipPostalCodeEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeRequired")]
            public bool ZipPostalCodeRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityEnabled")]
            public bool CityEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityRequired")]
            public bool CityRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryEnabled")]
            public bool CountryEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceEnabled")]
            public bool StateProvinceEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.PhoneEnabled")]
            public bool PhoneEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.PhoneRequired")]
            public bool PhoneRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxEnabled")]
            public bool FaxEnabled { get; set; }
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxRequired")]
            public bool FaxRequired { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ValidateEmailAddress")]
            public bool ValidateEmailAddress { get; set; }
        }

        public partial class DateTimeSettingsModel
        {
            public DateTimeSettingsModel()
            {
                AvailableTimeZones = new List<SelectListItem>();
            }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowCustomersToSetTimeZone")]
            public bool AllowCustomersToSetTimeZone { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DefaultStoreTimeZone")]
            public string DefaultStoreTimeZoneId { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.DefaultStoreTimeZone")]
            public IList<SelectListItem> AvailableTimeZones { get; set; }
        }

        public partial class ExternalAuthenticationSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.CustomerUser.ExternalAuthenticationAutoRegisterEnabled")]
            public bool AutoRegisterEnabled { get; set; }
        }
        #endregion
    }
}