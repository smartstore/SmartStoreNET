using FluentValidation;
using SmartStore.PayPal.Models;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.PayPal.Validators
{
	public class PayPalExpressPaymentInfoValidator : AbstractValidator<PayPalExpressPaymentInfoModel>
	{
		public PayPalExpressPaymentInfoValidator(ILocalizationService localizationService) {

		}
	}
}