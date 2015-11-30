using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Validators.Catalog;

namespace SmartStore.Web.Models.Catalog
{
    [Validator(typeof(ProductAskQuestionValidator))]
    public partial class ProductAskQuestionModel : EntityModelBase
    {
        public string ProductName { get; set; }

        public string ProductSeName { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Account.Fields.Email")]
        public string SenderEmail { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Account.Fields.FullName")]
        public string SenderName { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Account.Fields.Phone")]
        public string SenderPhone { get; set; }

        [AllowHtml]
        [SmartResourceDisplayName("Common.Question")]
        public string Question { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}