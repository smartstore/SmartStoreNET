﻿using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using FluentValidation.Attributes;
using SmartStore.Plugin.Widgets.TrustedShopsSeal.Validators;

namespace SmartStore.Plugin.Widgets.TrustedShopsSeal.Models
{
    [Validator(typeof(TrustedShopsSealValidator))]
    public class ConfigurationModel : ModelBase
    {
         public ConfigurationModel()
        {
            AvailableZones = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.ContentManagement.Widgets.ChooseZone")]
        public string WidgetZone { get; set; }
        public IList<SelectListItem> AvailableZones { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsSeal.TrustedShopsId")]
        public string TrustedShopsId { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsSeal.ShopName")]
        public string ShopName { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsSeal.ShopText")]
        public string ShopText { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsSeal.IsTestMode")]
        public bool IsTestMode { get; set; }
    }
}