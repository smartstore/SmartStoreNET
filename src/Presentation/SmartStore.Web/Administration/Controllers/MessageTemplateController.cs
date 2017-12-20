using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Collections;
using SmartStore.Core.Domain.Messages;
using SmartStore.Data.Utilities;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
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
    public partial class MessageTemplateController : AdminControllerBase
    {
        private readonly IMessageTemplateService _messageTemplateService;
		private readonly IMessageFactory _messageFactory;
		private readonly IEmailAccountService _emailAccountService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
		private readonly IStoreService _storeService;
		private readonly IStoreMappingService _storeMappingService;
        private readonly EmailAccountSettings _emailAccountSettings;

        public MessageTemplateController(
			IMessageTemplateService messageTemplateService,
			IMessageFactory messageFactory,
			IEmailAccountService emailAccountService, 
			ILanguageService languageService, 
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService, 
			IPermissionService permissionService, 
			IStoreService storeService,
			IStoreMappingService storeMappingService,
			EmailAccountSettings emailAccountSettings)
        {
            _messageTemplateService = messageTemplateService;
			_messageFactory = messageFactory;
            _emailAccountService = emailAccountService;
            _languageService = languageService;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _permissionService = permissionService;
			_storeService = storeService;
			_storeMappingService = storeMappingService;
            _emailAccountSettings = emailAccountSettings;
        }

        [NonAction]
        public void UpdateLocales(MessageTemplate mt, MessageTemplateModel model)
        {
            foreach (var localized in model.Locales)
            {
				int lid = localized.LanguageId;

				MediaHelper.UpdateDownloadTransientState(mt.GetLocalized(x => x.Attachment1FileId, lid, false, false), localized.Attachment1FileId, true);
				MediaHelper.UpdateDownloadTransientState(mt.GetLocalized(x => x.Attachment2FileId, lid, false, false), localized.Attachment2FileId, true);
				MediaHelper.UpdateDownloadTransientState(mt.GetLocalized(x => x.Attachment3FileId, lid, false, false), localized.Attachment3FileId, true);

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
			if (model == null)
				throw new ArgumentNullException("model");

			model.AvailableStores = _storeService
				.GetAllStores()
				.Select(s => s.ToModel())
				.ToList();
			if (!excludeProperties)
			{
				if (messageTemplate != null)
				{
					model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(messageTemplate);
				}
				else
				{
					model.SelectedStoreIds = new int[0];
				}
			}
		}

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();
			
			var model = new MessageTemplateListModel();

			foreach (var s in _storeService.GetAllStores())
			{
				model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
			}

			return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command, MessageTemplateListModel model)
        {
			var gridModel = new GridModel<MessageTemplateModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
			{
				var messageTemplates = _messageTemplateService.GetAllMessageTemplates(model.SearchStoreId);

				gridModel.Data = messageTemplates.Select(x => x.ToModel());
				gridModel.Total = messageTemplates.Count;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<MessageTemplateModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var messageTemplate = _messageTemplateService.GetMessageTemplateById(id);
            if (messageTemplate == null)
                return RedirectToAction("List");

			// TODO: (mc) Liquid > LastModelTree
            var model = messageTemplate.ToModel();
            //model.TokensTree = _messageTokenProvider.GetTreeOfAllowedTokens();
			DeserializeLastModelTree(messageTemplate);

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

		private void DeserializeLastModelTree(MessageTemplate template)
		{
			if (template.LastModelTree.HasValue())
			{
				ViewBag.LastModelTree = Newtonsoft.Json.JsonConvert.DeserializeObject<TreeNode<ModelTreeMember>>(template.LastModelTree);
			}
		}

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		[FormValueRequired("save", "save-continue")]
        public ActionResult Edit(MessageTemplateModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
                return AccessDeniedView();

            var messageTemplate = _messageTemplateService.GetMessageTemplateById(model.Id);
            if (messageTemplate == null)
                return RedirectToAction("List");
            
            if (ModelState.IsValid)
            {
                messageTemplate = model.ToEntity(messageTemplate);

				MediaHelper.UpdateDownloadTransientStateFor(messageTemplate, x => x.Attachment1FileId);
				MediaHelper.UpdateDownloadTransientStateFor(messageTemplate, x => x.Attachment2FileId);
				MediaHelper.UpdateDownloadTransientStateFor(messageTemplate, x => x.Attachment3FileId);

                _messageTemplateService.UpdateMessageTemplate(messageTemplate);
				
				// Stores
				_storeMappingService.SaveStoreMappings<MessageTemplate>(messageTemplate, model.SelectedStoreIds);
                
				// locales
                UpdateLocales(messageTemplate, model);

                NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Updated"));
                return continueEditing ? RedirectToAction("Edit", messageTemplate.Id) : RedirectToAction("List");
            }

			// TODO: (mc) Liquid > LastModelTree
			// If we got this far, something failed, redisplay form
			//model.TokensTree = _messageTokenProvider.GetTreeOfAllowedTokens();
			DeserializeLastModelTree(messageTemplate);

			// Available email accounts
			foreach (var ea in _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
			
			// Store
			PrepareStoresMappingModel(model, messageTemplate, true);
            return View(model);
        }

		[HttpPost]
		public ActionResult Delete(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
				return AccessDeniedView();

			var messageTemplate = _messageTemplateService.GetMessageTemplateById(id);
			if (messageTemplate == null)
				return RedirectToAction("List");

			_messageTemplateService.DeleteMessageTemplate(messageTemplate);

			NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.MessageTemplates.Deleted"));
			return RedirectToAction("List");
		}

		[HttpPost, ActionName("Edit")]
		[FormValueRequired("preview")]
		public ActionResult Preview(int id)
		{
			// TODO: (mc) Liquid > Display info about preview models
			var template = _messageTemplateService.GetMessageTemplateById(id);
			if (template == null)
			{
				return RedirectToAction("List");
			}			

			try
			{
				var context = new MessageContext
				{
					MessageTemplate = template,
					TestMode = true
				};

				// TODO: (mc) Liquid > make proper UI for testing (IFrame, Recipient etc.)
				var result = _messageFactory.CreateMessage(context, false);
				var messageModel = result.Model;

				return Content(result.Email.Body, "text/html");
			}
			catch (Exception ex)
			{
				NotifyError(ex);
				return RedirectToAction("Edit", template.Id);
			}
		}

		[HttpPost, ActionName("Edit")]
		[FormValueRequired("save-in-file")]
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
		public ActionResult CopyTemplate(MessageTemplateModel model)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageTemplates))
				return AccessDeniedView();

			var messageTemplate = _messageTemplateService.GetMessageTemplateById(model.Id);
			if (messageTemplate == null)
				return RedirectToAction("List");

			try
			{
				var newMessageTemplate = _messageTemplateService.CopyMessageTemplate(messageTemplate);
				NotifySuccess("The message template has been copied successfully");
				return RedirectToAction("Edit", new { id = newMessageTemplate.Id });
			}
			catch (Exception exc)
			{
				NotifyError(exc.Message);
				return RedirectToAction("Edit", new { id = model.Id });
			}
		}
    }
}
