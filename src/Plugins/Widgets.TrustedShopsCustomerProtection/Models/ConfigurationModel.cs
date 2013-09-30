﻿using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using FluentValidation.Attributes;
using SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Validators;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerProtection.Models
{
    [Validator(typeof(TrustedShopsCustomerProtectionValidator))]
    public class ConfigurationModel : ModelBase
    {
         public ConfigurationModel()
        {
            AvailableModes = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerProtection.ProtectionMode")]
        public string ProtectionMode { get; set; }
        public IList<SelectListItem> AvailableModes { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerProtection.TrustedShopsId")]
        public string TrustedShopsId { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerProtection.IsTestMode")]
        public bool IsTestMode { get; set; }

        //[SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerProtection.IsExcellenceMode")]
        //public bool IsExcellenceMode { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerProtection.UserName")]
        public string UserName { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerProtection.Password")]
        public string Password { get; set; }


    }
}