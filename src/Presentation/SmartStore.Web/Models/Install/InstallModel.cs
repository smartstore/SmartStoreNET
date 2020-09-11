using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Infrastructure.Installation;

namespace SmartStore.Web.Models.Install
{
    [Validator(typeof(InstallValidator))]
    public partial class InstallModel : ModelBase
    {
        public InstallModel()
        {
            this.AvailableLanguages = new List<SelectListItem>();
            this.AvailableAppLanguages = new List<SelectListItem>();
            this.AvailableMediaStorages = new List<SelectListItem>();
        }

        public string AdminEmail { get; set; }
        [DataType(DataType.Password)]
        public string AdminPassword { get; set; }
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [AllowHtml]
        public string DatabaseConnectionString { get; set; }
        public string DataProvider { get; set; }
        //SQL Server properties
        public string SqlConnectionInfo { get; set; }
        [AllowHtml]
        public string SqlServerName { get; set; }

        public string SqlDatabaseName { get; set; }
        [AllowHtml]
        public string SqlServerUsername { get; set; }
        [AllowHtml]
        public string SqlServerPassword { get; set; }
        public string SqlAuthenticationType { get; set; }
        public bool SqlServerCreateDatabase { get; set; }

        public bool UseCustomCollation { get; set; }
        public string Collation { get; set; }


        public bool InstallSampleData { get; set; }

        public List<SelectListItem> AvailableLanguages { get; set; }

        public string PrimaryLanguage { get; set; }
        public List<SelectListItem> AvailableAppLanguages { get; set; }
        public string MediaStorage { get; set; }
        public List<SelectListItem> AvailableMediaStorages { get; set; }
    }

    public class InstallValidator : AbstractValidator<InstallModel>
    {
        public InstallValidator(IInstallationLocalizationService locService)
        {
            RuleFor(x => x.AdminEmail).NotEmpty();
            RuleFor(x => x.AdminEmail).EmailAddress();
            RuleFor(x => x.AdminPassword).NotEmpty();
            RuleFor(x => x.ConfirmPassword).NotEmpty();
            RuleFor(x => x.AdminPassword).Equal(x => x.ConfirmPassword).WithMessage(locService.GetResource("PasswordsDoNotMatch"));
            RuleFor(x => x.DataProvider).NotEmpty();
            RuleFor(x => x.PrimaryLanguage).NotEmpty();
        }
    }
}