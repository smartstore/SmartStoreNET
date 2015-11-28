using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Stores;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Configuration;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public partial class StoreController : AdminControllerBase
	{
		private readonly IStoreService _storeService;
		private readonly ISettingService _settingService;
		private readonly ILocalizationService _localizationService;
		private readonly IPermissionService _permissionService;
		private readonly ICurrencyService _currencyService;

		public StoreController(IStoreService storeService,
			ISettingService settingService,
			ILocalizationService localizationService,
			IPermissionService permissionService,
			ICurrencyService currencyService)
		{
			this._storeService = storeService;
			this._settingService = settingService;
			this._localizationService = localizationService;
			this._permissionService = permissionService;
			this._currencyService = currencyService;
		}

		private void PrepareStoreModel(StoreModel model, Store store)
		{
			model.AvailableCurrencies = _currencyService.GetAllCurrencies(false, store == null ? 0 : store.Id)
				.Select(x => new SelectListItem
				{
					Text = x.Name,
					Value = x.Id.ToString()
				})
				.ToList();
		}

		public ActionResult List()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			return View();
		}

		public ActionResult AllStores(string label, int selectedId = 0)
		{
			var stores = _storeService.GetAllStores();

			if (label.HasValue())
			{
				stores.Insert(0, new Store { Name = label, Id = 0 });
			}

			var list = 
				from m in stores
				select new
				{
					id = m.Id.ToString(),
					text = m.Name,
					selected = m.Id == selectedId
				};

			return new JsonResult { Data = list.ToList(), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var storeModels = _storeService.GetAllStores()
				.Select(x => 
				{
					var model = x.ToModel();

					PrepareStoreModel(model, x);

					model.Hosts = model.Hosts.EmptyNull().Replace(",", "<br />");

					return model;
				})
				.ToList();

			var gridModel = new GridModel<StoreModel>
			{
				Data = storeModels,
				Total = storeModels.Count()
			};

			return new JsonResult
			{
				Data = gridModel
			};
		}

		public ActionResult Create()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var model = new StoreModel();
			PrepareStoreModel(model, null);

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		public ActionResult Create(StoreModel model, bool continueEditing)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			if (ModelState.IsValid)
			{
				var store = model.ToEntity();
				MediaHelper.UpdatePictureTransientStateFor(store, s => s.LogoPictureId);
				//ensure we have "/" at the end
				store.Url = store.Url.EnsureEndsWith("/");
				_storeService.InsertStore(store);

				NotifySuccess(_localizationService.GetResource("Admin.Configuration.Stores.Added"));
				return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
			}

			//If we got this far, something failed, redisplay form
			PrepareStoreModel(model, null);
			return View(model);
		}

		public ActionResult Edit(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var store = _storeService.GetStoreById(id);
			if (store == null)
				return RedirectToAction("List");

			var model = store.ToModel();
			PrepareStoreModel(model, store);

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		[FormValueRequired("save", "save-continue")]
		public ActionResult Edit(StoreModel model, bool continueEditing)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var store = _storeService.GetStoreById(model.Id);
			if (store == null)
				return RedirectToAction("List");

			if (ModelState.IsValid)
			{
				store = model.ToEntity(store);

				MediaHelper.UpdatePictureTransientStateFor(store, s => s.LogoPictureId);

				//ensure we have "/" at the end
				store.Url = store.Url.EnsureEndsWith("/");
				_storeService.UpdateStore(store);

				NotifySuccess(_localizationService.GetResource("Admin.Configuration.Stores.Updated"));
				return continueEditing ? RedirectToAction("Edit", new { id = store.Id }) : RedirectToAction("List");
			}

			//If we got this far, something failed, redisplay form
			PrepareStoreModel(model, store);
			return View(model);
		}

		[HttpPost]
		public ActionResult Delete(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageStores))
				return AccessDeniedView();

			var store = _storeService.GetStoreById(id);
			if (store == null)
				return RedirectToAction("List");

			try
			{
				_storeService.DeleteStore(store);

				//when we delete a store we should also ensure that all "per store" settings will also be deleted
				var settingsToDelete = _settingService
					.GetAllSettings()
					.Where(s => s.StoreId == id)
					.ToList();

				settingsToDelete.ForEach(x => _settingService.DeleteSetting(x));

				//when we had two stores and now have only one store, we also should delete all "per store" settings
				var allStores = _storeService.GetAllStores();

				if (allStores.Count == 1)
				{
					settingsToDelete = _settingService
						.GetAllSettings()
						.Where(s => s.StoreId == allStores[0].Id)
						.ToList();

					settingsToDelete.ForEach(x => _settingService.DeleteSetting(x));
				}

				NotifySuccess(_localizationService.GetResource("Admin.Configuration.Stores.Deleted"));
				return RedirectToAction("List");
			}
			catch (Exception exc)
			{
				NotifyError(exc);
			}
			return RedirectToAction("Edit", new { id = store.Id });
		}
	}
}
