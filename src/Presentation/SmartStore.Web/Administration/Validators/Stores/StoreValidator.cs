using FluentValidation;
using SmartStore.Admin.Models.Stores;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Stores
{
	public partial class StoreValidator : AbstractValidator<StoreModel>
	{
		public StoreValidator(ILocalizationService localizationService)
		{
			RuleFor(x => x.Name)
				.NotNull()
				.WithMessage(localizationService.GetResource("Admin.Configuration.Stores.Fields.Name.Required"));
			RuleFor(x => x.Url)
				.NotNull()
				.WithMessage(localizationService.GetResource("Admin.Configuration.Stores.Fields.Url.Required"));

			RuleFor(x => x.HtmlBodyId).Matches(@"^([A-Za-z])(\w|\-)*$")
				.WithMessage(localizationService.GetResource("Admin.Configuration.Stores.Fields.HtmlBodyId.Validation"));
		}
	}
}