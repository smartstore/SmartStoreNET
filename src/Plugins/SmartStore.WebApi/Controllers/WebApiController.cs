using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Caching;
using SmartStore.WebApi.Models;
using SmartStore.WebApi.Security;
using SmartStore.WebApi.Services;
using Telerik.Web.Mvc;

namespace SmartStore.WebApi.Controllers
{
    [AdminAuthorize]
    public class WebApiController : PluginControllerBase
    {
        private readonly WebApiSettings _webApiSettings;
        private readonly IWebApiPluginService _webApiPluginService;
        private readonly AdminAreaSettings _adminAreaSettings;

        public WebApiController(
            WebApiSettings settings,
            IWebApiPluginService webApiPluginService,
            AdminAreaSettings adminAreaSettings)
        {
            _webApiSettings = settings;
            _webApiPluginService = webApiPluginService;
            _adminAreaSettings = adminAreaSettings;
        }

        [Permission(WebApiPermissions.Read)]
        public ActionResult Configure()
        {
            var model = new WebApiConfigModel();
            model.Copy(_webApiSettings, true);

            var odataUri = new Uri(Request.Url, Url.Content("~/" + WebApiGlobal.MostRecentOdataPath));
            var swaggerUri = new Uri(Request.Url, Url.Content("~/swagger/ui/index"));

            model.ApiOdataUrl = odataUri.AbsoluteUri.EnsureEndsWith("/");
            model.ApiOdataMetadataUrl = model.ApiOdataUrl + "$metadata";
            model.SwaggerUrl = swaggerUri.AbsoluteUri;

            model.GridPageSize = _adminAreaSettings.GridPageSize;

            return View(model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("savegeneralsettings")]
        [ValidateAntiForgeryToken]
        [Permission(WebApiPermissions.Update)]
        public ActionResult SaveGeneralSettings(WebApiConfigModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            model.Copy(_webApiSettings, false);
            Services.Settings.SaveSetting(_webApiSettings);

            WebApiCachingControllingData.Remove();

            return Configure();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(WebApiPermissions.Read)]
        public ActionResult GridUserData(GridCommand command)
        {
            var customerQuery = _webApiPluginService.GetCustomers();
            var cachedUsers = WebApiCachingUserData.Data().ToDictionarySafe(x => x.CustomerId, x => x);
            var yes = T("Admin.Common.Yes");
            var no = T("Admin.Common.No");

            var apiUsers = customerQuery
                .Select(x => new WebApiUserModel
                {
                    Id = x.Id,
                    Username = x.Username,
                    Email = x.Email,
                    AdminComment = x.AdminComment
                })
                .ForCommand(command);

            var pagedApiUsers = apiUsers.PagedForCommand(command).ToList();

            foreach (var user in pagedApiUsers)
            {
                if (cachedUsers.ContainsKey(user.Id))
                {
                    var cachedUser = cachedUsers[user.Id];

                    user.PublicKey = cachedUser.PublicKey;
                    user.SecretKey = cachedUser.SecretKey;
                    user.Enabled = cachedUser.Enabled;
                    user.EnabledFriendly = (cachedUser.Enabled ? yes : no);

                    if (cachedUser.LastRequest.HasValue)
                    {
                        user.LastRequestDateFriendly = cachedUser.LastRequest.Value.RelativeFormat(true, "f");
                    }
                    else
                    {
                        user.LastRequestDateFriendly = "-";
                    }
                }
            }

            var model = new GridModel<WebApiUserModel>
            {
                Data = pagedApiUsers,
                Total = apiUsers.Count()
            };

            return new JsonResult { Data = model };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(WebApiPermissions.Create)]
        public void ApiButtonCreateKeys(int customerId)
        {
            _webApiPluginService.CreateKeys(customerId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(WebApiPermissions.Delete)]
        public void ApiButtonRemoveKeys(int customerId)
        {
            _webApiPluginService.RemoveKeys(customerId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(WebApiPermissions.Update)]
        public void ApiButtonEnable(int customerId)
        {
            _webApiPluginService.EnableOrDisableUser(customerId, true);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(WebApiPermissions.Update)]
        public void ApiButtonDisable(int customerId)
        {
            _webApiPluginService.EnableOrDisableUser(customerId, false);
        }
    }
}
