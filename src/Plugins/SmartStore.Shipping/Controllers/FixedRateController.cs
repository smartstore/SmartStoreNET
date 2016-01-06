using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using SmartStore.Shipping.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Shipping;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Shipping.Controllers
{
	[AdminAuthorize]
	public class FixedRateController : PluginControllerBase
    {
        private readonly IShippingService _shippingService;
        private readonly ISettingService _settingService;

        public FixedRateController(IShippingService shippingServicee, ISettingService settingService)
        {
            this._shippingService = shippingServicee;
            this._settingService = settingService;
        }

        public ActionResult Configure()
        {
            var shippingMethods = _shippingService.GetAllShippingMethods();
            if (shippingMethods.Count == 0)
                return Content(T("Admin.Configuration.Shipping.Methods.NoMethodsLoaded"));

            var tmp = new List<FixedRateModel>();
            foreach (var shippingMethod in shippingMethods)
                tmp.Add(new FixedRateModel()
                {
                    ShippingMethodId = shippingMethod.Id,
                    ShippingMethodName = shippingMethod.Name,
                    Rate = GetShippingRate(shippingMethod.Id)
                });

            var gridModel = new GridModel<FixedRateModel>
            {
                Data = tmp,
                Total = tmp.Count
            };

            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Configure(GridCommand command)
        {
            var tmp = new List<FixedRateModel>();
            foreach (var shippingMethod in _shippingService.GetAllShippingMethods())
                tmp.Add(new FixedRateModel()
                {
                    ShippingMethodId = shippingMethod.Id,
                    ShippingMethodName = shippingMethod.Name,
                    Rate = GetShippingRate(shippingMethod.Id)
                });

            var tmp2 = tmp.ForCommand(command);
            var gridModel = new GridModel<FixedRateModel>
            {
                Data = tmp2,
                Total = tmp2.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ShippingRateUpdate(FixedRateModel model, GridCommand command)
        {
            int shippingMethodId = model.ShippingMethodId;
            decimal rate = model.Rate;

			if (shippingMethodId != 0)
			{
				_settingService.SetSetting(string.Format("ShippingRateComputationMethod.FixedRate.Rate.ShippingMethodId{0}", shippingMethodId), rate);
			}

            var tmp = new List<FixedRateModel>();
            foreach (var shippingMethod in _shippingService.GetAllShippingMethods())
                tmp.Add(new FixedRateModel()
                {
                    ShippingMethodId = shippingMethod.Id,
                    ShippingMethodName = shippingMethod.Name,
                    Rate = GetShippingRate(shippingMethod.Id)
                });

            var tmp2 = tmp.ForCommand(command);
            var gridModel = new GridModel<FixedRateModel>
            {
                Data = tmp2,
                Total = tmp2.Count()
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [NonAction]
        protected decimal GetShippingRate(int shippingMethodId)
        {
            decimal rate = this._settingService.GetSettingByKey<decimal>(string.Format("ShippingRateComputationMethod.FixedRate.Rate.ShippingMethodId{0}", shippingMethodId));
            return rate;
        }
    }
}
