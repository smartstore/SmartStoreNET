using FluentValidation;
using SmartStore.Admin.Models.Orders;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Orders
{
	public partial class ReturnRequestValidator : AbstractValidator<ReturnRequestModel>
    {
        public ReturnRequestValidator(ILocalizationService localizationService)
        {
        }
    }
}