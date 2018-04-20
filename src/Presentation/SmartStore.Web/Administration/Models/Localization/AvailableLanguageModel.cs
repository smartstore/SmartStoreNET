using System;
using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Localization
{
	public class AvailableLanguageListModel : ModelBase
	{
		public string Version { get; set; }
		public int ResourceCount { get; set; }

		public List<AvailableLanguageModel> Languages { get; set; }
	}

	public class AvailableLanguageModel : LanguageModel
    {
        public bool IsInstalled { get; set; }
        public bool IsDownloadRunning { get; set; }

		public int? PreviousSetId { get; set; }
		public string Version { get; set; }
        public string Type { get; set; }
		public DateTime UpdatedOn { get; set; }
        public string UpdatedOnString { get; set; }

		public int TranslatedCount { get; set; }
		public decimal TranslatedPercentage { get; set; }
        public decimal? TranslatedPercentageAtLastImport { get; set; }
	}


	public class LanguageDownloadState
    {
        public int Id { get; set; }
        public int ProgressPercent { get; set; }
        public string ProgressMessage { get; set; }
    }

    internal class LanguageDownloadContext : ModelBase
    {
        public LanguageDownloadContext(int setId)
        {
            Guard.NotZero(setId, nameof(setId));

            SetId = setId;
        }

        public int SetId { get; private set; }
        public CheckAvailableResourcesResult AvailableResources { get;  set; }
    }
}