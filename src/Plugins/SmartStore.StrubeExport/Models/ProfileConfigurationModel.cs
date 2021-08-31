using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml.Serialization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.StrubeExport.Models
{
    [CustomModelPart]
    [Serializable]
    public class ProfileConfigurationModel
    {
        [SmartResourceDisplayName("Plugins.SmartStore.StrubeExport.ExportShipAddress")]
        public bool ExportShipAddress { get; set; } = false;

        [SmartResourceDisplayName("Plugins.SmartStore.StrubeExport.SuppressPrice")]
        public bool SuppressPrice { get; set; } = false;

        [SmartResourceDisplayName("Plugins.SmartStore.StrubeExport.SuppressBank")]
        public bool SuppressBank { get; set; } = false;

    }
}