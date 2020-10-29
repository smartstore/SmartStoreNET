using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Caching;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;
using SmartStore.Core.Security;
using SmartStore.Data.Utilities;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class MessageTemplateController : AdminControllerBase
    {
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly ICampaignService _campaignService;
        private readonly IMessageFactory _messageFactory;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEmailSender _emailSender;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IMediaTracker _mediaTracker;

        public MessageTemplateController(
            IMessageTemplateService messageTemplateService,
            ICampaignService campaignService,
            IMessageFactory messageFactory,
            IEmailAccountService emailAccountService,
            IEmailSender emailSender,
            ILanguageService languageService,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            EmailAccountSettings emailAccountSettings,
            IMediaTracker mediaTracker)
        {
            _messageTemplateService = messageTemplateService;
            _campaignService = campaignService;
            _messageFactory = messageFactory;
            _emailAccountService = emailAccountService;
            _emailSender = emailSender;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _emailAccountSettings = emailAccountSettings;
            _mediaTracker = mediaTracker;
        }

        #region Utilities

        [NonAction]
        public void UpdateLocales(MessageTemplate mt, MessageTemplateModel model)
        {
            foreach (var localized in model.Locales)
            {
                int lid = localized.LanguageId;

                // Attachments: handle tracking of localized media file uploads
                var attachments = new List<(int? prevId, int? curId, string prop)>(3)
                {
                    (mt.GetLocalized(x => x.Attachment1FileId, lid, false, false), localized.Attachment1FileId, $"Attachment1FileId[{lid}]"),
                    (mt.GetLocalized(x => x.Attachment2FileId, lid, false, false), localized.Attachment2FileId, $"Attachment2FileId[{lid}]"),
                    (mt.GetLocalized(x => x.Attachment3FileId, lid, false, false), localized.Attachment3FileId, $"Attachment3FileId[{lid}]")
                };

                foreach (var attach in attachments)
                {
                    if (attach.prevId != attach.curId)
                    {
                        if (attach.prevId.HasValue)
                        {
                            _mediaTracker.Untrack(mt, attach.prevId.Value, attach.prop);
                        }
                        if (attach.curId.HasValue)
                        {
                            _mediaTracker.Track(mt, attach.curId.Value, attach.prop);
                        }
                    }
                }

                _localizedEntityService.SaveLocalizedValue(mt, x => x.To, localized.To, lid);
                _localizedEntityService.SaveLocalizedValue(mt, x => x.ReplyTo, localized.ReplyTo, lid);
                _localizedEntityService.SaveLocalizedValue(mt, x => x.BccEmailAddresses, localized.BccEmailAddresses, lid);
                _localizedEntityService.SaveLocalizedValue(mt, x => x.Subject, localized.Subject, lid);
                _localizedEntityService.SaveLocalizedValue(mt, x => x.Body, localized.Body, lid);
                _localizedEntityService.SaveLocalizedValue(mt, x => x.EmailAccountId, localized.EmailAccountId, lid);
                _localizedEntityService.SaveLocalizedValue(mt, x => x.Attachment1FileId, localized.Attachment1FileId, lid);
                _localizedEntityService.SaveLocalizedValue(mt, x => x.Attachment2FileId, localized.Attachment2FileId, lid);
                _localizedEntityService.SaveLocalizedValue(mt, x => x.Attachment3FileId, localized.Attachment3FileId, lid);
            }
        }

        [NonAction]
        private void PrepareStoresMappingModel(MessageTemplateModel model, MessageTemplate messageTemplate, bool excludeProperties)
        {
            Guard.NotNull(model, nameof(model));

            if (!excludeProperties)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(messageTemplate);
            }
        }

        private void PrepareLastModelTree(MessageTemplate template)
        {
            ViewBag.LastModelTreeJson = template.LastModelTree;
            ViewBag.LastModelTree = Services.Resolve<IMessageModelProvider>().GetLastModelTree(template);
        }

        #endregion

        #region List / Edit / Delete

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Cms.MessageTemplate.Read)]
        public ActionResult List()
        {
            var model = new MessageTemplateListModel();

            foreach (var s in _storeService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.MessageTemplate.Read)]
        public ActionResult List(GridCommand command, MessageTemplateListModel model)
        {
            var gridModel = new GridModel<MessageTemplateModel>();

            var messageTemplates = _messageTemplateService.GetAllMessageTemplates(model.SearchStoreId);

            gridModel.Data = messageTemplates.Select(x => x.ToModel());
            gridModel.Total = messageTemplates.Count;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Cms.MessageTemplate.Read)]
        public ActionResult Edit(int id)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(id);
            if (messageTemplate == null)
                return RedirectToAction("List");

            var model = messageTemplate.ToModel();
            PrepareLastModelTree(messageTemplate);

            // available email accounts
            foreach (var ea in _emailAccountService.GetAllEmailAccounts())
            {
                model.AvailableEmailAccounts.Add(ea.ToModel());
            }

            // Store
            PrepareStoresMappingModel(model, messageTemplate, false);

            // locales
            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.To = messageTemplate.GetLocalized(x => x.To, languageId, false, false);
                locale.ReplyTo = messageTemplate.GetLocalized(x => x.ReplyTo, languageId, false, false);
                locale.BccEmailAddresses = messageTemplate.GetLocalized(x => x.BccEmailAddresses, languageId, false, false);
                locale.Subject = messageTemplate.GetLocalized(x => x.Subject, languageId, false, false);
                locale.Body = messageTemplate.GetLocalized(x => x.Body, languageId, false, false);
                locale.Attachment1FileId = messageTemplate.GetLocalized(x => x.Attachment1FileId, languageId, false, false);
                locale.Attachment2FileId = messageTemplate.GetLocalized(x => x.Attachment2FileId, languageId, false, false);
                locale.Attachment3FileId = messageTemplate.GetLocalized(x => x.Attachment3FileId, languageId, false, false);

                var emailAccountId = messageTemplate.GetLocalized(x => x.EmailAccountId, languageId, false, false);
                locale.EmailAccountId = emailAccountId > 0 ? emailAccountId : _emailAccountSettings.DefaultEmailAccountId;
            });

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.MessageTemplate.Update)]
        public ActionResult Edit(MessageTemplateModel model, bool continueEditing, FormCollection form)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(model.Id);
            if (messageTemplate == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                messageTemplate = model.ToEntity(messageTemplate);

                _messageTemplateService.UpdateMessageTemplate(messageTemplate);

                SaveStoreMappings(messageTemplate, model.SelectedStoreIds);
                UpdateLocales(messageTemplate, model);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, messageTemplate, form));

                NotifySuccess(T("Admin.ContentManagement.MessageTemplates.Updated"));
                return continueEditing ? RedirectToAction("Edit", messageTemplate.Id) : RedirectToAction("List");
            }

            model.AvailableEmailAccounts = _emailAccountService.GetAllEmailAccounts()
                .Select(x => x.ToModel())
                .ToList();

            PrepareLastModelTree(messageTemplate);
            PrepareStoresMappingModel(model, messageTemplate, true);

            return View(model);
        }

        [HttpPost]
        [Permission(Permissions.Cms.MessageTemplate.Delete)]
        public ActionResult Delete(int id)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(id);
            if (messageTemplate == null)
                return RedirectToAction("List");

            _messageTemplateService.DeleteMessageTemplate(messageTemplate);

            NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Preview 

        [Permission(Permissions.Cms.MessageTemplate.Read)]
        public ActionResult Preview(int id, bool isCampaign = false)
        {
            var model = new MessageTemplatePreviewModel();

            // TODO: (mc) Liquid > Display info about preview models
            try
            {
                CreateMessageResult result = null;

                if (isCampaign)
                {
                    var campaign = _campaignService.GetCampaignById(id);
                    if (campaign == null)
                    {
                        model.Error = "The request campaign does not exist.";
                        return View(model);
                    }

                    result = _campaignService.Preview(campaign);
                }
                else
                {
                    var template = _messageTemplateService.GetMessageTemplateById(id);
                    if (template == null)
                    {
                        model.Error = "The requested message template does not exist.";
                        return View(model);
                    }

                    var messageContext = new MessageContext
                    {
                        MessageTemplate = template,
                        TestMode = true
                    };

                    result = _messageFactory.CreateMessage(messageContext, false);
                }

                var email = result.Email;

                model.AccountEmail = email.EmailAccount?.Email ?? result.MessageContext.EmailAccount?.Email;
                model.EmailAccountId = email.EmailAccountId;
                model.Bcc = email.Bcc;
                model.Body = email.Body;
                model.From = email.From;
                model.ReplyTo = email.ReplyTo;
                model.Subject = email.Subject;
                model.To = email.To;
                model.Error = null;
                model.Token = Guid.NewGuid().ToString();
                model.BodyUrl = Url.Action("PreviewBody", new { token = model.Token });

                HttpContext.Cache.Insert("mtpreview:" + model.Token, model, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(1));
            }
            catch (Exception ex)
            {
                model.Error = ex.ToAllMessages();
            }

            return View(model);
        }

        [Permission(Permissions.Cms.MessageTemplate.Read)]
        public ActionResult PreviewBody(string token)
        {
            var body = GetPreviewMailModel(token)?.Body;

            if (body.IsEmpty())
            {
                body = "<div style='padding:20px;font-family:sans-serif;color:red'>{0}</div>".FormatCurrent(T("Admin.MessageTemplate.Preview.NoBody"));
            }

            return Content(body, "text/html");
        }

        [HttpPost]
        public ActionResult PreservePreview(string token)
        {
            // While the preview window is open, the preview model should not expire.
            GetPreviewMailModel(token);
            return Content(token);
        }

        private MessageTemplatePreviewModel GetPreviewMailModel(string token)
        {
            return (MessageTemplatePreviewModel)HttpContext.Cache.Get("mtpreview:" + token);
        }

        [HttpPost]
        [Permission(Permissions.System.Message.Send)]
        public async Task<ActionResult> SendTestMail(string token, string to)
        {
            var model = GetPreviewMailModel(token);
            if (model == null)
            {
                return Json(new { success = false, message = "Preview result not available anymore. Try again." });
            }

            try
            {
                var account = _emailAccountService.GetEmailAccountById(model.EmailAccountId) ?? _emailAccountService.GetDefaultEmailAccount();
                var msg = new EmailMessage(to, model.Subject, model.Body, model.From);
                await _emailSender.SendEmailAsync(new SmtpContext(account), msg);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Templates

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("save-in-file")]
        [Permission(Permissions.Cms.MessageTemplate.Update)]
        public ActionResult SaveInFile(int id)
        {
            var template = _messageTemplateService.GetMessageTemplateById(id);
            if (template == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                var converter = new MessageTemplateConverter(Services.DbContext);
                converter.Save(template, Services.WorkContext.WorkingLanguage);
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return RedirectToAction("Edit", template.Id);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("message-template-copy")]
        [Permission(Permissions.Cms.MessageTemplate.Create)]
        public ActionResult CopyTemplate(MessageTemplateModel model)
        {
            var messageTemplate = _messageTemplateService.GetMessageTemplateById(model.Id);
            if (messageTemplate == null)
                return RedirectToAction("List");

            try
            {
                var newMessageTemplate = _messageTemplateService.CopyMessageTemplate(messageTemplate);
                NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.SuccessfullyCopied"));
                return RedirectToAction("Edit", new { id = newMessageTemplate.Id });
            }
            catch (Exception exc)
            {
                NotifyError(exc.Message);
                return RedirectToAction("Edit", new { id = model.Id });
            }
        }

        [Permission(Permissions.Cms.MessageTemplate.Create)]
        public ActionResult ImportAllTemplates()
        {
            // Hidden action for admins.
            var converter = new MessageTemplateConverter(Services.DbContext);
            converter.ImportAll(Services.WorkContext.WorkingLanguage);

            NotifySuccess("All file based message templates imported successfully.");

            return RedirectToAction("List");
        }

        #endregion
    }
}
