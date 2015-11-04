using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.UrlRecord;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	// TODO: add new permisssion record "ManageUrlRecords"
	[AdminAuthorize]
	public class UrlRecordController : AdminControllerBase
	{
		private readonly IUrlRecordService _urlRecordService;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly ICommonServices _services;
		private readonly ILanguageService _languageService;

		public UrlRecordController(
			IUrlRecordService urlRecordService,
			AdminAreaSettings adminAreaSettings,
			ICommonServices services,
			ILanguageService languageService)
		{
			_urlRecordService = urlRecordService;
			_adminAreaSettings = adminAreaSettings;
			_services = services;
			_languageService = languageService;
		}

		private void PrepareUrlRecordModel(UrlRecordModel model, UrlRecord urlRecord)
		{
			var allLanguages = _languageService.GetAllLanguages(true);

			model.AvailableLanguages = allLanguages
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
				.ToList();

			model.AvailableLanguages.Insert(0, new SelectListItem { Text = T("Admin.System.SeNames.Language.Standard"), Value = "0" });

			if (urlRecord != null)
			{
				model.Id = urlRecord.Id;
				model.Slug = urlRecord.Slug;
				model.EntityName = urlRecord.EntityName;
				model.EntityId = urlRecord.EntityId;
				model.IsActive = urlRecord.IsActive;
				model.LanguageId = urlRecord.LanguageId;

				try
				{
					var slugCount = _urlRecordService.CountSlugsPerEntity(new int[] { urlRecord.Id });

					model.SlugsPerEntity = (slugCount.ContainsKey(urlRecord.Id) ? slugCount[urlRecord.Id] : 0);
				}
				catch (Exception exc)
				{
					NotifyError(exc);
				}
			}
		}

		public ActionResult List(string entityName, int? entityId)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();

			var model = new UrlRecordListModel
			{
				GridPageSize = _adminAreaSettings.GridPageSize,
				EntityName = entityName,
				EntityId = entityId
			};

			var allLanguages = _languageService.GetAllLanguages(true);

			model.AvailableLanguages = allLanguages
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
				.ToList();

			model.AvailableLanguages.Insert(0, new SelectListItem { Text = T("Admin.System.SeNames.Language.Standard"), Value = "0" });

			return View(model);
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command, UrlRecordListModel model)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();

			var allLanguages = _languageService.GetAllLanguages(true);
			var defaultLanguageName = T("Admin.System.SeNames.Language.Standard");

			var urlRecords = _urlRecordService.GetAllUrlRecords(command.Page - 1, command.PageSize,
				model.SeName, model.EntityName, model.EntityId, model.LanguageId, model.IsActive);

			var slugsPerEntity = _urlRecordService.CountSlugsPerEntity(urlRecords.Select(x => x.Id).Distinct().ToArray());

			var gridModel = new GridModel<UrlRecordModel>
			{
				Data = urlRecords.Select(x =>
				{
					string languageName;

					if (x.LanguageId == 0)
					{
						languageName = defaultLanguageName;
					}
					else
					{
						var language = allLanguages.FirstOrDefault(y => y.Id == x.LanguageId);
						languageName = (language != null ? language.Name : "".NaIfEmpty());
					}

					var urlRecordModel = new UrlRecordModel
					{
						Id = x.Id,
						Slug = x.Slug,
						EntityName = x.EntityName,
						EntityId = x.EntityId,
						IsActive = x.IsActive,
						Language = languageName,
						SlugsPerEntity = (slugsPerEntity.ContainsKey(x.Id) ? slugsPerEntity[x.Id] : 0)
					};

					return urlRecordModel;
				}),
				Total = urlRecords.TotalCount
			};

			return new JsonResult
			{
				Data = gridModel
			};
		}

		public ActionResult Edit(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();

			var urlRecord = _urlRecordService.GetUrlRecordById(id);
			if (urlRecord == null)
				return RedirectToAction("List");

			var model = new UrlRecordModel();
			PrepareUrlRecordModel(model, urlRecord);

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		public ActionResult Edit(UrlRecordModel model, bool continueEditing)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();

			var urlRecord = _urlRecordService.GetUrlRecordById(model.Id);
			if (urlRecord == null)
				return RedirectToAction("List");

			if (!urlRecord.IsActive && model.IsActive)
			{
				var urlRecords = _urlRecordService.GetAllUrlRecords(0, int.MaxValue, null, model.EntityName, model.EntityId, model.LanguageId, true);
				if (urlRecords.Count > 0)
				{
					ModelState.AddModelError("", T("Admin.System.SeNames.ActiveSlugAlreadyExist"));
				}
			}

			if (ModelState.IsValid)
			{
				urlRecord.Slug = model.Slug;
				urlRecord.EntityId = model.EntityId;
				urlRecord.EntityName = model.EntityName;
				urlRecord.IsActive = model.IsActive;
				urlRecord.LanguageId = model.LanguageId;

				_urlRecordService.UpdateUrlRecord(urlRecord);

				NotifySuccess(T("Admin.Common.DataEditSuccess"));

				return continueEditing ? RedirectToAction("Edit", new { id = urlRecord.Id }) : RedirectToAction("List");
			}

			PrepareUrlRecordModel(model, null);

			return View(model);
		}

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();

			var urlRecord = _urlRecordService.GetUrlRecordById(id);
			if (urlRecord == null)
				return RedirectToAction("List");

			try
			{
				_urlRecordService.DeleteUrlRecord(urlRecord);

				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

				return RedirectToAction("List");
			}
			catch (Exception exc)
			{
				NotifyError(exc);
				return RedirectToAction("Edit", new { id = urlRecord.Id });
			}
		}

		[HttpPost]
		public ActionResult DeleteSelected(ICollection<int> selectedIds)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageMaintenance))
				return AccessDeniedView();

			if (selectedIds != null)
			{
				var urlRecords = _urlRecordService.GetUrlRecordsByIds(selectedIds.ToArray());

				foreach (var urlRecord in urlRecords)
				{
					_urlRecordService.DeleteUrlRecord(urlRecord);
				}
			}

			return Json(new { Result = true });
		}
	}
}