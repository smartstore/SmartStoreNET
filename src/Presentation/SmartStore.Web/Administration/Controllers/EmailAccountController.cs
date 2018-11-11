using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class EmailAccountController : AdminControllerBase
	{
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
		private readonly IStoreContext _storeContext;
        private readonly IEmailSender _emailSender;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly IPermissionService _permissionService;

		public EmailAccountController(IEmailAccountService emailAccountService,
            ILocalizationService localizationService, ISettingService settingService,
			IEmailSender emailSender, IStoreContext storeContext,
            EmailAccountSettings emailAccountSettings, IPermissionService permissionService)
		{
            this._emailAccountService = emailAccountService;
            this._localizationService = localizationService;
            this._emailAccountSettings = emailAccountSettings;
            this._emailSender = emailSender;
            this._settingService = settingService;
			this._storeContext = storeContext;
            this._permissionService = permissionService;
		}

		public ActionResult List(string id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

			//mark as default email account (if selected)
			if (!String.IsNullOrEmpty(id))
			{
				int defaultEmailAccountId = Convert.ToInt32(id);
				var defaultEmailAccount = _emailAccountService.GetEmailAccountById(defaultEmailAccountId);
				if (defaultEmailAccount != null)
				{
					_emailAccountSettings.DefaultEmailAccountId = defaultEmailAccountId;
					_settingService.SaveSetting(_emailAccountSettings);
				}
			}

			var emailAccountModels = _emailAccountService.GetAllEmailAccounts()
									.Select(x => x.ToModel())
									.ToList();
			foreach (var eam in emailAccountModels)
				eam.IsDefaultEmailAccount = eam.Id == _emailAccountSettings.DefaultEmailAccountId;

			var gridModel = new GridModel<EmailAccountModel>
			{
				Data = emailAccountModels,
				Total = emailAccountModels.Count()
			};
			return View(gridModel);
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
        {
			var model = new GridModel<EmailAccountModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
			{
				var emailAccountModels = _emailAccountService.GetAllEmailAccounts()
					.Select(x => x.ToModel())
					.ToList();

				foreach (var eam in emailAccountModels)
				{
					eam.IsDefaultEmailAccount = eam.Id == _emailAccountSettings.DefaultEmailAccountId;
				}

				model.Data = emailAccountModels;
				model.Total = emailAccountModels.Count();
			}
			else
			{
				model.Data = Enumerable.Empty<EmailAccountModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
			{
				Data = model
			};
		}

		public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            var model = new EmailAccountModel();
            //default values
            model.Port = 25;
			return View(model);
		}
        
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		public ActionResult Create(EmailAccountModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var emailAccount = model.ToEntity();
                _emailAccountService.InsertEmailAccount(emailAccount);

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = emailAccount.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
		}

		public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

			var emailAccount = _emailAccountService.GetEmailAccountById(id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");

			return View(emailAccount.ToModel());
		}
        
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(EmailAccountModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            var emailAccount = _emailAccountService.GetEmailAccountById(model.Id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                emailAccount = model.ToEntity(emailAccount);
                _emailAccountService.UpdateEmailAccount(emailAccount);

                NotifySuccess(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = emailAccount.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
		}

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("sendtestemail")]
        public ActionResult SendTestEmail(EmailAccountModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            var emailAccount = _emailAccountService.GetEmailAccountById(model.Id);
            if (emailAccount == null)
                return RedirectToAction("List");

            try
            {
				if (model.SendTestEmailTo.IsEmpty())
				{
					NotifyError(T("Admin.Common.EnterEmailAdress"));
				}
				else
				{
					var to = new EmailAddress(model.SendTestEmailTo);
					var from = new EmailAddress(emailAccount.Email, emailAccount.DisplayName);
					var subject = string.Concat(_storeContext.CurrentStore.Name, ". ", T("Admin.Configuration.EmailAccounts.TestingEmail"));
					var body = T("Admin.Common.EmailSuccessfullySent");

					var msg = new EmailMessage(to, subject, body, from);

					_emailSender.SendEmail(new SmtpContext(emailAccount), msg);

					NotifySuccess(T("Admin.Configuration.EmailAccounts.SendTestEmail.Success"), false);
				}
            }
            catch (Exception exception)
            {
				model.TestEmailShortErrorMessage = exception.ToAllMessages();
				model.TestEmailFullErrorMessage = exception.ToString();
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageEmailAccounts))
                return AccessDeniedView();

            var emailAccount = _emailAccountService.GetEmailAccountById(id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Deleted"));
            _emailAccountService.DeleteEmailAccount(emailAccount);
            return RedirectToAction("List");
        }
	}
}
