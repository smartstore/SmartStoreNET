using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class CampaignController : AdminControllerBase
	{
        private readonly ICampaignService _campaignService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
		private readonly IMessageModelProvider _messageModelProvider;
		private readonly IStoreMappingService _storeMappingService;

        public CampaignController(
			ICampaignService campaignService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
			IMessageModelProvider messageModelProvider,
			IStoreMappingService storeMappingService)
		{
            _campaignService = campaignService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
			_messageModelProvider = messageModelProvider;
			_storeMappingService = storeMappingService;
		}
      
        private void PrepareCampaignModel(CampaignModel model, Campaign campaign, bool excludeProperties)
		{
			if (!excludeProperties)
			{
				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(campaign);
			}

			if (campaign != null)
			{
				model.CreatedOn = Services.DateTimeHelper.ConvertToUserTime(campaign.CreatedOnUtc, DateTimeKind.Utc);
			}

			model.AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
			model.LastModelTree = _messageModelProvider.GetLastModelTree(MessageTemplateNames.SystemCampaign);
		}

		public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

			ViewData["StoreCount"] = Services.StoreService.GetAllStores().Count();

			return View();
		}

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
			var model = new GridModel<CampaignModel>();

			if (Services.Permissions.Authorize(StandardPermissionProvider.ManageCampaigns))
			{
				var campaigns = _campaignService.GetAllCampaigns();

				model.Data = campaigns.Select(x =>
				{
					var m = x.ToModel();
					m.CreatedOn = Services.DateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
					return m;
				});

                model.Total = campaigns.Count;
			}
			else
			{
				model.Data = Enumerable.Empty<CampaignModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        public ActionResult Create()
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var model = new CampaignModel();
			PrepareCampaignModel(model, null, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(CampaignModel model, bool continueEditing)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var campaign = model.ToEntity();
                campaign.CreatedOnUtc = DateTime.UtcNow;
                _campaignService.InsertCampaign(campaign);

				_storeMappingService.SaveStoreMappings<Campaign>(campaign, model.SelectedStoreIds);

                NotifySuccess(T("Admin.Promotions.Campaigns.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
			PrepareCampaignModel(model, null, true);

            return View(model);
        }

		public ActionResult Edit(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                return RedirectToAction("List");
			
            var model = campaign.ToModel();
			PrepareCampaignModel(model, campaign, false);

            return View(model);
		}

        [HttpPost]
        [ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(CampaignModel model, bool continueEditing)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                campaign = model.ToEntity(campaign);
                _campaignService.UpdateCampaign(campaign);

				_storeMappingService.SaveStoreMappings<Campaign>(campaign, model.SelectedStoreIds);

                NotifySuccess(T("Admin.Promotions.Campaigns.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
			PrepareCampaignModel(model, campaign, true);

            return View(model);
		}

		[HttpPost, ActionName("Edit")]
        [FormValueRequired("send-mass-email")]
        public ActionResult SendMassEmail(CampaignModel model)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

			PrepareCampaignModel(model, campaign, false);

            try
            {
                var subscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(null, 0 , int.MaxValue, false);
                var totalEmailsSent = _campaignService.SendCampaign(campaign, subscriptions);

                NotifySuccess(string.Format(T("Admin.Promotions.Campaigns.MassEmailSentToCustomers"), totalEmailsSent), false);
                return View(model);
            }
            catch (Exception exc)
            {
                NotifyError(exc, false);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
        {
            if (!Services.Permissions.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                return RedirectToAction("List");

            _campaignService.DeleteCampaign(campaign);

            NotifySuccess(T("Admin.Promotions.Campaigns.Deleted"));
			return RedirectToAction("List");
		}

		private void DeserializeLastModelTree(MessageTemplate template)
		{
			ViewBag.LastModelTreeJson = template.LastModelTree;
			ViewBag.LastModelTree = _messageModelProvider.GetLastModelTree(template);
		}
	}
}
