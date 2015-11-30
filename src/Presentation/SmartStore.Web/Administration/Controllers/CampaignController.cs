using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class CampaignController : AdminControllerBase
	{
        private readonly ICampaignService _campaignService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmailAccountService _emailAccountService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IPermissionService _permissionService;
		private readonly IStoreService _storeService;
		private readonly IStoreMappingService _storeMappingService;

        public CampaignController(ICampaignService campaignService,
            IDateTimeHelper dateTimeHelper, IEmailAccountService emailAccountService,
            EmailAccountSettings emailAccountSettings,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            ILocalizationService localizationService, IMessageTokenProvider messageTokenProvider,
            IPermissionService permissionService,
			IStoreService storeService,
			IStoreMappingService storeMappingService)
		{
            this._campaignService = campaignService;
            this._dateTimeHelper = dateTimeHelper;
            this._emailAccountService = emailAccountService;
            this._emailAccountSettings = emailAccountSettings;
            this._newsLetterSubscriptionService = newsLetterSubscriptionService;
            this._localizationService = localizationService;
            this._messageTokenProvider = messageTokenProvider;
            this._permissionService = permissionService;
			this._storeService = storeService;
			this._storeMappingService = storeMappingService;
		}

		private void PrepareCampaignModel(CampaignModel model, Campaign campaign, bool excludeProperties)
		{
			model.AvailableStores = _storeService.GetAllStores().Select(s => s.ToModel()).ToList();
			model.AllowedTokens = string.Join(", ", _messageTokenProvider.GetListOfCampaignAllowedTokens());

			if (!excludeProperties)
			{
				if (campaign != null)
					model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(campaign);
				else
					model.SelectedStoreIds = new int[0];
			}

			if (campaign != null)
			{
				model.CreatedOn = _dateTimeHelper.ConvertToUserTime(campaign.CreatedOnUtc, DateTimeKind.Utc);
			}
		}
        
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

			ViewData["StoreCount"] = _storeService.GetAllStores().Count();

			return View();
		}

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaigns = _campaignService.GetAllCampaigns();
            var gridModel = new GridModel<CampaignModel>
            {
                Data = campaigns.Select(x =>
                {
                    var model = x.ToModel();
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    return model;
                }),
                Total = campaigns.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var model = new CampaignModel();
			PrepareCampaignModel(model, null, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Create(CampaignModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var campaign = model.ToEntity();
                campaign.CreatedOnUtc = DateTime.UtcNow;
                _campaignService.InsertCampaign(campaign);

				_storeMappingService.SaveStoreMappings<Campaign>(campaign, model.SelectedStoreIds);

                NotifySuccess(_localizationService.GetResource("Admin.Promotions.Campaigns.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
			PrepareCampaignModel(model, null, true);

            return View(model);
        }

		public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                return RedirectToAction("List");

            var model = campaign.ToModel();
			PrepareCampaignModel(model, campaign, false);

            return View(model);
		}

        [HttpPost]
        [ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(CampaignModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                campaign = model.ToEntity(campaign);
                _campaignService.UpdateCampaign(campaign);

				_storeMappingService.SaveStoreMappings<Campaign>(campaign, model.SelectedStoreIds);

                NotifySuccess(_localizationService.GetResource("Admin.Promotions.Campaigns.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
			PrepareCampaignModel(model, campaign, true);

            return View(model);
		}

        [HttpPost,ActionName("Edit")]
        [FormValueRequired("send-test-email")]
        public ActionResult SendTestEmail(CampaignModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

			PrepareCampaignModel(model, campaign, false);

            try
            {
				var emailAccount = _emailAccountService.GetDefaultEmailAccount();
                if (emailAccount == null)
                    throw new SmartException("Email account could not be loaded");

                var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmail(model.TestEmail);
                if (subscription != null)
                {
                    // there's a subscription. let's use it
                    var subscriptions = new List<NewsLetterSubscription>();
                    subscriptions.Add(subscription);
                    _campaignService.SendCampaign(campaign, emailAccount, subscriptions);
                }
                else
                {
                    //no subscription found
                    _campaignService.SendCampaign(campaign, emailAccount, model.TestEmail);
                }

                NotifySuccess(_localizationService.GetResource("Admin.Promotions.Campaigns.TestEmailSentToCustomers"), false);
                return View(model);
            }
            catch (Exception exc)
            {
                NotifyError(exc, false);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-mass-email")]
        public ActionResult SendMassEmail(CampaignModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

			PrepareCampaignModel(model, campaign, false);

            try
            {
				var emailAccount = _emailAccountService.GetDefaultEmailAccount();
                if (emailAccount == null)
                    throw new SmartException("Email account could not be loaded");

                var subscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(null, 0 , int.MaxValue, false);
                var totalEmailsSent = _campaignService.SendCampaign(campaign, emailAccount, subscriptions);

                NotifySuccess(string.Format(_localizationService.GetResource("Admin.Promotions.Campaigns.MassEmailSentToCustomers"), totalEmailsSent), false);
                return View(model);
            }
            catch (Exception exc)
            {
                NotifyError(exc, false);
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCampaigns))
                return AccessDeniedView();

            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                return RedirectToAction("List");

            _campaignService.DeleteCampaign(campaign);

            NotifySuccess(_localizationService.GetResource("Admin.Promotions.Campaigns.Deleted"));
			return RedirectToAction("List");
		}
	}
}
