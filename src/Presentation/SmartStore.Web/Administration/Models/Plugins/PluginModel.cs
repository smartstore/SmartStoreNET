using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Plugins;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Mvc;

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

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.Installed")]
        public bool Installed { get; set; }

        // codehint: sm-add
        public string IconUrl { get; set; }

        public bool CanChangeEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Plugins.Fields.IsEnabled")]
        public bool IsEnabled { get; set; }

        public IList<PluginLocalizedModel> Locales { get; set; }

		//Store mapping
		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
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