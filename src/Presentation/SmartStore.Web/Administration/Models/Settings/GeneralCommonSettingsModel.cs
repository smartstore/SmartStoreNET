using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Seo;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Admin.Validators.Settings;
using FluentValidation.Attributes;
using SmartStore.Core.Domain.Localization;

namespace SmartStore.Admin.Models.Settings
{
    [Validator(typeof(GeneralCommonSettingsValidator))]
	public partial class GeneralCommonSettingsModel : ModelBase
    {
        public GeneralCommonSettingsModel()
        {
            StoreInformationSettings = new StoreInformationSettingsModel();
            SeoSettings = new SeoSettingsModel();
            SecuritySettings = new SecuritySettingsModel();
            PdfSettings = new PdfSettingsModel();
            LocalizationSettings = new LocalizationSettingsModel(); 
            FullTextSettings = new FullTextSettingsModel();
            //codehint: sm-add begin
            CompanyInformationSettings = new CompanyInformationSettingsModel();
            ContactDataSettings = new ContactDataSettingsModel();
            BankConnectionSettings = new BankConnectionSettingsModel();
            SocialSettings = new SocialSettingsModel();
            //codehint: sm-add end
        }

        public StoreInformationSettingsModel StoreInformationSettings { get; set; }
        public SeoSettingsModel SeoSettings { get; set; }
        public SecuritySettingsModel SecuritySettings { get; set; }
        public PdfSettingsModel PdfSettings { get; set; }
        public LocalizationSettingsModel LocalizationSettings { get; set; }
        public FullTextSettingsModel FullTextSettings { get; set; }
        //codehint: sm-add begin
        public CompanyInformationSettingsModel CompanyInformationSettings { get; set; }
        public ContactDataSettingsModel ContactDataSettings { get; set; }
        public BankConnectionSettingsModel BankConnectionSettings { get; set; }
        public SocialSettingsModel SocialSettings { get; set; }
        //codehint: sm-add end

        #region Nested classes

		public partial class StoreInformationSettingsModel
        {
            public StoreInformationSettingsModel()
            {
                // codehint: sm-delete
            }
            
            // codehint: sm-delete

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.StoreClosed")]
			public bool StoreClosed { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.StoreClosedAllowForAdmins")]
            public bool StoreClosedAllowForAdmins { get; set; }
            
            // codehint: sm-delete
        }

