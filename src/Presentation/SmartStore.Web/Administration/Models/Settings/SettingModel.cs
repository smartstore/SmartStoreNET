using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Validators.Settings;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Settings
{
    [Validator(typeof(SettingValidator))]
    public class SettingModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Configuration.Settings.AllSettings.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.AllSettings.Fields.Value")]
        [AllowHtml]
        public string Value { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.AllSettings.Fields.StoreName")]
		public string Store { get; set; }
		public int StoreId { get; set; }
    }
}