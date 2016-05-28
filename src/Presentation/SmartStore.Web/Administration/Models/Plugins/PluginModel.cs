using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Plugins;
using SmartStore.Licensing;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Plugins
{
    [Validator(typeof(PluginValidator))]
    public class PluginModel : ModelBase, ILocalizedModel<PluginLocalizedModel>
    {
        public PluginModel()
        {
            Locales = new List<PluginLocalizedModel>();
        }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Group")]
        [AllowHtml]
        public string Group { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]
        [AllowHtml]
        public string FriendlyName { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.SystemName")]
        [AllowHtml]
        public string SystemName { get; set; }

        [SmartResourceDisplayName("Common.Description")]
        [AllowHtml]
        public string Description { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Version")]
        [AllowHtml]
        public string Version { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Author")]
        [AllowHtml]
        public string Author { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.DisplayOrder")] 
        public int DisplayOrder { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Configure")]
        public string ConfigurationUrl { get; set; }

		public string Url { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Installed")]
        public bool Installed { get; set; }

		public string LicenseUrl { get; set; }
		public bool IsLicensable { get; set; }
		public LicensingState LicenseState { get; set; }
		public string TruncatedLicenseKey { get; set; }
		public int? RemainingDemoUsageDays { get; set; }

		public string RemainingDemoUsageDaysLabel
		{
			get
			{
				if (RemainingDemoUsageDays.HasValue)
				{
					if (RemainingDemoUsageDays <= 3)
						return "label-important";

					if (RemainingDemoUsageDays <= 6)
						return "label-warning";
				}
				return "label-success";
			}
		}

		public bool IsConfigurable { get; set; }

		public RouteInfo ConfigurationRoute { get; set; }

        public string IconUrl { get; set; }

        public IList<PluginLocalizedModel> Locales { get; set; }

		public int[] SelectedStoreIds { get; set; }
    }


    public class PluginLocalizedModel : ILocalizedModelLocal
    {
        public int LanguageId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.FriendlyName")]
        [AllowHtml]
        public string FriendlyName { get; set; }

		[SmartResourceDisplayName("Common.Description")]
		[AllowHtml]
		public string Description { get; set; }
    }
}