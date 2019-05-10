using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Core.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;

namespace SmartStore.Admin.Models.Localization
{
    [Validator(typeof(LanguageValidator))]
    public class LanguageModel : EntityModelBase, IStoreSelector
    {
        public LanguageModel()
        {
            FlagFileNames = new List<string>();
            AvailableDownloadLanguages = new List<AvailableLanguageModel>();
        }

        [SmartResourceDisplayName("Admin.Configuration.Languages.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Languages.Fields.LanguageCulture")]
        [AllowHtml]
        public string LanguageCulture { get; set; }
		public List<SelectListItem> AvailableCultures { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Languages.Fields.UniqueSeoCode")]
        [AllowHtml]
        public string UniqueSeoCode { get; set; }
		public List<SelectListItem> AvailableTwoLetterLanguageCodes { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Languages.Fields.FlagImageFileName")]
        [AllowHtml]
        public string FlagImageFileName { get; set; }
        public IList<string> FlagFileNames { get; set; }
		public List<SelectListItem> AvailableFlags { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Languages.Fields.Rtl")]
        public bool Rtl { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Languages.Fields.Published")]
        public bool Published { get; set; }

        [SmartResourceDisplayName("Common.DisplayOrder")]
        public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		public IEnumerable<SelectListItem> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Languages.Fields.AvailableLanguageSetId")]
        public int AvailableLanguageSetId { get; set; }
        public List<AvailableLanguageModel> AvailableDownloadLanguages { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Languages.Fields.LastResourcesImportOn")]
		public DateTime? LastResourcesImportOn { get; set; }
		[SmartResourceDisplayName("Admin.Configuration.Languages.Fields.LastResourcesImportOn")]
		public string LastResourcesImportOnString { get; set; }
	}

    public partial class LanguageValidator : AbstractValidator<LanguageModel>
    {
        public LanguageValidator(Localizer T)
        {
            RuleFor(x => x.Name).NotEmpty();

            RuleFor(x => x.LanguageCulture)
                .Must(x =>
                {
                    try
                    {
                        var culture = new CultureInfo(x);
                        return culture != null;
                    }
                    catch
                    {
                        return false;
                    }
                })
                .WithMessage(T("Admin.Configuration.Languages.Fields.LanguageCulture.Validation"));

            RuleFor(x => x.UniqueSeoCode).NotEmpty();
            RuleFor(x => x.UniqueSeoCode)
                //.Length(2)	// Never validates.
                .Length(x => 2);
        }
    }
}