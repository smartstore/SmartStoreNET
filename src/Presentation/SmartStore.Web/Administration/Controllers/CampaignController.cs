using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Security;
using SmartStore.Services.Messages;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

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

            model.LastModelTree = _messageModelProvider.GetLastModelTree(MessageTemplateNames.SystemCampaign);
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Promotion.Campaign.Read)]
        public ActionResult List()
        {
            ViewData["StoreCount"] = Services.StoreService.GetAllStores().Count();

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Campaign.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<CampaignModel>();

            var campaigns = _campaignService.GetAllCampaigns();

            model.Data = campaigns.Select(x =>
            {
                var m = x.ToModel();
                m.CreatedOn = Services.DateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                return m;
            });

            model.Total = campaigns.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Promotion.Campaign.Create)]
        public ActionResult Create()
        {
            var model = new CampaignModel();
            PrepareCampaignModel(model, null, false);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Promotion.Campaign.Create)]
        public ActionResult Create(CampaignModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var campaign = model.ToEntity();
                campaign.CreatedOnUtc = DateTime.UtcNow;
                _campaignService.InsertCampaign(campaign);

                SaveStoreMappings(campaign, model.SelectedStoreIds);

                NotifySuccess(T("Admin.Promotions.Campaigns.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            PrepareCampaignModel(model, null, true);

            return View(model);
        }

        [Permission(Permissions.Promotion.Campaign.Read)]
        public ActionResult Edit(int id)
        {
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
        [Permission(Permissions.Promotion.Campaign.Update)]
        public ActionResult Edit(CampaignModel model, bool continueEditing)
        {
            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                campaign = model.ToEntity(campaign);
                _campaignService.UpdateCampaign(campaign);

                SaveStoreMappings(campaign, model.SelectedStoreIds);

                NotifySuccess(T("Admin.Promotions.Campaigns.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            PrepareCampaignModel(model, campaign, true);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-mass-email")]
        [Permission(Permissions.System.Message.Send)]
        public ActionResult SendMassEmail(CampaignModel model)
        {
            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

            PrepareCampaignModel(model, campaign, false);

            try
            {
                var subscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(null, 0, int.MaxValue, false);
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
        [Permission(Permissions.Promotion.Campaign.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
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
