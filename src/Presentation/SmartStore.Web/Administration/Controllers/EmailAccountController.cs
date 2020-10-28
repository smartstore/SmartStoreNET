using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Email;
using SmartStore.Core.Security;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class EmailAccountController : AdminControllerBase
    {
        #region Fields

        private readonly IEmailAccountService _emailAccountService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IEmailSender _emailSender;
        private readonly EmailAccountSettings _emailAccountSettings;

        #endregion

        #region Constructor 

        public EmailAccountController(
            IEmailAccountService emailAccountService,
            ILocalizationService localizationService,
            ISettingService settingService,
            IEmailSender emailSender,
            IStoreContext storeContext,
            EmailAccountSettings emailAccountSettings)
        {
            _emailAccountService = emailAccountService;
            _localizationService = localizationService;
            _emailAccountSettings = emailAccountSettings;
            _emailSender = emailSender;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region List / Create / Edit / Delete

        [Permission(Permissions.Configuration.EmailAccount.Read)]
        public ActionResult List(string id)
        {
            // Mark as default email account (if selected).
            if (id.HasValue())
            {
                int defaultEmailAccountId = Convert.ToInt32(id);
                var defaultEmailAccount = _emailAccountService.GetEmailAccountById(defaultEmailAccountId);
                if (defaultEmailAccount != null && Services.Permissions.Authorize(Permissions.Configuration.EmailAccount.Update))
                {
                    _emailAccountSettings.DefaultEmailAccountId = defaultEmailAccountId;
                    _settingService.SaveSetting(_emailAccountSettings);
                }
            }

            var emailAccountModels = _emailAccountService.GetAllEmailAccounts()
                .Select(x => x.ToModel())
                .ToList();

            foreach (var eam in emailAccountModels)
            {
                eam.IsDefaultEmailAccount = eam.Id == _emailAccountSettings.DefaultEmailAccountId;
            }

            var gridModel = new GridModel<EmailAccountModel>
            {
                Data = emailAccountModels,
                Total = emailAccountModels.Count()
            };

            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.EmailAccount.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<EmailAccountModel>();

            var emailAccountModels = _emailAccountService.GetAllEmailAccounts()
                .Select(x => x.ToModel())
                .ToList();

            foreach (var eam in emailAccountModels)
            {
                eam.IsDefaultEmailAccount = eam.Id == _emailAccountSettings.DefaultEmailAccountId;
            }

            model.Data = emailAccountModels;
            model.Total = emailAccountModels.Count();

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Configuration.EmailAccount.Create)]
        public ActionResult Create()
        {
            var model = new EmailAccountModel();
            //default values
            model.Port = 25;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.EmailAccount.Create)]
        public ActionResult Create(EmailAccountModel model, bool continueEditing)
        {
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

        [Permission(Permissions.Configuration.EmailAccount.Read)]
        public ActionResult Edit(int id)
        {
            var emailAccount = _emailAccountService.GetEmailAccountById(id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");

            return View(emailAccount.ToModel());
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.EmailAccount.Update)]
        public ActionResult Edit(EmailAccountModel model, bool continueEditing)
        {
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.EmailAccount.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var emailAccount = _emailAccountService.GetEmailAccountById(id);
            if (emailAccount == null)
                //No email account found with the specified id
                return RedirectToAction("List");

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.EmailAccounts.Deleted"));
            _emailAccountService.DeleteEmailAccount(emailAccount);
            return RedirectToAction("List");
        }

        #endregion

        #region Test email

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("sendtestemail")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.EmailAccount.Update)]
        public ActionResult SendTestEmail(EmailAccountModel model)
        {
            var emailAccount = _emailAccountService.GetEmailAccountById(model.Id);
            if (emailAccount == null)
            {
                return RedirectToAction("List");
            }

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
                    var body = T("Admin.Common.EmailSuccessfullySent");

                    // Avoid System.ArgumentException: "The specified string is not in the form required for a subject" when testing mails.
                    var subject = string.Concat(_storeContext.CurrentStore.Name, ". ", T("Admin.Configuration.EmailAccounts.TestingEmail"))
                        .RegexReplace(@"\p{C}+", " ")
                        .TrimSafe();

                    var msg = new EmailMessage(to, subject, body, from);

                    _emailSender.SendEmail(new SmtpContext(emailAccount), msg);

                    NotifySuccess(T("Admin.Configuration.EmailAccounts.SendTestEmail.Success"), false);
                }
            }
            catch (Exception ex)
            {
                model.TestEmailShortErrorMessage = ex.ToAllMessages();
                model.TestEmailFullErrorMessage = ex.ToString();
            }

            return View(model);
        }

        #endregion
    }
}
