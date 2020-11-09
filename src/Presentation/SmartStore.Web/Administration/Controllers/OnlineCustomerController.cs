using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Customers;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class OnlineCustomerController : AdminControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IGeoCountryLookup _geoCountryLookup;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly CustomerSettings _customerSettings;
        private readonly AdminAreaSettings _adminAreaSettings;

        public OnlineCustomerController(
            ICustomerService customerService,
            IGeoCountryLookup geoCountryLookup,
            IDateTimeHelper dateTimeHelper,
            CustomerSettings customerSettings,
            AdminAreaSettings adminAreaSettings)
        {
            _customerService = customerService;
            _geoCountryLookup = geoCountryLookup;
            _dateTimeHelper = dateTimeHelper;
            _customerSettings = customerSettings;
            _adminAreaSettings = adminAreaSettings;
        }

        [Permission(Permissions.Customer.Read)]
        public ActionResult List()
        {
            var customers = _customerService.GetOnlineCustomers(DateTime.UtcNow.AddMinutes(-_customerSettings.OnlineCustomerMinutes), null, 0, _adminAreaSettings.GridPageSize);

            var model = new GridModel<OnlineCustomerModel>
            {
                Data = customers.Select(x =>
                {
                    return new OnlineCustomerModel
                    {
                        Id = x.Id,
                        CustomerInfo = x.IsRegistered() ? x.Email : T("Admin.Customers.Guest").Text,
                        LastIpAddress = x.LastIpAddress,
                        Location = _geoCountryLookup.LookupCountry(x.LastIpAddress)?.Name.EmptyNull(),
                        LastActivityDate = _dateTimeHelper.ConvertToUserTime(x.LastActivityDateUtc, DateTimeKind.Utc),
                        LastVisitedPage = x.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage)
                    };
                }),
                Total = customers.TotalCount
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Customer.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<OnlineCustomerModel>();

            var lastActivityFrom = DateTime.UtcNow.AddMinutes(-_customerSettings.OnlineCustomerMinutes);
            var customers = _customerService.GetOnlineCustomers(lastActivityFrom, null, command.Page - 1, command.PageSize);

            model.Data = customers.Select(x =>
            {
                return new OnlineCustomerModel
                {
                    Id = x.Id,
                    CustomerInfo = x.IsRegistered() ? x.Email : T("Admin.Customers.Guest").Text,
                    LastIpAddress = x.LastIpAddress,
                    Location = _geoCountryLookup.LookupCountry(x.LastIpAddress)?.Name.EmptyNull(),
                    LastActivityDate = _dateTimeHelper.ConvertToUserTime(x.LastActivityDateUtc, DateTimeKind.Utc),
                    LastVisitedPage = x.GetAttribute<string>(SystemCustomerAttributeNames.LastVisitedPage)
                };
            });

            model.Total = customers.TotalCount;

            return new JsonResult
            {
                Data = model
            };
        }
    }
}
