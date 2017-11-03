using System;
using System.Collections.Generic;

namespace SmartStore.Admin.Models.Localization
{
    [Serializable]
    public class CheckAvailableResourcesResult
    {
        public CheckAvailableResourcesResult()
        {
            Resources = new List<AvailableResourcesModel>();
        }

        public string AppId { get; set; }
        public string Version { get; set; }

        public List<AvailableResourcesModel> Resources { get; set; }
    }

    [Serializable]
    public class AvailableResourcesModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Type { get; set; }
        public string DownloadUrl { get; set; }
        public List<string> PluginSystemNames { get; set; }
        public DateTime UpdatedOn { get; set; }

        public LanguageModel Language { get; set; }
        public AggregationModel Aggregation { get; set; }

        [Serializable]
        public class LanguageModel
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Culture { get; set; }
            public string TwoLetterIsoCode { get; set; }
            public bool Rtl { get; set; }
        }

        [Serializable]
        public class AggregationModel
        {
            public int SetId { get; set; }
            public int NumberOfResources { get; set; }
            public int NumberOfTouched { get; set; }
            public decimal TouchedPercentage { get; set; }
        }
    }
}