using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Collections;
using SmartStore.Core.Domain.Messages;
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
    public class MessageTemplateController : AdminControllerBase
    {
        #region Fields

        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILanguageService _languageService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly IMessageTokenProvider _messageTokenProvider;
        private readonly IPermissionService _permissionService;
		private readonly IStoreService _storeService;
		private readonly IStoreMappingService _storeMappingService;
        private readonly EmailAccountSettings _emailAccountSettings;
        #endregion Fields

        #region Constructors

        public MessageTemplateController(IMessageTemplateService messageTemplateService, 
            IEmailAccountService emailAccountService, ILanguageService languageService, 
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService, IMessageTokenProvider messageTokenProvider,
			IPermissionService permissionService, IStoreService storeService,
			IStoreMappingService storeMappingService,
			EmailAccountSettings emailAccountSettings)
        {
            this._messageTemplateService = messageTemplateService;
            this._emailAccountService = emailAccountService;
            this._languageService = languageService;
            this._localizedEntityService = localizedEntityService;
            this._localizationService = localizationService;
            this._messageTokenProvider = messageTokenProvider;
            this._permissionService = permissionService;
			this._storeService = storeService;
			this._storeMappingService = storeMappingService;
            this._emailAccountSettings = emailAccountSettings;
        }

        #endregion
        
        #region Utilities

        [NonAction]
        public void UpdateLocales(MessageTemplate mt, MessageTemplateModel model)
        {
            foreach (var localized in model.Locales)
            {
				int lid = localized.LanguageId;

				MediaHelper.UpdateDownloadTransientState(mt.GetLocalized(x => x.Attachment1FileId, lid, false, false), localized.Attachment1FileId, true);
				MediaHelper.UpdateDownloadTransientState(mt.GetLocalized(x => x.Attachment2FileId, lid, false, false), localized.Attachment2FileId, true);
				MediaHelper.UpdateDownloadTransientState(mt.GetLocalized(x => x.Attachment3FileId, lid, false, false), localized.Attachment3FileId, true);

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
        
        #endregion
        
        #region Methods

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

            var model = messageTemplate.ToModel();
            model.TokensTree = _messageTokenProvider.GetTreeOfAllowedTokens();

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
            
            //If we got this far, something failed, redisplay form
            model.TokensTree = _messageTokenProvider.GetTreeOfAllowedTokens();

            //available email accounts
            foreach (var ea in _emailAccountService.GetAllEmailAccounts())
                model.AvailableEmailAccounts.Add(ea.ToModel());
			
			//Store
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

        #endregion
    }
}
