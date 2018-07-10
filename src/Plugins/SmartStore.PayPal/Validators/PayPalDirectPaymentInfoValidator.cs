using FluentValidation;
using SmartStore.PayPal.Models;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.PayPal.Validators
{
	public class PaymentInfoValidator : AbstractValidator<PayPalDirectPaymentInfoModel>
	{
		public PaymentInfoValidator(ILocalizationService localizationService) {
			//useful links:
			//http://fluentvalidation.codeplex.com/wikipage?title=Custom&referringTitle=Documentation&ANCHOR#CustomValidator
			//http://benjii.me/2010/11/credit-card-validator-attribute-for-asp-net-mvc-3/

			RuleFor(x => x.CardholderName).NotEmpty().WithMessage(localizationService.GetResource("Payment.CardholderName.Required"));
			RuleFor(x => x.CardNumber).CreditCard().WithMessage(localizationService.GetResource("Payment.CardNumber.Wrong"));
			RuleFor(x => x.CardCode).CreditCardCvvNumber();
			RuleFor(x => x.ExpireMonth).NotEmpty().WithMessage(localizationService.GetResource("Payment.ExpireMonth.Required"));
			RuleFor(x => x.ExpireYear).NotEmpty().WithMessage(localizationService.GetResource("Payment.ExpireYear.Required"));
		}
	}
}