using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.PayPal.Models
{
    public class PayPalDirectPaymentInfoModel : ModelBase
    {
        public PayPalDirectPaymentInfoModel()
        {
            CreditCardTypes = new List<SelectListItem>();
            ExpireMonths = new List<SelectListItem>();
            ExpireYears = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Payment.SelectCreditCard")]
        [AllowHtml]
        public string CreditCardType { get; set; }

        [SmartResourceDisplayName("Payment.SelectCreditCard")]
        public IList<SelectListItem> CreditCardTypes { get; set; }

        [SmartResourceDisplayName("Payment.CardholderName")]
        [AllowHtml]
        public string CardholderName { get; set; }

        [SmartResourceDisplayName("Payment.CardNumber")]
        [AllowHtml]
        public string CardNumber { get; set; }

        [SmartResourceDisplayName("Payment.ExpirationDate")]
        [AllowHtml]
        public string ExpireMonth { get; set; }

        [SmartResourceDisplayName("Payment.ExpirationDate")]
        [AllowHtml]
        public string ExpireYear { get; set; }

        public IList<SelectListItem> ExpireMonths { get; set; }
        public IList<SelectListItem> ExpireYears { get; set; }

        [SmartResourceDisplayName("Payment.CardCode")]
        [AllowHtml]
        public string CardCode { get; set; }
    }

    public class PaymentInfoValidator : AbstractValidator<PayPalDirectPaymentInfoModel>
    {
        public PaymentInfoValidator(Localizer T)
        {
            RuleFor(x => x.CardholderName).NotEmpty();
            RuleFor(x => x.ExpireMonth).NotEmpty();
            RuleFor(x => x.ExpireYear).NotEmpty();
            RuleFor(x => x.CardNumber).CreditCard().WithMessage(T("Payment.CardNumber.Wrong"));
            RuleFor(x => x.CardCode).CreditCardCvvNumber();
        }
    }
}