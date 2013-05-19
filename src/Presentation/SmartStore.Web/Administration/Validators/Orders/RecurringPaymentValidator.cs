using FluentValidation;
using SmartStore.Admin.Models.Orders;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Orders
{
    public class RecurringPaymentValidator : AbstractValidator<RecurringPaymentModel>
    {
        public RecurringPaymentValidator(ILocalizationService localizationService)
        {
        }
    }
}