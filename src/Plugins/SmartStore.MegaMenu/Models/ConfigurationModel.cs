using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.MegaMenu.Models
{
    public class ConfigurationModel : ModelBase
    {


        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.ConfigSetting")]
        [AllowHtml]
        public string ConfigSetting { get; set; }
    }
}