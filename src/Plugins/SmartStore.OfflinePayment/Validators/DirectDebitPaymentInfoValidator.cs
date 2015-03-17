using FluentValidation;
using SmartStore.OfflinePayment.Models;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Validators;

namespace SmartStore.OfflinePayment.Validators
{
    public class DirectDebitPaymentInfoValidator : AbstractValidator<DirectDebitPaymentInfoModel>
    {
		public DirectDebitPaymentInfoValidator(ILocalizationService localize)
        {
			RuleFor(x => x.DirectDebitAccountHolder).NotEmpty()
				.WithMessage(localize.GetResource("Plugins.Payments.DirectDebit.DirectDebitAccountHolderRequired"));


			RuleFor(x => x.DirectDebitAccountNumber).NotEmpty().When(x => x.EnterIBAN == "no-iban")
				.WithMessage(localize.GetResource("Plugins.Payments.DirectDebit.DirectDebitAccountNumberRequired"));

			RuleFor(x => x.DirectDebitBankCode).NotEmpty().When(x => x.EnterIBAN == "no-iban")
				.WithMessage(localize.GetResource("Plugins.Payments.DirectDebit.DirectDebitBankCodeRequired"));


			RuleFor(x => x.DirectDebitIban).Matches(RegularExpressions.IsIban).When(x => x.EnterIBAN == "iban")
				.WithMessage(localize.GetResource("Plugins.Payments.DirectDebit.DirectDebitIbanRequired"));

			RuleFor(x => x.DirectDebitBic).Matches(RegularExpressions.IsBic).When(x => x.EnterIBAN == "iban")
				.WithMessage(localize.GetResource("Plugins.Payments.DirectDebit.DirectDebitBicRequired"));
        }}
}