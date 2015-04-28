using FluentValidation;
using SmartStore.Web.Infrastructure.Installation;
using SmartStore.Web.Models.Install;

namespace SmartStore.Web.Validators.Install
{
    public class InstallValidator : AbstractValidator<InstallModel>
    {
        public InstallValidator(IInstallationLocalizationService locService)
        {
            RuleFor(x => x.AdminEmail).NotEmpty().WithMessage(locService.GetResource("AdminEmailRequired"));
            RuleFor(x => x.AdminEmail).EmailAddress();
            RuleFor(x => x.AdminPassword).NotEmpty().WithMessage(locService.GetResource("AdminPasswordRequired"));
            RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage(locService.GetResource("ConfirmPasswordRequired"));
            RuleFor(x => x.AdminPassword).Equal(x => x.ConfirmPassword).WithMessage(locService.GetResource("PasswordsDoNotMatch"));
            RuleFor(x => x.DataProvider).NotEmpty().WithMessage(locService.GetResource("DataProviderRequired"));
            RuleFor(x => x.PrimaryLanguage).NotEmpty().WithMessage(locService.GetResource("PrimaryLanguageRequired"));
        }
    }
}