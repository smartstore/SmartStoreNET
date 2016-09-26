using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.MegaMenu.Models
{
    public class ConfigurationModel : ModelBase
    {
        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.ProductRotatorInterval")]
        public int ProductRotatorInterval { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.ProductRotatorDuration")]
        public int ProductRotatorDuration { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.ProductRotatorCycle")]
        public bool ProductRotatorCycle { get; set; }

        [SmartResourceDisplayName("Plugins.SmartStore.MegaMenu.MenuMinHeight")]
        public int MenuMinHeight { get; set; }
    }
}