using FluentValidation;
using FluentValidation.Attributes;
using SmartStore.Collections;
using SmartStore.Services.Messages;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStore.Admin.Models.Messages
{
    [Validator(typeof(CampaignValidator))]
    public class CampaignModel : EntityModelBase, IStoreSelector
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
        
        [SmartResourceDisplayName("Common.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Campaigns.Fields.AllowedTokens")]
        public TreeNode<ModelTreeMember> LastModelTree { get; set; }

        [SmartResourceDisplayName("Admin.Promotions.Campaigns.Fields.TestEmail")]
        [AllowHtml]
        public string TestEmail { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
		public IEnumerable<SelectListItem> AvailableStores { get; set; }
		public int[] SelectedStoreIds { get; set; }
    }

    public partial class CampaignValidator : AbstractValidator<CampaignModel>
    {
        public CampaignValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.Subject).NotEmpty();
            RuleFor(x => x.Body).NotEmpty();
        }
    }
}