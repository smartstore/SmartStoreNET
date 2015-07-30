using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Export;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Services;
using SmartStore.Services.ExportImport;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class ExportController : AdminControllerBase
	{
		private readonly ICommonServices _services;
		private readonly IExportService _exportService;

		public ExportController(
			ICommonServices services,
			IExportService exportService)
		{
			_services = services;
			_exportService = exportService;
		}

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			return View();
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
		{
			var model = new GridModel<ExportProfileModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
			{
				var query = _exportService.GetExportProfiles();
				var profiles = query.ToList();

				model.Total = profiles.Count;

				model.Data = profiles.Select(x =>
				{
					var profileModel = new ExportProfileModel
					{
						Name = x.Name,
						ProviderSystemName = x.ProviderSystemName,
						EntityType = x.EntityType,
						Enabled = x.Enabled,
						FileType = "",		// TODO
						SchedulingHours = x.SchedulingHours,
						LastExecution = ""	// TODO
					};

					return profileModel;
				});
			}

			return new JsonResult {	Data = model };
		}
	}
}