using FluentValidation;
using SmartStore.Admin.Models.Affiliates;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Affiliates
{
    public partial class AffiliateValidator : AbstractValidator<AffiliateModel>
    {
        public AffiliateValidator(ILocalizationService localizationService)
        {
        }
    }
}