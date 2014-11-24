using FluentValidation;
using SmartStore.Admin.Models.Settings;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Settings
{
	public partial class RewardPointsSettingsValidator : AbstractValidator<RewardPointsSettingsModel>
    {
        public RewardPointsSettingsValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.PointsForPurchases_Awarded).NotEqual((int)OrderStatus.Pending)
				.WithMessage(localizationService.GetResource("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Awarded.Pending"));

            RuleFor(x => x.PointsForPurchases_Canceled).NotEqual((int)OrderStatus.Pending)
				.WithMessage(localizationService.GetResource("Admin.Configuration.Settings.RewardPoints.PointsForPurchases_Canceled.Pending"));
        }
    }
}