using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

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

        [SmartResourceDisplayName("Admin.Configuration.Languages.Fields.DisplayOrder")]
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
}