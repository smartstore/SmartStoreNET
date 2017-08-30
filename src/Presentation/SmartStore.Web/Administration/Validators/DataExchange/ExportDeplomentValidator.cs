using System.IO;
using FluentValidation;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.DataExchange
{
	public partial class ExportDeploymentValidator : AbstractValidator<ExportDeploymentModel>
	{
		public ExportDeploymentValidator(ILocalizationService localization)
		{
			RuleFor(x => x.Name)
				.NotEmpty()
				.WithMessage(localization.GetResource("Admin.Validation.Name"));

			RuleFor(x => x.EmailAddresses)
				.NotEmpty()
				.When(x => x.DeploymentType == ExportDeploymentType.Email)
				.WithMessage(localization.GetResource("Admin.Validation.EmailAddress"));

			RuleFor(x => x.Url)
				.NotEmpty()
				.When(x => x.DeploymentType == ExportDeploymentType.Http || x.DeploymentType == ExportDeploymentType.Ftp)
				.WithMessage(localization.GetResource("Admin.Validation.Url"));

			RuleFor(x => x.Username)
				.NotEmpty()
				.When(x => x.DeploymentType == ExportDeploymentType.Ftp)
				.WithMessage(localization.GetResource("Admin.Validation.UsernamePassword"));

			//RuleFor(x => x.Password)
			//	.NotEmpty()
			//	.When(x => x.DeploymentType == ExportDeploymentType.Ftp)
			//	.WithMessage(localization.GetResource("Admin.Validation.UsernamePassword"));

			RuleFor(x => x.FileSystemPath)
				.Must(x =>
				{
					var isValidPath =
						x.HasValue() &&
						!x.IsCaseInsensitiveEqual("con") &&
						x != "~/" &&
						x != "~" &&
						x.IndexOfAny(Path.GetInvalidPathChars()) == -1;

					return isValidPath;
				})
				.When(x => x.DeploymentType == ExportDeploymentType.FileSystem)
				.WithMessage(localization.GetResource("Admin.Validation.InvalidPath"), x => x.FileSystemPath.NaIfEmpty());
		}
	}
}