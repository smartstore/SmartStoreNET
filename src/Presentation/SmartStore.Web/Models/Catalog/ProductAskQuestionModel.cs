using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Validators.Catalog;

namespace SmartStore.Web.Models.Catalog
{
    [Validator(typeof(ProductAskQuestionValidator))]
    public partial class ProductAskQuestionModel : EntityModelBase
    {
        public LocalizedValue<string> ProductName { get; set; }

        public string ProductSeName { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Account.Fields.Email")]
        public string SenderEmail { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Account.Fields.FullName")]
        public string SenderName { get; set; }
		public bool SenderNameRequired { get; set; }

		[AllowHtml]
        [SmartResourceDisplayName("Account.Fields.Phone")]
        public string SenderPhone { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Common.Question")]
        public string Question { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}