using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;


namespace SmartStore.StrubeExport.Models
{
    public class ConfigurationModel : ModelBase
    {


        [SmartResourceDisplayName("Plugins.SmartStore.StrubeExport.MyFirstSetting")]
        [AllowHtml]
        public string MyFirstSetting { get; set; }



    }


}