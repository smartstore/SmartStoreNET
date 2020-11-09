using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Security;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
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
        private readonly IMessageModelProvider _messageModelProvider;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;

        public CampaignController(
            ICampaignService campaignService,
            IMessageModelProvider messageModelProvider,
            IStoreMappingService storeMappingService,
            IAclService aclService)
        {
            _campaignService = campaignService;
            _messageModelProvider = messageModelProvider;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
        }

        private void PrepareCampaignModel(CampaignModel model, Campaign campaign)
        {
            if (campaign != null)
            {
                model.CreatedOn = Services.DateTimeHelper.ConvertToUserTime(campaign.CreatedOnUtc, DateTimeKind.Utc);
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(campaign);
                model.SelectedCustomerRoleIds = _aclService.GetCustomerRoleIdsWithAccessTo(campaign);
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
            PrepareCampaignModel(model, null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Promotion.Campaign.Create)]
        public ActionResult Create(CampaignModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var campaign = model.ToEntity();
                campaign.CreatedOnUtc = DateTime.UtcNow;
                _campaignService.InsertCampaign(campaign);

                SaveAclMappings(campaign, model.SelectedCustomerRoleIds);
                SaveStoreMappings(campaign, model.SelectedStoreIds);

                NotifySuccess(T("Admin.Promotions.Campaigns.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            PrepareCampaignModel(model, null);

            return View(model);
        }

        [Permission(Permissions.Promotion.Campaign.Read)]
        public ActionResult Edit(int id)
        {
            var campaign = _campaignService.GetCampaignById(id);
            if (campaign == null)
                return RedirectToAction("List");

            var model = campaign.ToModel();
            PrepareCampaignModel(model, campaign);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
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

                SaveAclMappings(campaign, model.SelectedCustomerRoleIds);
                SaveStoreMappings(campaign, model.SelectedStoreIds);

                NotifySuccess(T("Admin.Promotions.Campaigns.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = campaign.Id }) : RedirectToAction("List");
            }

            PrepareCampaignModel(model, campaign);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("send-campaign")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.System.Message.Send)]
        public ActionResult SendCampaign(CampaignModel model)
        {
            var campaign = _campaignService.GetCampaignById(model.Id);
            if (campaign == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                var numberOfQueuedMessages = _campaignService.SendCampaign(campaign);

                NotifySuccess(string.Format(T("Admin.Promotions.Campaigns.MassEmailSentToCustomers"), numberOfQueuedMessages));
            }
            catch (Exception ex)
            {
                NotifyError(ex, false);
            }

            return RedirectToAction("Edit");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Promotion.Campaign.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var campaign = _campaignService.GetCampaignById(id);
            if (campaign != null)
            {
                _campaignService.DeleteCampaign(campaign);

                NotifySuccess(T("Admin.Promotions.Campaigns.Deleted"));
            }

            return RedirectToAction("List");
        }
    }
}
