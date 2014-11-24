using FluentValidation;
using SmartStore.Admin.Models.Plugins;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Plugins
{
	public partial class PluginValidator : AbstractValidator<PluginModel>
    {
        public PluginValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FriendlyName).NotNull().WithMessage(localizationService.GetResource("Admin.Configuration.Plugins.Fields.FriendlyName.Required"));
        }
    }
}