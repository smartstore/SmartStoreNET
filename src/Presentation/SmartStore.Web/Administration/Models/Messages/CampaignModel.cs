using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation.Attributes;
using SmartStore.Admin.Models.Stores;
using SmartStore.Admin.Validators.Messages;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Admin.Models.Messages
{
    [Validator(typeof(CampaignValidator))]
    public class CampaignModel : EntityModelBase
    {
        [SmartResourceDisplayName("Admin.Promotions.Campaigns.Fields.Name")]
        [AllowHtml]
        public string Name { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Campaigns.Fields.Subject")]
        [AllowHtml]
        public string Subject { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Campaigns.Fields.Body")]
        [AllowHtml]
        public string Body { get; set; }
        
        [SmartResourceDisplayName("Admin.Promotions.Campaigns.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Campaigns.Fields.AllowedTokens")]
        public string AllowedTokens { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Campaigns.Fields.TestEmail")]
        [AllowHtml]
        public string TestEmail { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.AvailableFor")]
		public List<StoreModel> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }
    }
}