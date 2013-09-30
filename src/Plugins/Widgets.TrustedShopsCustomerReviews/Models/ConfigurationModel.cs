﻿using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;
using FluentValidation.Attributes;
using SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Validators;

namespace SmartStore.Plugin.Widgets.TrustedShopsCustomerReviews.Models
{
    [Validator(typeof(TrustedShopsCustomerReviewsValidator))]
    public class ConfigurationModel : ModelBase
    {
         public ConfigurationModel()
        {
            AvailableZones = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Admin.ContentManagement.Widgets.ChooseZone")]
        public string ZoneId { get; set; }
        public IList<SelectListItem> AvailableZones { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerReviews.TrustedShopsId")]
        public string TrustedShopsId { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerReviews.TrustedShopsActivation")]
        public bool TrustedShopsActivation { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerReviews.ShopName")]
        public string ShopName { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerReviews.IsTestMode")]
        public bool IsTestMode { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerReviews.DisplayWidget")]
        public bool DisplayWidget { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerReviews.DisplayReviewLinkOnOrderCompleted")]
        public bool DisplayReviewLinkOnOrderCompleted { get; set; }

        [SmartResourceDisplayName("Plugins.Widgets.TrustedShopsCustomerReviews.DisplayReviewLinkInEmail")]
        public bool DisplayReviewLinkInEmail { get; set; }

    }
}