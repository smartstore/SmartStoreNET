using FluentValidation;
using SmartStore.Admin.Models.Stores;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Stores
{
	public class StoreValidator : AbstractValidator<StoreModel>
	{
		public StoreValidator(ILocalizationService localizationService)
		{
			RuleFor(x => x.Name)
				.NotNull()
				.WithMessage(localizationService.GetResource("Admin.Configuration.Stores.Fields.Name.Required"));
		}
	}
}