		public partial class SeoSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.PageTitleSeparator")]
            [AllowHtml]
            public string PageTitleSeparator { get; set; }
            
            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.PageTitleSeoAdjustment")]
            public PageTitleSeoAdjustment PageTitleSeoAdjustment { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DefaultTitle")]
            [AllowHtml]
            public string DefaultTitle { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DefaultMetaKeywords")]
            [AllowHtml]
            public string DefaultMetaKeywords { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DefaultMetaDescription")]
            [AllowHtml]
            public string DefaultMetaDescription { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ConvertNonWesternChars")]
            public bool ConvertNonWesternChars { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CanonicalUrlsEnabled")]
            public bool CanonicalUrlsEnabled { get; set; }
        }

		public partial class SecuritySettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.EncryptionKey")]
            [AllowHtml]
            public string EncryptionKey { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.AdminAreaAllowedIpAddresses")]
            [AllowHtml]
            public string AdminAreaAllowedIpAddresses { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.HideAdminMenuItemsBasedOnPermissions")]
            public bool HideAdminMenuItemsBasedOnPermissions { get; set; }




            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaEnabled")]
            public bool CaptchaEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnLoginPage")]
            public bool CaptchaShowOnLoginPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnRegistrationPage")]
            public bool CaptchaShowOnRegistrationPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnContactUsPage")]
            public bool CaptchaShowOnContactUsPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnEmailWishlistToFriendPage")]
            public bool CaptchaShowOnEmailWishlistToFriendPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnEmailProductToFriendPage")]
            public bool CaptchaShowOnEmailProductToFriendPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnAskQuestionPage")]
            public bool CaptchaShowOnAskQuestionPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnBlogCommentPage")]
            public bool CaptchaShowOnBlogCommentPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnNewsCommentPage")]
            public bool CaptchaShowOnNewsCommentPage { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CaptchaShowOnProductReviewPage")]
            public bool CaptchaShowOnProductReviewPage { get; set; }
            
            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.reCaptchaPublicKey")]
            [AllowHtml]
            public string ReCaptchaPublicKey { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.reCaptchaPrivateKey")]
            [AllowHtml]
            public string ReCaptchaPrivateKey { get; set; }
        }

		public partial class PdfSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.PdfEnabled")]
            public bool Enabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.PdfLetterPageSizeEnabled")]
            public bool LetterPageSizeEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.PdfLogo")]
            [UIHint("Picture")]
            public int LogoPictureId { get; set; }
        }

		public partial class LocalizationSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.UseImagesForLanguageSelection")]
            public bool UseImagesForLanguageSelection { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.SeoFriendlyUrlsForLanguagesEnabled")]
            public bool SeoFriendlyUrlsForLanguagesEnabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.LoadAllLocaleRecordsOnStartup")]
            public bool LoadAllLocaleRecordsOnStartup { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DefaultLanguageRedirectBehaviour")]
            public DefaultLanguageRedirectBehaviour DefaultLanguageRedirectBehaviour { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.InvalidLanguageRedirectBehaviour")]
            public InvalidLanguageRedirectBehaviour InvalidLanguageRedirectBehaviour { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.DetectBrowserUserLanguage")]
            public bool DetectBrowserUserLanguage { get; set; }
        }

		public partial class FullTextSettingsModel
        {
            public bool Supported { get; set; }

            public bool Enabled { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.FullTextSettings.SearchMode")]
            public FulltextSearchMode SearchMode { get; set; }
            public SelectList SearchModeValues { get; set; }
        }

        //codehint: sm-add begin
		public partial class CompanyInformationSettingsModel
        {

            public CompanyInformationSettingsModel()
            {
                AvailableCountries = new List<SelectListItem>();
                Salutations = new List<SelectListItem>();
                ManagementDescriptions = new List<SelectListItem>();
            }

            public IList<SelectListItem> AvailableCountries { get; set; }
            public IList<SelectListItem> Salutations { get; set; }
            public IList<SelectListItem> ManagementDescriptions { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.CompanyName")]
            public string CompanyName { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.Salutation")]
            public string Salutation { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.Title")]
            public string Title { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.Firstname")]
            public string Firstname { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.Lastname")]
            public string Lastname { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.CompanyManagementDescription")]
            public string CompanyManagementDescription { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.CompanyManagement")]
            public string CompanyManagement { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.Street")]
            public string Street { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.Street2")]
            public string Street2 { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.ZipCode")]
            public string ZipCode { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.Location")]
            public string City { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.Country")]
            public int CountryId { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.Country")]
            [AllowHtml]
            public string CountryName { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.State")]
            public string Region { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.VatId")]
            public string VatId { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.CommercialRegister")]
            public string CommercialRegister { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.CompanyInformationSettings.TaxNumber")]
            public string TaxNumber { get; set; }
        }

		public partial class ContactDataSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ContactDataSettings.CompanyTelephoneNumber")]
            public string CompanyTelephoneNumber { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ContactDataSettings.HotlineTelephoneNumber")]
            public string HotlineTelephoneNumber { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ContactDataSettings.MobileTelephoneNumber")]
            public string MobileTelephoneNumber { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ContactDataSettings.CompanyFaxNumber")]
            public string CompanyFaxNumber { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ContactDataSettings.CompanyEmailAddress")]
            public string CompanyEmailAddress { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ContactDataSettings.WebmasterEmailAddress")]
            public string WebmasterEmailAddress { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ContactDataSettings.SupportEmailAddress")]
            public string SupportEmailAddress { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.ContactDataSettings.ContactEmailAddress")]
            public string ContactEmailAddress { get; set; }
        }

		public partial class BankConnectionSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.BankConnectionSettings.Bankname")]
            public string Bankname { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.BankConnectionSettings.Bankcode")]
            public string Bankcode { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.BankConnectionSettings.AccountNumber")]
            public string AccountNumber { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.BankConnectionSettings.AccountHolder")]
            public string AccountHolder { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.BankConnectionSettings.Iban")]
            public string Iban { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.BankConnectionSettings.Bic")]
            public string Bic { get; set; }
        }

		public partial class SocialSettingsModel
        {
            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.SocialSettings.ShowSocialLinksInFooter")]
            public bool ShowSocialLinksInFooter { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.SocialSettings.FacebookLink")]
            public string FacebookLink { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.SocialSettings.GooglePlusLink")]
            public string GooglePlusLink { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.SocialSettings.TwitterLink")]
            public string TwitterLink { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.SocialSettings.PinterestLink")]
            public string PinterestLink { get; set; }

            [SmartResourceDisplayName("Admin.Configuration.Settings.GeneralCommon.SocialSettings.YoutubeLink")]
            public string YoutubeLink { get; set; }
        }

        //codehint: sm-add end
        #endregion
    }
}