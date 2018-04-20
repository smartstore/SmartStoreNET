using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core.Domain.Messages;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Utilities;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class QueuedEmailController : AdminControllerBase
	{
		private readonly IQueuedEmailService _queuedEmailService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

		public QueuedEmailController(
			IQueuedEmailService queuedEmailService,
            IDateTimeHelper dateTimeHelper, ILocalizationService localizationService,
            IPermissionService permissionService)
		{
            this._queuedEmailService = queuedEmailService;
            this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
		}

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

            var model = new QueuedEmailListModel();
            return View(model);
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult QueuedEmailList(GridCommand command, QueuedEmailListModel model)
        {
			var gridModel = new GridModel<QueuedEmailModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
			{
				DateTime? startDateValue = (model.SearchStartDate == null) ? null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchStartDate.Value, _dateTimeHelper.CurrentTimeZone);
				DateTime? endDateValue = (model.SearchEndDate == null) ? null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.SearchEndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

				var q = new SearchEmailsQuery
				{
					EndTime = endDateValue,
					From = model.SearchFromEmail,
					MaxSendTries = model.SearchMaxSentTries,
					OrderByLatest = true,
					PageIndex = command.Page - 1,
					PageSize = command.PageSize,
					SendManually = model.SearchSendManually,
					StartTime = startDateValue,
					To = model.SearchToEmail,
					UnsentOnly = model.SearchLoadNotSent
				};
				var queuedEmails = _queuedEmailService.SearchEmails(q);

				gridModel.Data = queuedEmails.Select(x =>
				{
					var m = x.ToModel();
					m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);

					if (x.SentOnUtc.HasValue)
						m.SentOn = _dateTimeHelper.ConvertToUserTime(x.SentOnUtc.Value, DateTimeKind.Utc);

					return m;
				});

				gridModel.Total = queuedEmails.TotalCount;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<QueuedEmailModel>();

				NotifyAccessDenied();
			}

			return new JsonResult
			{
				Data = gridModel
			};
		}

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-email-by-number")]
        public ActionResult GoToEmailByNumber(QueuedEmailListModel model)
        {
            var queuedEmail = _queuedEmailService.GetQueuedEmailById(model.GoDirectlyToNumber ?? 0);
			if (queuedEmail != null)
			{
				return RedirectToAction("Edit", "QueuedEmail", new { id = queuedEmail.Id });
			}

			return List();
        }

		public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

			var email = _queuedEmailService.GetQueuedEmailById(id);
            if (email == null)
                return RedirectToAction("List");

            var model = email.ToModel();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(email.CreatedOnUtc, DateTimeKind.Utc);
            if (email.SentOnUtc.HasValue)
                model.SentOn = _dateTimeHelper.ConvertToUserTime(email.SentOnUtc.Value, DateTimeKind.Utc);

            return View(model);
		}

        [HttpPost, ActionName("Edit")]
        [FormValueExists("save", "save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(QueuedEmailModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

            var email = _queuedEmailService.GetQueuedEmailById(model.Id);
            if (email == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                email = model.ToEntity(email);
                _queuedEmailService.UpdateQueuedEmail(email);

                NotifySuccess(_localizationService.GetResource("Admin.System.QueuedEmails.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = email.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(email.CreatedOnUtc, DateTimeKind.Utc);
            if (email.SentOnUtc.HasValue)
                model.SentOn = _dateTimeHelper.ConvertToUserTime(email.SentOnUtc.Value, DateTimeKind.Utc);

            return View(model);
		}

        [HttpPost, ActionName("Edit"), FormValueRequired("requeue")]
        public ActionResult Requeue(QueuedEmailModel queuedEmailModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

            var queuedEmail = _queuedEmailService.GetQueuedEmailById(queuedEmailModel.Id);
            if (queuedEmail == null)
                return RedirectToAction("List");

            var requeuedEmail = new QueuedEmail
            {
                Priority = queuedEmail.Priority,
                From = queuedEmail.From,
                To = queuedEmail.To,
                CC = queuedEmail.CC,
                Bcc = queuedEmail.Bcc,
                Subject = queuedEmail.Subject,
                Body = queuedEmail.Body,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = queuedEmail.EmailAccountId,
				SendManually = queuedEmail.SendManually
            };
            _queuedEmailService.InsertQueuedEmail(requeuedEmail);

            NotifySuccess(_localizationService.GetResource("Admin.System.QueuedEmails.Requeued"));

            return RedirectToAction("Edit", requeuedEmail.Id);
        }

		[HttpPost, ActionName("Edit"), FormValueRequired("sendnow")]
		public ActionResult SendNow(QueuedEmailModel queuedEmailModel)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
				return AccessDeniedView();

			var queuedEmail = _queuedEmailService.GetQueuedEmailById(queuedEmailModel.Id);
			if (queuedEmail == null)
				return RedirectToAction("List");

			var result = _queuedEmailService.SendEmail(queuedEmail);

			if (result)
				NotifySuccess(_localizationService.GetResource("Admin.Common.TaskSuccessfullyProcessed"));
			else
				NotifyError(_localizationService.GetResource("Common.Error.SendMail"));

			return RedirectToAction("Edit", queuedEmail.Id);
		}

	    [HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

			var email = _queuedEmailService.GetQueuedEmailById(id);
            if (email == null)
                return RedirectToAction("List");

            _queuedEmailService.DeleteQueuedEmail(email);

            NotifySuccess(_localizationService.GetResource("Admin.System.QueuedEmails.Deleted"));

			return RedirectToAction("List");
		}

        [HttpPost]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
                return AccessDeniedView();

            if (selectedIds != null)
            {
                var queuedEmails = _queuedEmailService.GetQueuedEmailsByIds(selectedIds.ToArray());

				foreach (var queuedEmail in queuedEmails)
				{
					_queuedEmailService.DeleteQueuedEmail(queuedEmail);
				}
            }

            return Json(new { Result = true });
        }

		[HttpPost, ActionName("List"), FormValueRequired("delete-all")]
		public ActionResult DeleteAll()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
				return AccessDeniedView();

			int count = _queuedEmailService.DeleteAllQueuedEmails();

			NotifySuccess(T("Admin.Common.RecordsDeleted", count));

			return RedirectToAction("List");
		}

		public ActionResult DownloadAttachment(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageMessageQueue))
				return AccessDeniedView();

			var qea = _queuedEmailService.GetQueuedEmailAttachmentById(id);
			if (qea == null)
			{
				return HttpNotFound("List");
			}

			if (qea.StorageLocation == EmailAttachmentStorageLocation.Blob)
			{
				var data = _queuedEmailService.LoadQueuedEmailAttachmentBinary(qea);
				if (data != null)
				{
					return File(data, qea.MimeType, qea.Name);
				}
			}
			else if (qea.StorageLocation == EmailAttachmentStorageLocation.Path)
			{
				var path = qea.Path;
				if (path[0] == '~' || path[0] == '/')
				{
					path = CommonHelper.MapPath(VirtualPathUtility.ToAppRelative(path), false);
				}

				if (!System.IO.File.Exists(path))
				{
					NotifyError(string.Concat(T("Admin.Common.FileNotFound"), ": ", path));

					var referrer = Services.WebHelper.GetUrlReferrer();
					if (referrer.HasValue())
						return Redirect(referrer);
					
					return RedirectToAction("List");
				}

				return File(path, qea.MimeType, qea.Name);
			}
			else if (qea.FileId.HasValue)
			{
				return RedirectToAction("DownloadFile", "Download", new { downloadId = qea.FileId.Value });
			}

			NotifyError(T("Admin.System.QueuedEmails.CouldNotDownloadAttachment"));
			return RedirectToAction("List");
		}
	}
}
