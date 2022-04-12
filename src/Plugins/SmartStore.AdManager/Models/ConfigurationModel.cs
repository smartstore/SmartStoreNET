using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace SmartStore.AdManager.Models
{
    public class ConfigurationModel : ModelBase
    {


        [SmartResourceDisplayName("Plugins.SmartStore.AdManager.MyFirstSetting")]
        [AllowHtml]
        public string MyFirstSetting { get; set; }



    }


}