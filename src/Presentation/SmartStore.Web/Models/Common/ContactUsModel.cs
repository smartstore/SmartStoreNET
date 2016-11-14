using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Validators.Common;

namespace SmartStore.Web.Models.Common
{
	[Validator(typeof(ContactUsValidator))]
    public partial class ContactUsModel : ModelBase
    {
        [SmartResourceDisplayName("ContactUs.PrivacyAgreement")]
        public bool PrivacyAgreement { get; set; }

        public bool DisplayPrivacyAgreement { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("ContactUs.Email")]
		[DataType(DataType.EmailAddress)]
		public string Email { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("ContactUs.Enquiry")]
        public string Enquiry { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("ContactUs.FullName")]
        public string FullName { get; set; }

        public bool SuccessfullySent { get; set; }
        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }

        public string MetaTitle { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
    }
}