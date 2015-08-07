using FluentValidation;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.DataExchange
{
	public partial class ExportDeploymentValidator : AbstractValidator<ExportDeploymentModel>
	{
		public ExportDeploymentValidator(ILocalizationService localization)
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.WithMessage(localization.GetResource("Admin.Configuration.Export.Deployment.Name.Validate"));
		}
	}
}